using System.Net;
using System.Text.Json;
using DVLD.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled domain or application exception: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            EntityNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            AgeRequirementNotMetException => (HttpStatusCode.BadRequest, "Age Requirement Not Met"),
            ActiveLicenseAlreadyExistsException => (HttpStatusCode.Conflict, "Active License Already Exists"),
            PendingApplicationAlreadyExistsException => (HttpStatusCode.Conflict, "Pending Application Already Exists"),
            InvalidApplicationStateTransitionException => (HttpStatusCode.Conflict, "Invalid State Transition"),
            PrerequisiteTestNotPassedTestException => (HttpStatusCode.BadRequest, "Prerequisite Test Not Passed"),
            DomainException => (HttpStatusCode.BadRequest, "Domain Business Rule Violation"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid Argument"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected server error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        string json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
