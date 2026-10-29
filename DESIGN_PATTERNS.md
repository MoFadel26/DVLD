# Driving & Vehicle Licensing Department (DVLD)
## Design Patterns Catalog & Architectural Guide

This project uses the following **Gang of Four (GoF)** and **Enterprise Architectural Design Patterns** in C# (.NET 10).

Each pattern below lists:
1. **Pattern Name & Category**
2. **Where and How It Is Applied in DVLD**
3. **Why It Fits This Specific Use Case**
4. **What a Simpler Alternative Would Have Looked Like**
5. **The Trade-Offs Incurred**

---

### 1. Chain of Responsibility Pattern (Behavioral)

- **Location**: `DVLD.Application/Patterns/ChainOfResponsibility/`
  - Interfaces: `IValidationHandler<TRequest>`, `INewApplicationValidationPipeline`
  - Base: `AbstractValidationHandler<TRequest>`
  - Handlers:
    - `PersonExistsValidationHandler`: Checks that the applicant person exists in the database.
    - `MinimumAgeValidationHandler`: Checks that the applicant's age is at least `LicenseClass.MinimumAllowedAge`.
    - `NoActiveLicenseOfSameClassValidationHandler`: Checks that the applicant does not already hold an active license of the same class.
    - `NoPendingApplicationOfSameClassValidationHandler`: Checks that the applicant does not have a pending (`New`) application of the same class.
    - `NewApplicationValidationPipeline`: Links the handlers in the order above.

- **Why It Fits This Specific Case**:
  A new local driving license application must pass four business rule checks, in order, before the fee is calculated and any records are created. If a check fails, its handler throws a domain exception (`AgeRequirementNotMetException`, `ActiveLicenseAlreadyExistsException`, etc.) and the chain stops. If a check passes, the handler calls the next handler in the chain.

- **Simpler Alternative**:
  One method with consecutive `if` statements that query the repositories one after another:
  ```csharp
  if (person == null) throw new EntityNotFoundException(...);
  if (person.GetAge() < licenseClass.MinimumAllowedAge) throw new AgeRequirementNotMetException(...);
  if (hasActiveLicense) throw new ActiveLicenseAlreadyExistsException(...);
  if (hasPendingApp) throw new PendingApplicationAlreadyExistsException(...);
  ```

- **Trade-Offs**:
  - **Pros**: Follows the **Single Responsibility Principle** (each handler checks one business rule) and the **Open/Closed Principle** (a new check, such as criminal record or medical fitness, is a new handler added to the chain; existing handlers do not change). Each handler can be unit tested on its own.
  - **Cons**: More classes and interfaces than a single validation method.

---

### 2. State Pattern (Behavioral)

- **Location**: `DVLD.Domain/Patterns/State/` & `DVLD.Domain/Entities/Application.cs`
  - Interface: `IApplicationState`
  - Concrete States:
    - `NewApplicationState`: Allows `Cancel()`, `Complete()`, `CanScheduleTest() => true`, `CanIssueLicense() => true`.
    - `CancelledApplicationState`: Rejects further cancellation or completion (`InvalidApplicationStateTransitionException`), disallows scheduling tests or issuing licenses.
    - `CompletedApplicationState`: Rejects cancellation or re-completion, disallows scheduling tests or issuing licenses.
  - Factory: `ApplicationStateFactory`

- **Why It Fits This Specific Case**:
  An `Application` in DVLD has three statuses: `New`, `Cancelled`, `Completed`. Whether it can be cancelled, have tests scheduled (vision, theory, practical), or have a license issued depends on the current status. `Application.CurrentState` returns the state object for the current status, and that object decides what is allowed.

- **Simpler Alternative**:
  A plain integer/enum property `ApplicationStatus` with enum checks repeated in each service method:
  ```csharp
  if (app.ApplicationStatus == EnApplicationStatus.Completed)
      throw new DomainException("Application is already completed.");
  if (app.ApplicationStatus == EnApplicationStatus.Cancelled)
      throw new DomainException("Cannot perform action on cancelled application.");
  ```

- **Trade-Offs**:
  - **Pros**: Status rules and transitions live in the state classes instead of being repeated as conditionals across several services.
  - **Cons**: Adds classes and state object allocations where a simple enum check would be enough for basic CRUD.

---

### 3. Strategy Pattern (Behavioral)

- **Location**: `DVLD.Application/Patterns/Strategy/`
  - Interface: `IFeeCalculationStrategy`
  - Concrete Strategies:
    - `NewLicenseFeeStrategy`: Base Application Fee ($5) + License Class Fee ($15 to $300).
    - `RetakeTestFeeStrategy`: Base Retake Fee ($5) + Test Type Fee ($10 / $20 / $30).
    - `RenewLicenseFeeStrategy`: Base Application Fee ($5) + License Class Fee.
    - `ReplaceLostFeeStrategy`: Base Application Fee ($5) + Lost Surcharge ($10).
    - `ReplaceDamagedFeeStrategy`: Base Application Fee ($5) + Damaged Surcharge ($5).
    - `ReleaseDetainedFeeStrategy`: Base Application Fee ($5) + Impoundment Fine Fee.
    - `InternationalLicenseFeeStrategy`: Base Application Fee ($5) + International License Fee ($50).
  - Context: `FeeCalculationContext` (`IFeeCalculationContext`)

