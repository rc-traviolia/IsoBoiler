using System.ComponentModel.DataAnnotations;

namespace IsoBoiler.HTTP.Authentication
{
    public class AuthSettings<TTokenFormat> : IValidatableObject where TTokenFormat : IAuthToken
    {
        [Required]
        public required string ClientID { get; set; }
        [Required]
        public required string ClientSecret { get; set; }
        [Required]
        public required string URI { get; set; }
        public string GrantType { get; set; } = "client_credentials";
        public string Scope { get; set; } = "machine";
        public int TTL_Seconds { get; set; } = 3600; //1 hour

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(GrantType))
            {
                yield return new ValidationResult("GrantType cannot be null, empty, or whitespace.", new[] { nameof(GrantType) });
            }
            if (string.IsNullOrWhiteSpace(Scope))
            {
                yield return new ValidationResult("Scope cannot be null, empty, or whitespace.", new[] { nameof(Scope) });
            }
        }
    }
}
