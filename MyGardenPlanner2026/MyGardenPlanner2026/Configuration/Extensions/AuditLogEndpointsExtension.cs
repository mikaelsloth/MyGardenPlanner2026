namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Mvc;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Security.Claims;

/// <summary>
/// Download-endpoint til AuditLog-eksport. Kører uden for Blazor-circuittet (almindelig
/// HTTP GET), og kan derfor ikke tilgå det circuit-scopede IReAuthenticationService
/// direkte — step-up-friskhed bevises i stedet via et signeret token udstedt af Blazor-
/// komponenten efter StepUpGuard er bestået (se IAuditLogExportTokenService).
/// </summary>
public static class AuditLogEndpointsExtension
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/admin/audit-log")
            .RequireAuthorization(AuthorizationServicesExtensions.RequireAuditViewerPolicy);

        group.MapGet("/export", async (
            HttpContext context,
            [FromServices] IAuditLogQueryService queryService,
            [FromServices] IAuditLogExportService exportService,
            [FromServices] IAuditLogExportTokenService tokenService,
            [FromServices] IAdminActionRateLimiter rateLimiter,
            [FromQuery] string format,
            [FromQuery] string token,
            [FromQuery] string? entityName,
            [FromQuery] string? entityId,
            [FromQuery] string? userId,
            [FromQuery] string? userEmail,
            [FromQuery] AuditAction? action,
            [FromQuery] DateTimeOffset? fromUtc,
            [FromQuery] DateTimeOffset? toUtc,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Results.Forbid();
            }

            if (!tokenService.TryValidateToken(token, currentUserId, out var tokenError))
            {
                return Results.Problem(
                    detail: $"Eksport-token er ugyldig eller udløbet: {tokenError} " +
                        "Bekræft din identitet igen og prøv eksporten forfra.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            if (!await rateLimiter.TryAcquireAsync(currentUserId, cancellationToken))
            {
                return Results.Problem(
                    detail: "For mange handlinger på kort tid. Vent et øjeblik og prøv igen.",
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            if (!Enum.TryParse<AuditLogExportFormat>(format, ignoreCase: true, out var exportFormat))
            {
                return Results.Problem(
                    detail: $"Ukendt eksportformat '{format}'. Gyldige værdier: Csv, Json, Xlsx.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var filter = new AuditLogFilterDto(entityName, entityId, userId, userEmail, action, fromUtc, toUtc);

            var totalCount = await queryService.CountAsync(filter, cancellationToken);
            if (totalCount > IAuditLogExportService.HardRowLimit)
            {
                return Results.Problem(
                    detail: $"Eksporten omfatter {totalCount} rækker, hvilket overstiger den " +
                        $"tilladte grænse på {IAuditLogExportService.HardRowLimit}. Indsnævr filteret og prøv igen.",
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            var memoryStream = new MemoryStream();
            await using (memoryStream.ConfigureAwait(false))
            {
                await exportService.ExportAsync(filter, exportFormat, memoryStream, cancellationToken);

                var (contentType, fileExtension) = exportFormat switch
                {
                    AuditLogExportFormat.Csv => ("text/csv", "csv"),
                    AuditLogExportFormat.Json => ("application/json", "json"),
                    AuditLogExportFormat.Xlsx =>
                        ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
                    _ => throw new ArgumentOutOfRangeException(nameof(format))
                };

                var fileName = $"audit-log-export-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.{fileExtension}";

                return Results.File(memoryStream.ToArray(), contentType, fileName);
            }
        });

        return endpoints;
    }
}