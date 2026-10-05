# Driving & Vehicle Licensing Department (DVLD)
## Design Patterns Catalog & Architectural Guide

This project was built to deliberately practice and showcase classic **Gang of Four (GoF)** and **Enterprise Architectural Design Patterns** in a clean, readable, and idiomatic C# (.NET 10) implementation.

Below is the detailed documentation for every pattern applied in this system. For each pattern, we specify:
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
    - `PersonExistsValidationHandler`: Verifies the applicant person exists in the database.
    - `MinimumAgeValidationHandler`: Verifies the applicant's age matches or exceeds `LicenseClass.MinimumAllowedAge`.
    - `NoActiveLicenseOfSameClassValidationHandler`: Verifies the applicant does not already hold an active license of the same class.
    - `NoPendingApplicationOfSameClassValidationHandler`: Verifies the applicant does not have an active pending (`New`) application of the same class.
    - `NewApplicationValidationPipeline`: Chains the handlers together.

- **Why It Fits This Specific Case**:
  Submitting a new local driving license application requires a strict sequence of business rule validations before any records are created or fees are processed. If any validation fails, the pipeline immediately halts and throws an informative domain exception (`AgeRequirementNotMetException`, `ActiveLicenseAlreadyExistsException`, etc.). If a check passes, execution automatically forwards to the successor in the chain.

- **Simpler Alternative**:
  A single monolithic method containing consecutive `if` statements querying the repositories one after another:
  ```csharp
  if (person == null) throw new EntityNotFoundException(...);
  if (person.GetAge() < licenseClass.MinimumAllowedAge) throw new AgeRequirementNotMetException(...);
  if (hasActiveLicense) throw new ActiveLicenseAlreadyExistsException(...);
  if (hasPendingApp) throw new PendingApplicationAlreadyExistsException(...);
  ```

- **Trade-Offs**:
  - **Pros**: Adheres strictly to the **Single Responsibility Principle** (each handler evaluates exactly one business rule) and the **Open/Closed Principle** (adding new checks, such as criminal record or medical fitness, requires only creating a new handler and inserting it into the chain without touching existing handlers). Each handler can be independently unit tested in isolation.
  - **Cons**: Requires more classes and boilerplate interfaces than a linear validation method.

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
  An `Application` in DVLD transitions through distinct lifecycle stages (`New`, `Cancelled`, `Completed`). Key business operations (scheduling vision/theory/practical tests, issuing licenses, or cancelling) are strictly dependent on the current state. The State pattern allows the application object to alter its behavior when its internal status changes.

- **Simpler Alternative**:
  Using a plain integer/enum property `ApplicationStatus` and littering service methods with defensive enum checks:
  ```csharp
  if (app.ApplicationStatus == EnApplicationStatus.Completed)
      throw new DomainException("Application is already completed.");
  if (app.ApplicationStatus == EnApplicationStatus.Cancelled)
      throw new DomainException("Cannot perform action on cancelled application.");
  ```

- **Trade-Offs**:
  - **Pros**: Encapsulates state-dependent transitions and behavior directly within state classes, eliminating fragile conditional logic spread throughout multiple service layers.
  - **Cons**: Adds classes and allocations for state objects where a simple enum check would suffice for trivial CRUD systems.

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
  DVLD offers 7 distinct service types, each governed by different fee calculation formulas. The Strategy pattern defines a family of pricing algorithms, encapsulates each one in a separate class, and makes them interchangeable at runtime.

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
  - **Pros**: Eliminates monolithic switch statements and makes each pricing algorithm open for modification or replacement (e.g. promotional discounts, regional surcharges) without modifying calling code.
  - **Cons**: Requires defining multiple strategy classes, interfaces, and registering them into the dependency injection container.

---

### 4. Factory Method / Abstract Factory Pattern (Creational)

