using DVLD.Application.Patterns.ChainOfResponsibility;
using DVLD.Application.Patterns.Decorator;
using DVLD.Application.Patterns.Factory;
using DVLD.Application.Patterns.Observer;
using DVLD.Application.Patterns.Strategy;
using DVLD.Application.Patterns.TemplateMethod;
using DVLD.Application.Services;
using DVLD.Domain.Patterns.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DVLD.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 1. Chain of Responsibility
        services.AddTransient<PersonExistsValidationHandler>();
        services.AddTransient<MinimumAgeValidationHandler>();
        services.AddTransient<NoActiveLicenseOfSameClassValidationHandler>();
        services.AddTransient<NoPendingApplicationOfSameClassValidationHandler>();
        services.AddTransient<INewApplicationValidationPipeline, NewApplicationValidationPipeline>();

        // 2. Strategy Pattern
        services.AddTransient<IFeeCalculationStrategy, NewLicenseFeeStrategy>();
        services.AddTransient<IFeeCalculationStrategy, RetakeTestFeeStrategy>();
        services.AddTransient<IFeeCalculationStrategy, RenewLicenseFeeStrategy>();
        services.AddTransient<IFeeCalculationStrategy, ReplaceLostFeeStrategy>();
        services.AddTransient<IFeeCalculationStrategy, ReplaceDamagedFeeStrategy>();
        services.AddTransient<IFeeCalculationStrategy, ReleaseDetainedFeeStrategy>();
        services.AddTransient<IFeeCalculationStrategy, InternationalLicenseFeeStrategy>();
        services.AddTransient<IFeeCalculationContext, FeeCalculationContext>();

        // 3. Factory Method / Abstract Factory Pattern
        services.AddTransient<ILicenseFactory, FirstTimeLicenseFactory>();
        services.AddTransient<ILicenseFactory, RenewLicenseFactory>();
        services.AddTransient<ILicenseFactory, LostReplacementLicenseFactory>();
        services.AddTransient<ILicenseFactory, DamagedReplacementLicenseFactory>();
        services.AddTransient<ILicenseFactoryProvider, LicenseFactoryProvider>();

        // 4. Template Method Pattern
        services.AddTransient<ITestWorkflow, VisionTestWorkflow>();
        services.AddTransient<ITestWorkflow, TheoryTestWorkflow>();
        services.AddTransient<ITestWorkflow, PracticalTestWorkflow>();
        services.AddTransient<ITestWorkflowResolver, TestWorkflowResolver>();

        // 5. Observer Pattern / Domain Events
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<LicenseIssuedEvent>, LicenseIssuedEventHandler>();
        services.AddScoped<IDomainEventHandler<TestPassedEvent>, TestPassedEventHandler>();
        services.AddScoped<IDomainEventHandler<TestFailedEvent>, TestFailedEventHandler>();
        services.AddScoped<IDomainEventHandler<ApplicationStatusChangedEvent>, ApplicationStatusChangedEventHandler>();

        // 6. Application Services with Decorator Pattern
        // Core service registration
        services.AddScoped<ApplicationService>();
        // Decorated IApplicationService: Base -> Performance -> Logging
        services.AddScoped<IApplicationService>(sp =>
        {
            var baseService = sp.GetRequiredService<ApplicationService>();
            var perfLogger = sp.GetRequiredService<ILogger<PerformanceApplicationServiceDecorator>>();
            var performanceDecorator = new PerformanceApplicationServiceDecorator(baseService, perfLogger);
            var logLogger = sp.GetRequiredService<ILogger<LoggingApplicationServiceDecorator>>();
            return new LoggingApplicationServiceDecorator(performanceDecorator, logLogger);
        });

        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<ITestService, TestService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<ILicenseClassService, LicenseClassService>();

        return services;
    }
}