- **Why It Fits This Specific Case**:
  DVLD has 7 application types, and each one has its own fee formula. Each formula is in its own strategy class. `FeeCalculationContext` picks the strategy that matches the request's application type.

- **Simpler Alternative**:
  A single method with a hardcoded `switch` expression:
  ```csharp
  decimal total = appType switch {
      EnApplicationType.NewDrivingLicense => 5m + classFee,
      EnApplicationType.RetakeTest => 5m + testFee,
      ...
  };
  ```

- **Trade-Offs**:
  - **Pros**: No large switch statement. One fee formula can be changed or replaced (for example, to add a discount or a regional surcharge) without changing the calling code.
  - **Cons**: Requires multiple strategy classes, an interface, and registering each strategy in the dependency injection container.

---

### 4. Factory Method / Abstract Factory Pattern (Creational)

- **Location**: `DVLD.Application/Patterns/Factory/`
  - Interface: `ILicenseFactory`
  - Concrete Factories:
    - `FirstTimeLicenseFactory`: Issues the first license with validity from `LicenseClass.ValidityLength` (5 or 10 years), sets `IssueReason = FirstTime`.
    - `RenewLicenseFactory`: Deactivates the previous active license, creates a new license with a new validity period, sets `IssueReason = Renew`.
    - `LostReplacementLicenseFactory`: Deactivates the old lost license, creates a replacement with the original expiration date, sets `IssueReason = ReplacementForLost`.
    - `DamagedReplacementLicenseFactory`: Deactivates the old damaged license, creates a replacement with the original expiration date, sets `IssueReason = ReplacementForDamaged`.
  - Provider: `LicenseFactoryProvider` (`ILicenseFactoryProvider`)

