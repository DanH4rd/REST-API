using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;

namespace ToDoAPI.Models.Validation
{
    /// <summary>
    /// Class describes custom validation rule to ensure the date is in correct format and in the future.
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime || value is DateTime dateTime && dateTime < DateTime.UtcNow)
            {
                var resourceManager = new ResourceManager(typeof(Resources.ValidationMessages));
                string localizedErrorMessage = resourceManager.GetString(ErrorMessageResourceName ?? "ExpiryDateFuture", CultureInfo.CurrentUICulture)
                                                ?? "ExpiryDate must be in the future.";
                return new ValidationResult(localizedErrorMessage, new[] { validationContext.MemberName ?? string.Empty });
            }
            return ValidationResult.Success!;
        }
    }
}