- **Location**: `DVLD.Application/Patterns/Factory/`
  - Interface: `ILicenseFactory`
  - Concrete Factories:
    - `FirstTimeLicenseFactory`: Issues initial license with validity corresponding to `LicenseClass.ValidityLength` (5 or 10 years), sets `IssueReason = FirstTime`.
    - `RenewLicenseFactory`: Deactivates previous active license, creates new license with renewed validity period, sets `IssueReason = Renew`.
    - `LostReplacementLicenseFactory`: Deactivates old lost license, creates replacement retaining the original expiration date, sets `IssueReason = ReplacementForLost`.
    - `DamagedReplacementLicenseFactory`: Deactivates old damaged license, creates replacement retaining the original expiration date, sets `IssueReason = ReplacementForDamaged`.
  - Provider: `LicenseFactoryProvider` (`ILicenseFactoryProvider`)

- **Why It Fits This Specific Case**:
  Issuing a license varies dramatically depending on whether it is an initial issuance, a renewal, or a replacement for loss/damage. Each variant has different validity calculations (e.g., fresh 10-year term vs preserving previous expiry date) and deactivation side effects. The Factory Method pattern decouples the licensing service from the specific creation mechanisms.

- **Simpler Alternative**:
  Instantiating and populating `new License { ... }` directly in each endpoint method or passing flags into a shared creation function.

- **Trade-Offs**:
  - **Pros**: Keeps license creation logic cohesive and isolated per business scenario. Adding new issuance types (e.g., Temporary Permits, Military Conversions) requires only implementing a new `ILicenseFactory`.
  - **Cons**: Introduces extra abstraction layers, context objects (`LicenseCreationContext`), and factory registry wiring.

---

### 5. Template Method Pattern (Behavioral)

- **Location**: `DVLD.Application/Patterns/TemplateMethod/`
  - Base: `BaseTestWorkflow` (`ITestWorkflow`)
  - Template Methods:
    - `ScheduleAppointmentAsync`: Defines the 6-step appointment scheduling algorithm (App status check -> Prerequisite check -> Already passed check -> Open appointment check -> Fee calculation -> Build & Persist).
    - `RecordTestResultAsync`: Defines the test execution algorithm (Lock appointment -> Record Pass/Fail -> Execute post-result hooks).
  - Concrete Subclasses:
    - `VisionTestWorkflow`: Overrides `ValidatePrerequisitesAsync` (No prerequisite needed).
    - `TheoryTestWorkflow`: Overrides `ValidatePrerequisitesAsync` (Guarantees Vision test is passed).
    - `PracticalTestWorkflow`: Overrides `ValidatePrerequisitesAsync` (Guarantees Theory test is passed).
  - Resolver: `TestWorkflowResolver`

- **Why It Fits This Specific Case**:
  DVLD enforces an invariant sequence: Vision Test -> Theory Test -> Practical Test. The 3 test types share the exact same appointment scheduling and score recording algorithm, but differ in prerequisite validation and fees. The Template Method defines the skeleton of the algorithm in the base class, ensuring that the critical sequence is strictly enforced.

- **Simpler Alternative**:
  Three separate methods in `TestService` (`ScheduleVisionTest`, `ScheduleTheoryTest`, `SchedulePracticalTest`) duplicating the validation, lock checks, fee retrieval, and appointment creation.

- **Trade-Offs**:
  - **Pros**: Prevents code duplication across the 3 test stages and mathematically prevents skipping prerequisite tests.
  - **Cons**: Subclasses are tightly coupled to the abstract base class via inheritance.

---

### 6. Builder Pattern (Creational)

- **Location**: `DVLD.Application/Patterns/Builder/`
  - Builders:
    - `LicenseBuilder`: Step-by-step construction of `License` entities. Validates foreign keys, positive fees, and ensures `ExpirationDate > IssueDate`.
    - `TestAppointmentBuilder`: Step-by-step construction of `TestAppointment` entities. Validates mandatory local app and test type identifiers.

- **Why It Fits This Specific Case**:
  `License` and `TestAppointment` are complex entities with numerous mandatory and optional attributes (ApplicationId, DriverId, LicenseClassId, IssueDate, ExpirationDate, Notes, Fees, IsActive, IssueReason, CreatedByUserId). The Builder pattern allows construction with clear, readable method chaining while guaranteeing that an invalid object cannot be built.

- **Simpler Alternative**:
  A massive 10-parameter constructor `new License(appId, driverId, classId, ...)` or raw object initializers where required fields can easily be omitted or set incorrectly.

