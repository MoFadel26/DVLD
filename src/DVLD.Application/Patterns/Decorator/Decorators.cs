using System.Diagnostics;
using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.Extensions.Logging;

namespace DVLD.Application.Patterns.Decorator;

/// <summary>
/// Decorator Pattern: Adds structured logging to IApplicationService without altering business logic.
/// </summary>
public class LoggingApplicationServiceDecorator : IApplicationService
{
    private readonly IApplicationService _innerService;
    private readonly ILogger<LoggingApplicationServiceDecorator> _logger;

    public LoggingApplicationServiceDecorator(IApplicationService innerService, ILogger<LoggingApplicationServiceDecorator> logger)
    {
        _innerService = innerService;
        _logger = logger;
    }

    public async Task<LocalLicenseApplicationResponseDto> CreateNewLocalLicenseApplicationAsync(
        CreateNewLocalLicenseApplicationDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Decorator: Starting creation of new license application for PersonId={PersonId}, ClassId={ClassId}",
            dto.ApplicantPersonId, dto.LicenseClassId);

        try
        {
            var result = await _innerService.CreateNewLocalLicenseApplicationAsync(dto, cancellationToken);
            _logger.LogInformation("Decorator: Successfully created LocalAppId={LocalAppId}, BaseAppId={AppId}",
                result.LocalDrivingLicenseApplicationId, result.ApplicationId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decorator: Failed to create license application for PersonId={PersonId}", dto.ApplicantPersonId);
            throw;
        }
    }

    public async Task<LocalLicenseApplicationResponseDto> GetLocalApplicationByIdAsync(int localAppId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Decorator: Fetching LocalApplication #{LocalAppId}", localAppId);
        return await _innerService.GetLocalApplicationByIdAsync(localAppId, cancellationToken);
    }

    public async Task<IReadOnlyList<LocalLicenseApplicationResponseDto>> GetAllLocalApplicationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Decorator: Fetching all LocalApplications");
        return await _innerService.GetAllLocalApplicationsAsync(cancellationToken);
    }

    public async Task CancelApplicationAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Decorator: Cancelling Application #{AppId}", applicationId);
        await _innerService.CancelApplicationAsync(applicationId, cancellationToken);
        _logger.LogInformation("Decorator: Application #{AppId} cancellation completed", applicationId);
    }
}

/// <summary>
/// Decorator Pattern: Measures and benchmarks execution time for service calls.
/// </summary>
public class PerformanceApplicationServiceDecorator : IApplicationService
{
    private readonly IApplicationService _innerService;
    private readonly ILogger<PerformanceApplicationServiceDecorator> _logger;

    public PerformanceApplicationServiceDecorator(IApplicationService innerService, ILogger<PerformanceApplicationServiceDecorator> logger)
    {
        _innerService = innerService;
        _logger = logger;
    }

    public async Task<LocalLicenseApplicationResponseDto> CreateNewLocalLicenseApplicationAsync(
        CreateNewLocalLicenseApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = await _innerService.CreateNewLocalLicenseApplicationAsync(dto, cancellationToken);
        sw.Stop();

        _logger.LogInformation("Decorator [Performance]: CreateNewLocalLicenseApplication took {ElapsedMs}ms", sw.ElapsedMilliseconds);
        return result;
    }

    public async Task<LocalLicenseApplicationResponseDto> GetLocalApplicationByIdAsync(int localAppId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = await _innerService.GetLocalApplicationByIdAsync(localAppId, cancellationToken);
        sw.Stop();

        _logger.LogInformation("Decorator [Performance]: GetLocalApplicationById took {ElapsedMs}ms", sw.ElapsedMilliseconds);
        return result;
    }

    public async Task<IReadOnlyList<LocalLicenseApplicationResponseDto>> GetAllLocalApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = await _innerService.GetAllLocalApplicationsAsync(cancellationToken);
        sw.Stop();

        _logger.LogInformation("Decorator [Performance]: GetAllLocalApplications took {ElapsedMs}ms", sw.ElapsedMilliseconds);
        return result;
    }

    public async Task CancelApplicationAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        await _innerService.CancelApplicationAsync(applicationId, cancellationToken);
        sw.Stop();

        _logger.LogInformation("Decorator [Performance]: CancelApplication took {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }
}
