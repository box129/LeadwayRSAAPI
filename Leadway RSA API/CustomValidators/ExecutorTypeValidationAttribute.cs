using System.ComponentModel.DataAnnotations;
using Leadway_RSA_API.Models;

namespace Leadway_RSA_API.CustomValidators
{
    public class ExecutorTypeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var executor = (Executor)validationContext.ObjectInstance;

            // If it's the default executor, no further validation is needed for specific fields
            if (executor.IsDefault)
            {
                return ValidationResult.Success;
            }

            // For user-added executors, validate based on the ExecutorType
            if (executor.ExecutorType == null)
            {
                return new ValidationResult("ExecutorType is required for a user-added executor.", new[] { nameof(Executor.ExecutorType) });
            }

            if (executor.ExecutorType == ExecutorType.Individual)
            {
                if (string.IsNullOrEmpty(executor.FirstName) || string.IsNullOrEmpty(executor.LastName))
                {
                    return new ValidationResult("First Name and Last Name are required for an Individual Executor.", new[] { nameof(Executor.FirstName), nameof(Executor.LastName) });
                }
            }
            else if (executor.ExecutorType == ExecutorType.Company)
            {
                if (string.IsNullOrEmpty(executor.CompanyName))
                {
                    return new ValidationResult("Company Name is required for a Company Executor.", new[] { nameof(Executor.CompanyName) });
                }
            }
            return ValidationResult.Success;
        }
    }
}