using System.ComponentModel.DataAnnotations;
using Leadway_RSA_API.Models;

namespace Leadway_RSA_API.CustomValidators
{
    // This custom attribute handles the conditional logic.
    // It inherits from ValidationAttribute and overrides the IsValid method.
    public class ExecutorTypeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var executor = (Executor)validationContext.ObjectInstance;

            if (executor.ExecutorType == "Individual")
            {
                if (string.IsNullOrEmpty(executor.FirstName) || string.IsNullOrEmpty(executor.LastName))
                {
                    // Return a validation failure if the condition is not met.
                    return new ValidationResult("First Name and Last Name are required for Individual ExecutorType.", new[] { "FirstName", "LastName" });
                }
            }
            else if (executor.ExecutorType == "Company")
            {
                if (string.IsNullOrEmpty(executor.CompanyName))
                {
                    // Return a validation failure if the condition is not met.
                    return new ValidationResult("Company Name is required for Company ExecutorType.", new[] { "CompanyName" });
                }
            }

            // Return success if all validation passes.
            return ValidationResult.Success;
        }
    }
}