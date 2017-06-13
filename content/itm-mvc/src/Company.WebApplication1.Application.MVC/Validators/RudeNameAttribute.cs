using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Company.WebApplication1.Validators
{
    public class RudeNameAttribute : ValidationAttribute, IClientModelValidator
    {
        private List<string> rudeNames = new List<string> { "rude", "names", "here" };
        public RudeNameAttribute()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string name = (string)value;
            if (rudeNames.Contains(name.ToLower()) || rudeNames.Contains(name.ToLower()))
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }

        // Clientside validation not entirely done, needs javascript (also not sure if correct)
        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-rudename"] = ErrorMessageString;
        }
    }
}
