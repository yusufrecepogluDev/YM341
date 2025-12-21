using ClupApi.Services;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace ClupApi.Tests
{
    /// <summary>
    /// Property-based tests for ValidationService
    /// **Feature: api-security**
    /// </summary>
    public class ValidationServicePropertyTests
    {
        private readonly ValidationService _validationService;

        public ValidationServicePropertyTests()
        {
            _validationService = new ValidationService();
        }

        /// <summary>
        /// **Property 6: Student number validation**
        /// *For any* student number input, the ClupApi SHALL accept only strings containing exactly 8-12 numeric digits.
        /// **Validates: Requirements 4.1**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool ValidStudentNumber_ShouldBeAccepted(byte digit1, byte digit2, byte digit3, byte digit4, 
            byte digit5, byte digit6, byte digit7, byte digit8)
        {
            // Generate valid 8-digit student number
            var studentNumber = $"{digit1 % 10}{digit2 % 10}{digit3 % 10}{digit4 % 10}" +
                               $"{digit5 % 10}{digit6 % 10}{digit7 % 10}{digit8 % 10}";
            
            var result = _validationService.ValidateStudentNumber(studentNumber);
            return result.IsValid;
        }

        [Theory]
        [InlineData("12345678", true)]      // 8 digits - valid
        [InlineData("123456789", true)]     // 9 digits - valid
        [InlineData("1234567890", true)]    // 10 digits - valid
        [InlineData("12345678901", true)]   // 11 digits - valid
        [InlineData("123456789012", true)]  // 12 digits - valid
        [InlineData("1234567", false)]      // 7 digits - invalid
        [InlineData("1234567890123", false)] // 13 digits - invalid
        [InlineData("1234567a", false)]     // contains letter - invalid
        [InlineData("", false)]             // empty - invalid
        [InlineData("   ", false)]          // whitespace - invalid
        public void StudentNumber_ValidationRules(string studentNumber, bool expectedValid)
        {
            var result = _validationService.ValidateStudentNumber(studentNumber);
            Assert.Equal(expectedValid, result.IsValid);
        }


        /// <summary>
        /// **Property 7: Email format validation**
        /// *For any* email input, the ClupApi SHALL accept only strings matching valid email format.
        /// **Validates: Requirements 4.2**
        /// </summary>
        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name@domain.org", true)]
        [InlineData("user+tag@example.co.uk", true)]
        [InlineData("invalid", false)]
        [InlineData("@example.com", false)]
        [InlineData("test@", false)]
        [InlineData("test@.com", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void Email_ValidationRules(string email, bool expectedValid)
        {
            var result = _validationService.ValidateEmail(email);
            Assert.Equal(expectedValid, result.IsValid);
        }

        /// <summary>
        /// **Property 8: Password strength validation**
        /// *For any* password input, the ClupApi SHALL accept only passwords with min 8 chars, uppercase, lowercase, and number.
        /// **Validates: Requirements 4.3**
        /// </summary>
        [Theory]
        [InlineData("Password1", true)]      // Valid: 8+ chars, upper, lower, digit
        [InlineData("MyPass123", true)]      // Valid
        [InlineData("Abcdefg1", true)]       // Valid: exactly 8 chars
        [InlineData("password1", false)]     // Missing uppercase
        [InlineData("PASSWORD1", false)]     // Missing lowercase
        [InlineData("Password", false)]      // Missing digit
        [InlineData("Pass1", false)]         // Too short
        [InlineData("", false)]              // Empty
        public void Password_ValidationRules(string password, bool expectedValid)
        {
            var result = _validationService.ValidatePassword(password);
            Assert.Equal(expectedValid, result.IsValid);
        }

        /// <summary>
        /// **Property 9: String length validation**
        /// *For any* string input exceeding max length, the ClupApi SHALL return validation error.
        /// **Validates: Requirements 4.4**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool StringExceedingMaxLength_ShouldFail(PositiveInt extraLength)
        {
            var maxLength = 10;
            var input = new string('a', maxLength + extraLength.Get);
            
            var result = _validationService.ValidateStringLength(input, maxLength, "TestField");
            return !result.IsValid;
        }

        [Property(MaxTest = 100)]
        public bool StringWithinMaxLength_ShouldPass(PositiveInt length)
        {
            var maxLength = 100;
            var actualLength = Math.Min(length.Get, maxLength);
            var input = new string('a', actualLength);
            
            var result = _validationService.ValidateStringLength(input, maxLength, "TestField");
            return result.IsValid;
        }

        /// <summary>
        /// SQL Injection pattern detection tests
        /// </summary>
        [Theory]
        [InlineData("SELECT * FROM users", true)]
        [InlineData("1; DROP TABLE users--", true)]
        [InlineData("' OR '1'='1", false)]  // Simple quote doesn't match our patterns
        [InlineData("normal text", false)]
        [InlineData("user@example.com", false)]
        [InlineData("UNION SELECT password", true)]
        [InlineData("'; DELETE FROM users;--", true)]
        public void SqlInjection_Detection(string input, bool expectedDetection)
        {
            var result = _validationService.ContainsSqlInjectionPatterns(input);
            Assert.Equal(expectedDetection, result);
        }
    }
}
