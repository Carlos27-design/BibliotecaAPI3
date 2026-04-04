using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Validations
{
    public class FirstMayusAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object ? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var valueString = value.ToString()!;

            var firstLetter = valueString[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayuscula");
            }

            return ValidationResult.Success;
        }
    }
}