- **Trade-Offs**:
  - **Pros**: Highly expressive fluent API, protects domain invariants, and avoids telescoping constructors.
  - **Cons**: Extra code to maintain for builder classes.

---

### 7. Observer Pattern / Domain Event Dispatcher (Behavioral)

- **Location**: `DVLD.Domain/Patterns/Events/` & `DVLD.Application/Patterns/Observer/`
  - Domain Events: `IDomainEvent`, `LicenseIssuedEvent`, `TestPassedEvent`, `TestFailedEvent`, `ApplicationStatusChangedEvent`.
  - Dispatcher: `IDomainEventDispatcher`, `DomainEventDispatcher`.
  - Observers / Handlers:
    - `LicenseIssuedEventHandler`: Logs license issuance and driver records.
    - `TestPassedEventHandler`: Logs successful test passes and milestones.
    - `TestFailedEventHandler`: Logs test failures and alerts to retake requirement.
    - `ApplicationStatusChangedEventHandler`: Audit logging of lifecycle transitions.

- **Why It Fits This Specific Case**:
  When domain state mutations occur (e.g. issuing a license or failing a test), secondary side effects (audit trails, notification triggers, analytics) should occur without polluting the primary transactional business logic. The Observer pattern allows decoupled subscribers to react to events.

- **Simpler Alternative**:
  Directly invoking audit logging and notification helper methods inside the main service methods.

- **Trade-Offs**:
  - **Pros**: High cohesion, low coupling, and the ability to add new observers (such as SMS/Email dispatchers) without touching the core licensing logic.
  - **Cons**: Indirection in program flow; errors in asynchronous event handlers must be explicitly handled so as not to break primary workflows.

---

### 8. Decorator Pattern (Structural)

- **Location**: `DVLD.Application/Patterns/Decorator/`
  - Interface: `IApplicationService`
  - Core Service: `ApplicationService`
  - Decorators:
    - `LoggingApplicationServiceDecorator`: Intercepts calls to log parameters, success, and error details.
    - `PerformanceApplicationServiceDecorator`: Measures execution time with high-resolution `Stopwatch` and logs execution latency.

- **Why It Fits This Specific Case**:
  Cross-cutting concerns like structured logging and performance metrics are required across application services. The Decorator pattern dynamically wraps `IApplicationService` in the dependency injection pipeline without modifying `ApplicationService`.

- **Simpler Alternative**:
  Writing `_logger.LogInformation(...)` and `Stopwatch.StartNew()` directly inside every method of `ApplicationService`.

- **Trade-Offs**:
  - **Pros**: Preserves the Single Responsibility Principle of the business service and allows decorators to be added, reordered, or removed via DI configuration.
  - **Cons**: Requires interface duplication and forwarding boilerplate for every method in the decorated interface.

---

### 9. Repository & Unit of Work Patterns (Architectural / Structural)

- **Location**: `DVLD.Application/Common/Interfaces/IPersistence.cs` & `DVLD.Infrastructure/Repositories/`
  - Generic: `IRepository<T>`, `Repository<T>`
  - Specialized: `IPersonRepository`, `IApplicationRepository`, `ILocalDrivingLicenseApplicationRepository`, `ILicenseClassRepository`, `ITestAppointmentRepository`, `ITestResultRepository`, `IDriverRepository`, `ILicenseRepository`, `IInternationalLicenseRepository`, `IDetainedLicenseRepository`.
  - Unit of Work: `IUnitOfWork`, `UnitOfWork`.

- **Why It Fits This Specific Case**:
  Decouples the business logic (`DVLD.Application`) from Entity Framework Core and PostgreSQL. It simplifies unit testing by allowing repositories to be mocked or backed by in-memory databases, and ensures atomic database transactions across multiple aggregates.

- **Simpler Alternative**:
  Injecting `DvldDbContext` directly into API controllers or application services.

- **Trade-Offs**:
  - **Pros**: Clear architectural boundary, highly testable code, and centralized persistence logic.
  - **Cons**: Extra abstraction layer over EF Core, which already implements Repository and Unit of Work patterns natively.
