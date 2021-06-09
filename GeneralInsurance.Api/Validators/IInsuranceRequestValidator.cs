using System;
using System.Collections.Generic;
using GeneralInsurance.Api.Models;

namespace GeneralInsurance.Api.Validators
{
    public interface IInsuranceRequestValidator
    {
        IEnumerable<ValidatorOutput> ValidateRequest(Insured insuranceRequest);
    }

    public class InsuranceRequestValidator : IInsuranceRequestValidator
    {
        private readonly IValidationItemsFactory _validationItemsFactory;

        public InsuranceRequestValidator(IValidationItemsFactory validationItemsFactory)
        {
            _validationItemsFactory = validationItemsFactory;
        }
        public IEnumerable<ValidatorOutput> ValidateRequest(Insured insuranceRequest)
        {
            var allErrors = new List<ValidatorOutput>
            {
                ValidateGender(insuranceRequest.Gender),
                ValidateCustomerType(insuranceRequest.CustomerType),
                ValidateInsuredType(insuranceRequest.InsuredType),
                ValidateTitle(insuranceRequest.Title)
            };
            return allErrors;
        }

        private ValidatorOutput ValidateTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return new ValidatorOutput() { IsValid = true, ValidationMessages = String.Empty, FieldName = "Title" };
            }
            var a = _validationItemsFactory.GetStringArrayComparer(new[] { "BISHOP", "BROTHER", "DAME", "MR", "MRS", "DR", "HON", "JUDGE", "LADY", "SIR", "LORD", "MADAM", "MAJOR" });
            return CreateValidationOutcome(a.CheckValidity(title), $"{title} is not an allowed title",
                "CustomerType");
        }

        private ValidatorOutput ValidateInsuredType(string insuredType)
        {
            var a = _validationItemsFactory.GetStringArrayComparer(new[] { "Primary", "Additional" });
            return CreateValidationOutcome(a.CheckValidity(insuredType), $"{insuredType} is not an allowed insured type",
                "CustomerType");
        }

        private ValidatorOutput ValidateCustomerType(string customerType)
        {
            var a = _validationItemsFactory.GetStringArrayComparer(new[] {"Consumer", "Trust", "Company"});
            return CreateValidationOutcome(a.CheckValidity(customerType), $"{customerType} is not an allowed customer type",
                "CustomerType");
        }

        private ValidatorOutput ValidateGender(string gender)
        {
            if (string.IsNullOrEmpty(gender))
            {
                return new ValidatorOutput(){IsValid = true, ValidationMessages = String.Empty, FieldName = "Gender"};
            }

            string genderLowered = gender.ToLower();
            if (genderLowered.Equals("male"))
            {
                gender = "Male";
            }
            if (genderLowered.Equals("female"))
            {
                gender = "Female";
            }

            var a = _validationItemsFactory.GetStringArrayComparer(new[] {"Male", "Female", "Unspecified"});
            return CreateValidationOutcome(a.CheckValidity(gender), $"{gender} is not an allowed gender",
                "Gender");
        }

        private ValidatorOutput CreateValidationOutcome(bool validationOutcome, string failureMessage, string fieldName)
        {
            if (validationOutcome)
            {
                return new ValidatorOutput()
                {
                    IsValid = true,
                    ValidationMessages = String.Empty,
                    FieldName = fieldName
                };
            }

            return new ValidatorOutput()
            {
                IsValid = false,
                ValidationMessages = failureMessage,
                FieldName = fieldName
            };
        }
    }
}