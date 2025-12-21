using ClupApi.Models;
using System.Text.RegularExpressions;

namespace ClupApi.Services
{
    public class ValidationService : IValidationService
    {
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        private static readonly Regex StudentNumberRegex = new(
            @"^\d{8,12}$",
            RegexOptions.Compiled);

        private static readonly Regex PasswordUppercaseRegex = new(
            @"[A-Z]",
            RegexOptions.Compiled);

        private static readonly Regex PasswordLowercaseRegex = new(
            @"[a-z]",
            RegexOptions.Compiled);

        private static readonly Regex PasswordDigitRegex = new(
            @"\d",
            RegexOptions.Compiled);

        // SQL keywords that indicate potential injection when followed by space or special chars
        private static readonly string[] SqlKeywords = new[]
        {
            "select", "insert", "update", "delete", "drop", "alter",
            "create", "exec", "execute", "union", "declare", "cast"
        };

        // Dangerous patterns that are always suspicious
        private static readonly string[] DangerousPatterns = new[]
        {
            "--", ";--", "/*", "*/", "@@", "xp_", "sp_", "0x",
            "sysobjects", "syscolumns", "waitfor delay"
        };

        public SecurityValidationResult ValidateStudentNumber(string studentNumber)
        {
            if (string.IsNullOrWhiteSpace(studentNumber))
            {
                return SecurityValidationResult.Failure("Öğrenci numarası boş olamaz");
            }

            if (!StudentNumberRegex.IsMatch(studentNumber))
            {
                return SecurityValidationResult.Failure(
                    "Öğrenci numarası 8-12 haneli sayısal karakterlerden oluşmalıdır");
            }

            return SecurityValidationResult.Success();
        }


        public SecurityValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return SecurityValidationResult.Failure("E-posta adresi boş olamaz");
            }

            if (!EmailRegex.IsMatch(email))
            {
                return SecurityValidationResult.Failure(
                    "Geçerli bir e-posta adresi giriniz");
            }

            return SecurityValidationResult.Success();
        }

        public SecurityValidationResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return SecurityValidationResult.Failure("Şifre boş olamaz");
            }

            var errors = new List<string>();

            if (password.Length < 8)
            {
                errors.Add("Şifre en az 8 karakter olmalıdır");
            }

            if (!PasswordUppercaseRegex.IsMatch(password))
            {
                errors.Add("Şifre en az bir büyük harf içermelidir");
            }

            if (!PasswordLowercaseRegex.IsMatch(password))
            {
                errors.Add("Şifre en az bir küçük harf içermelidir");
            }

            if (!PasswordDigitRegex.IsMatch(password))
            {
                errors.Add("Şifre en az bir rakam içermelidir");
            }

            if (errors.Count > 0)
            {
                return SecurityValidationResult.Failure(errors.ToArray());
            }

            return SecurityValidationResult.Success();
        }

        public SecurityValidationResult ValidateStringLength(string input, int maxLength, string fieldName)
        {
            if (input != null && input.Length > maxLength)
            {
                return SecurityValidationResult.Failure(
                    $"{fieldName} en fazla {maxLength} karakter olabilir");
            }

            return SecurityValidationResult.Success();
        }

        public bool ContainsSqlInjectionPatterns(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var lowerInput = input.ToLowerInvariant();

            // Check dangerous patterns
            foreach (var pattern in DangerousPatterns)
            {
                if (lowerInput.Contains(pattern))
                {
                    return true;
                }
            }

            // Check SQL keywords followed by space (indicating SQL statement)
            foreach (var keyword in SqlKeywords)
            {
                var keywordWithSpace = keyword + " ";
                if (lowerInput.Contains(keywordWithSpace))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
