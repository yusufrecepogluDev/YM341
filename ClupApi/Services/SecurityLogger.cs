namespace ClupApi.Services;


public class SecurityLogger : ISecurityLogger
{
    private readonly ILogger<SecurityLogger> _logger;

    public SecurityLogger(ILogger<SecurityLogger> logger)
    {
        _logger = logger;
    }

    public void LogFailedLogin(string identifier, string ipAddress)
    {
        // Mask identifier to avoid logging sensitive data
        var maskedIdentifier = MaskIdentifier(identifier);
        _logger.LogWarning(
            "Security Event: Failed login attempt. Identifier: {Identifier}, IP: {IpAddress}, Time: {Time}",
            maskedIdentifier, ipAddress, DateTime.UtcNow);
    }

    public void LogRateLimitExceeded(string ipAddress, string endpoint)
    {
        _logger.LogWarning(
            "Security Event: Rate limit exceeded. IP: {IpAddress}, Endpoint: {Endpoint}, Time: {Time}",
            ipAddress, endpoint, DateTime.UtcNow);
    }

    public void LogAuthorizationFailure(int userId, string resource, string action)
    {
        _logger.LogWarning(
            "Security Event: Authorization failure. UserId: {UserId}, Resource: {Resource}, Action: {Action}, Time: {Time}",
            userId, resource, action, DateTime.UtcNow);
    }

    public void LogSuspiciousInput(string ipAddress, string input, string endpoint)
    {
        // Never log the actual suspicious input - could be used for log injection
        var inputLength = input?.Length ?? 0;
        var inputPreview = inputLength > 0 ? $"[{inputLength} chars]" : "[empty]";
        
        _logger.LogWarning(
            "Security Event: Suspicious input detected. IP: {IpAddress}, Endpoint: {Endpoint}, InputSize: {InputPreview}, Time: {Time}",
            ipAddress, endpoint, inputPreview, DateTime.UtcNow);
    }

    public void LogSuccessfulLogin(string identifier, string ipAddress, string userType)
    {
        var maskedIdentifier = MaskIdentifier(identifier);
        _logger.LogInformation(
            "Security Event: Successful login. Identifier: {Identifier}, UserType: {UserType}, IP: {IpAddress}, Time: {Time}",
            maskedIdentifier, userType, ipAddress, DateTime.UtcNow);
    }

    public void LogTokenRefresh(int userId, string ipAddress)
    {
        _logger.LogInformation(
            "Security Event: Token refreshed. UserId: {UserId}, IP: {IpAddress}, Time: {Time}",
            userId, ipAddress, DateTime.UtcNow);
    }

    public void LogLogout(int userId, string ipAddress)
    {
        _logger.LogInformation(
            "Security Event: User logged out. UserId: {UserId}, IP: {IpAddress}, Time: {Time}",
            userId, ipAddress, DateTime.UtcNow);
    }

    private static string MaskIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return "[empty]";

        if (identifier.Length <= 4)
            return "****";

        // Show first 2 and last 2 characters
        return $"{identifier[..2]}***{identifier[^2..]}";
    }
}
