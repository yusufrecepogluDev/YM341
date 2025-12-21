using ClupApi.Models;

namespace ClupApi.Services
{
    public interface IValidationService
    {
        SecurityValidationResult ValidateStudentNumber(string studentNumber);
        SecurityValidationResult ValidateEmail(string email);
        SecurityValidationResult ValidatePassword(string password);
        SecurityValidationResult ValidateStringLength(string input, int maxLength, string fieldName);
        bool ContainsSqlInjectionPatterns(string input);
    }
}
