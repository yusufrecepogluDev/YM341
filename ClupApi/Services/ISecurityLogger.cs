namespace ClupApi.Services;

public interface ISecurityLogger
{
    void LogFailedLogin(string identifier, string ipAddress);

    void LogRateLimitExceeded(string ipAddress, string endpoint);

    void LogAuthorizationFailure(int userId, string resource, string action);

    void LogSuspiciousInput(string ipAddress, string input, string endpoint);

    void LogSuccessfulLogin(string identifier, string ipAddress, string userType);

    void LogTokenRefresh(int userId, string ipAddress);

    void LogLogout(int userId, string ipAddress);
}