- **Why It Fits This Specific Case**:
  Issuing a license works differently for a first license, a renewal, and a replacement for a lost or damaged license. Each case sets the expiration date differently (a new term of `ValidityLength` years, or the previous license's expiration date) and some deactivate the previous license. The licensing service asks `LicenseFactoryProvider` for the factory that matches the issue reason and does not need to know how each license is built.

- **Simpler Alternative**:
  Creating and filling `new License { ... }` directly in each endpoint method, or passing flags into a shared creation function.

- **Trade-Offs**:
  - **Pros**: License creation logic for each case is in one class. A new issue type (for example, temporary permits or military conversions) needs only a new `ILicenseFactory`.
  - **Cons**: Adds abstraction layers, a context object (`LicenseCreationContext`), and factory registration.

---

### 5. Template Method Pattern (Behavioral)

- **Location**: `DVLD.Application/Patterns/TemplateMethod/`
  - Base: `BaseTestWorkflow` (`ITestWorkflow`)
  - Template Methods:
    - `ScheduleAppointmentAsync`: Defines the 6-step appointment scheduling algorithm (App status check -> Prerequisite check -> Already passed check -> Open appointment check -> Fee calculation -> Build & Persist).
    - `RecordTestResultAsync`: Defines the test result algorithm (Lock appointment -> Record Pass/Fail -> Run post-result hook).
  - Concrete Subclasses:
    - `VisionTestWorkflow`: Overrides `ValidatePrerequisitesAsync` (No prerequisite needed).
    - `TheoryTestWorkflow`: Overrides `ValidatePrerequisitesAsync` (Requires a passed Vision test).
    - `PracticalTestWorkflow`: Overrides `ValidatePrerequisitesAsync` (Requires a passed Theory test).
  - Resolver: `TestWorkflowResolver`

- **Why It Fits This Specific Case**:
  DVLD requires tests in a fixed order: Vision Test -> Theory Test -> Practical Test. The 3 test types use the same appointment scheduling and result recording steps. They differ only in the prerequisite check; the fee is read from each test type's record. The base class defines the steps, and each subclass supplies its prerequisite check.

- **Simpler Alternative**:
  Three separate methods in `TestService` (`ScheduleVisionTest`, `ScheduleTheoryTest`, `SchedulePracticalTest`) that each repeat the validation, lock checks, fee lookup, and appointment creation.

- **Trade-Offs**:
  - **Pros**: No duplicated code across the 3 test stages, and a test cannot be scheduled before its prerequisite test is passed.
  - **Cons**: Subclasses are tightly coupled to the abstract base class via inheritance.

---

### 6. Builder Pattern (Creational)

- **Location**: `DVLD.Application/Patterns/Builder/`
  - Builders:
    - `LicenseBuilder`: Builds `License` entities step by step. `Build()` checks that `ApplicationId`, `DriverId`, and `LicenseClassId` are positive and that `ExpirationDate > IssueDate`.
    - `TestAppointmentBuilder`: Builds `TestAppointment` entities step by step. `Build()` checks that `TestTypeId` and `LocalDrivingLicenseApplicationId` are positive.

- **Why It Fits This Specific Case**:
  `License` and `TestAppointment` have many required and optional fields (ApplicationId, DriverId, LicenseClassId, IssueDate, ExpirationDate, Notes, Fees, IsActive, IssueReason, CreatedByUserId). The builder sets them through chained method calls, and `Build()` throws if the checks above fail.

- **Simpler Alternative**:
  A 10-parameter constructor `new License(appId, driverId, classId, ...)` or raw object initializers where required fields are easy to leave out or set wrong.

- **Trade-Offs**:
  - **Pros**: Readable chained calls, invalid IDs and dates are rejected in `Build()`, and no long constructors.
  - **Cons**: Extra code to maintain for builder classes.

---

### 7. Observer Pattern / Domain Event Dispatcher (Behavioral)

- **Location**: `DVLD.Domain/Patterns/Events/` & `DVLD.Application/Patterns/Observer/`
  - Domain Events: `IDomainEvent`, `LicenseIssuedEvent`, `TestPassedEvent`, `TestFailedEvent`, `ApplicationStatusChangedEvent`.
  - Dispatcher: `IDomainEventDispatcher`, `DomainEventDispatcher`.
  - Observers / Handlers:
    - `LicenseIssuedEventHandler`: Logs the issued license, driver, license class, and issue reason.
    - `TestPassedEventHandler`: Logs a passed test.
    - `TestFailedEventHandler`: Logs a warning for a failed test and that a retake is required.
    - `ApplicationStatusChangedEventHandler`: Logs the old and new application status.

- **Why It Fits This Specific Case**:
  When a service changes domain state (for example, issues a license or records a failed test), it dispatches an event. Side effects such as logging run in separate handlers instead of inside the main business logic.

- **Simpler Alternative**:
  Calling logging and notification helper methods directly inside the main service methods.

- **Trade-Offs**:
  - **Pros**: Services do not depend on the handlers. A new observer (such as an SMS or email sender) can be added without changing the licensing logic.
  - **Cons**: Program flow is harder to follow. A handler error must not break the main operation, so `DomainEventDispatcher` catches and logs exceptions from each handler.

---

### 8. Decorator Pattern (Structural)

- **Location**: `DVLD.Application/Patterns/Decorator/`
  - Interface: `IApplicationService`
  - Core Service: `ApplicationService`
  - Decorators:
    - `LoggingApplicationServiceDecorator`: Logs each call's parameters, success, and errors.
    - `PerformanceApplicationServiceDecorator`: Measures execution time with `Stopwatch` and logs it.

- **Why It Fits This Specific Case**:
  Logging and timing are needed around application service calls. The dependency injection setup wraps `ApplicationService` in `PerformanceApplicationServiceDecorator`, then in `LoggingApplicationServiceDecorator`, without changing `ApplicationService`.

- **Simpler Alternative**:
  Writing `_logger.LogInformation(...)` and `Stopwatch.StartNew()` directly inside every method of `ApplicationService`.

- **Trade-Offs**:
  - **Pros**: `ApplicationService` contains only business logic. Decorators can be added, reordered, or removed in the DI configuration.
  - **Cons**: Each decorator must implement every method of the interface and forward the call.

---

### 9. Repository & Unit of Work Patterns (Architectural / Structural)

- **Location**: `DVLD.Application/Common/Interfaces/IPersistence.cs` & `DVLD.Infrastructure/Repositories/`
  - Generic: `IRepository<T>`, `Repository<T>`
  - Specialized: `IPersonRepository`, `IApplicationRepository`, `ILocalDrivingLicenseApplicationRepository`, `ILicenseClassRepository`, `ITestAppointmentRepository`, `ITestResultRepository`, `IDriverRepository`, `ILicenseRepository`, `IInternationalLicenseRepository`, `IDetainedLicenseRepository`.
  - Unit of Work: `IUnitOfWork`, `UnitOfWork`.

- **Why It Fits This Specific Case**:
  The business logic (`DVLD.Application`) does not depend on Entity Framework Core or PostgreSQL. Tests can use mocked repositories or an in-memory database. All repositories share one `DvldDbContext` through `UnitOfWork`, so one `SaveChangesAsync` call saves pending changes from all of them together.

- **Simpler Alternative**:
  Injecting `DvldDbContext` directly into API controllers or application services.

- **Trade-Offs**:
  - **Pros**: Clear boundary between business logic and persistence, easier testing, and persistence code in one place.
  - **Cons**: Extra abstraction layer over EF Core, which already implements Repository and Unit of Work patterns natively.
