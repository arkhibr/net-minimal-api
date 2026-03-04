namespace Pix.MockServer.Security;

public interface IMockSecurityValidator
{
    bool TryValidate(HttpContext context, out IResult? failureResult, out string correlationId);
}

public sealed class MockSecurityValidator(ITokenService tokenService) : IMockSecurityValidator
{
    private const string MtlsHeaderName = "X-MTLS-Client-Cert";

    public bool TryValidate(HttpContext context, out IResult? failureResult, out string correlationId)
    {
        correlationId = EnsureCorrelationId(context);

        if (!context.Request.Headers.TryGetValue(MtlsHeaderName, out var certHeader) || string.IsNullOrWhiteSpace(certHeader))
        {
            failureResult = Results.Problem(
                title: "mTLS simulado ausente",
                detail: $"Header obrigatório '{MtlsHeaderName}' não foi informado.",
                statusCode: StatusCodes.Status403Forbidden,
                extensions: BuildExtensions(context, correlationId));
            return false;
        }

        var authorization = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            failureResult = Results.Problem(
                title: "Token ausente",
                detail: "Forneça um token Bearer válido.",
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: BuildExtensions(context, correlationId));
            return false;
        }

        var token = authorization["Bearer ".Length..].Trim();
        if (!tokenService.ValidateToken(token))
        {
            failureResult = Results.Problem(
                title: "Token inválido",
                detail: "O token informado está inválido ou expirado.",
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: BuildExtensions(context, correlationId));
            return false;
        }

        failureResult = null;
        return true;
    }

    private static string EnsureCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        correlationId = Guid.NewGuid().ToString("N");
        context.Request.Headers["X-Correlation-Id"] = correlationId;
        return correlationId;
    }

    private static Dictionary<string, object?> BuildExtensions(HttpContext context, string correlationId)
        => new()
        {
            ["traceId"] = context.TraceIdentifier,
            ["correlationId"] = correlationId
        };
}
