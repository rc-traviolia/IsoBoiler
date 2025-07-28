using IsoBoiler.Json;
using System.ComponentModel.DataAnnotations;

namespace IsoBoiler.Utilities
{
    public static class IValidatableObjectExtensions
    {
        public static TClass Validate<TClass>(this TClass classToCheck) where TClass : IValidatableObject
        {
            var context = new ValidationContext(classToCheck!); // <--- tells the validator what object to validate
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(classToCheck!, context, validationResults))
            {
                var validationFailureResults = new List<string>();
                foreach (var result in validationResults.Where(vr => vr.ErrorMessage is not null))
                {

                    validationFailureResults.Add(result.ErrorMessage!);
                }
                throw new ValidationException(validationFailureResults.ToJson());

            }

            return classToCheck;
        }
    }
}
