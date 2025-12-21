namespace ClupApi.Models
{
    public class SecurityValidationResult
    {
        public bool IsValid { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();

        public static SecurityValidationResult Success() => new() { IsValid = true };

        public static SecurityValidationResult Failure(params string[] errors) => new()
        {
            IsValid = false,
            Errors = errors
        };
    }
}
