using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using GeneralInsurance.Api.Validators;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GeneralInsurance.Api.Models
{
    public class Insured : IValidatableObject
    {
        /// <summary>
        /// Access nmuber of the Kiwibank customer (a maximum of 10 digits)
        /// </summary>
        [Range(1,9999999999, ErrorMessage = "CustomerID must be a number up to 10 digits long")] 
        [DataMember(Name = "customerId")]
        public int? CustomerId { get; set; }

        [StringLength((255))]
        [AtLeastOneOf2FieldsIsRequired("OrganisationName", ErrorMessage="At least one of Surname and Organsation name must be supplied")]
        [Only1Of2FieldsExists("OrganisationName", ErrorMessage="At most only one of Surname and Organisation can be supplied")]
        [IfThisHasValueThenThese4FieldsAreRequired("Gender", "Title", "FirstName", "Dob", ErrorMessage="If Surname exists then Title, FirstName, Date of Birth and Gender must also exist")]
        [RegularExpression("^([a-zA-Z0-9 &'\\w()/-]+)$", ErrorMessage = "Surname with unacceptable characters")]
        [DataMember(Name = "surname")]
        public string Surname { get; set; }

        [StringLength(120)]
        [RegularExpression("^(a-zA-Z0-9 --]+)$", ErrorMessage = "First name with unacceptable characters")]
        [DataMember(Name = "firstname")]
        public string FirstName { get; set; }

        [StringLength(120)]
        [RegularExpression("^(a-zA-Z0-9 --]+)$", ErrorMessage = "Middle name with unacceptable characters")]
        [ConfirmCombinedStringsNotToLong("FirstName", 120, ErrorMessage = "First and Middle names cannot combined to greater then 120 characters in length")]
        [DataMember(Name = "middlename")]
        public string MiddleName { get; set; }


        /// <summary>
        /// Date of birth of Customer (if not an Organisation)
        /// </summary>
        /// <value>Date of birth of the customer - formatted YYY-MM-DD</value>
        [CannotBeFutureDated(ErrorMessage="date of Birth cannot be set for sometime in the future")]
        [EnforceRequiredDateFormat(ErrorMessage="date of Birth must be in the format of 'YYYY-MM-DD'")]
        [DataMember(Name = "dob")]
        public string Dob { get; set; }

        /// <summary>
        /// Gender of Customer
        /// </summary>
        [DataMember(Name="gender")]
        public string Gender { get; set; }

        /// <summary>
        /// The type of customer being requested
        /// </summary>
        [Required]
        [FieldMustHaveSetValueIfAnotherFieldSet("Surname","Consumer", ErrorMessage="If surname is supplied then CustomerType must be 'Consumer'")]
        [FieldMustNotHaveSetValueIfAnotherFieldSet("OrganisationName","Consumer",ErrorMessage="If OrganisationName is supplied then CustomerType must not be 'Consumer")]
        [DataMember(Name="customerType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// The relationship of the customer to the insurance policy
        /// </summary>
        [Required]
        [DataMember(Name = "insuredtype")]
        public string InsuredType { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [StringLength((20))]
        [RegularExpression("^\\+(0,1)\\d{3}[ -]?([\\d]{3})[ -]?([\\d]{3,5})$$", ErrorMessage = "Mobile phone must be all digits except for a leading +")]
        [AtLeastOneOf3FieldsIsRequired("WorkPhone", "HomePhone", ErrorMessage ="At least one of Mobile, Work and Home phone numbers must be entered")]
        [DataMember(Name = "mobilePhone")]
        public string MobilePhone { get; set; }

        [StringLength((20))]
        [RegularExpression("^\\+(0,1)([0-9]+)$", ErrorMessage = "Work phone must be all digits except for a leading +")]
        [DataMember(Name = "workphone")]
        public string WorkPHone { get; set; }

        [StringLength((20))]
        [RegularExpression("^\\+(0,1)([0-9]+)$", ErrorMessage = "Home phone must be all digits except for a leading +")]
        [DataMember(Name = "homephone")]
        public string HomePhone { get; set; }

        [StringLength((20))]
        [DataMember(Name="title")]
        public string Title { get; set; }

        [StringLength(255)]
        [RegularExpression("^([a-zA-Z0-9 &'\"()/-]+)$",ErrorMessage = "Organisation Name with unacceptable characters")]
        [DataMember(Name = "organisationname")]
        public string OrganisationName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var service = (IInsuranceRequestValidator) validationContext.GetService(typeof(IInsuranceRequestValidator));
            var exceptions = new List<ValidationResult>();
            if(service == null) throw new NotImplementedException();

            var errors = service.ValidateRequest(this).Where(a => !a.IsValid).ToList();
            if (!errors.Any(a => a.IsValid))
            {
                exceptions.AddRange(errors.Select(a=>new ValidationResult(a.ValidationMessages, new []{a.FieldName})));
            }
            return exceptions;
        }
    }

    public class EnforceRequiredDateFormat : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (string) value;
            if (!string.IsNullOrEmpty(currentValue))
            {
                Regex regex = new Regex(@"^\d{4}-\d{2}-\d{2}$");
                Match match = regex.Match(currentValue);
                if (!match.Success)
                {
                    return new ValidationResult(ErrorMessage);
                }

                string[] dateParts = currentValue.Split("-");
                int day = int.Parse(dateParts[2]);
                if (day == 0 || day > 31)
                {
                    return new ValidationResult("Invalid day provided in Date of Birth");
                }

                int month = int.Parse(dateParts[1]);
                if (month == 0 || month > 12)
                {
                    return new ValidationResult("Invalid month provided in Date of Birth");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class ConfirmCombinedStringsNotToLong : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly int _maxLength;

        public ConfirmCombinedStringsNotToLong(string combinedProperty, int maxLength)
        {
            _comparisonProperty = combinedProperty;
            _maxLength = maxLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value ?? "";
            var property = validationContext.ObjectType.GetProperty((_comparisonProperty));
            if(property == null)
                throw new ArgumentException("Property with this name not found");
            var comparisionValue = (string) property.GetValue(validationContext.ObjectInstance);
            if(currentValue.Length + comparisionValue.Length > _maxLength)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }

    public class FieldMustHaveSetValueIfAnotherFieldSet : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly string _requiredeValue;

        public FieldMustHaveSetValueIfAnotherFieldSet(string combinedProperty, string requiredValue)
        {
            _comparisonProperty = combinedProperty;
            _requiredeValue = requiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isValueValid = false;
            ErrorMessage = ErrorMessageString;
            var currentValue = (string) value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (string) property.GetValue(validationContext.ObjectInstance);
            if (!string.IsNullOrEmpty(value1))
            {
                if (!string.IsNullOrEmpty(currentValue) && currentValue.Equals(_requiredeValue.ToLower(),
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    isValueValid = true;
                }
            }
            else
            {
                // Other field is not set so no test is required
                isValueValid = true;
            }
            if(!isValueValid)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;

        }
    }

    public class FieldMustNotHaveSetValueIfAnotherFieldSet : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly string _requiredeValue;

        public FieldMustNotHaveSetValueIfAnotherFieldSet(string combinedProperty, string requiredValue)
        {
            _comparisonProperty = combinedProperty;
            _requiredeValue = requiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isValueValid = false;
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (string)property.GetValue(validationContext.ObjectInstance);

            if (!string.IsNullOrEmpty(value1))
            {
                if (!string.IsNullOrEmpty(currentValue) && currentValue.ToLower() != _requiredeValue.ToLower())
                {
                    isValueValid = true;
                }
            }
            else
            {
                // Other field is not set so no test is required
                isValueValid = true;
            }
            if (!isValueValid)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }

    public class AtLeastOneOf3FieldsIsRequired : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly string _comparisonProperty2;

        public AtLeastOneOf3FieldsIsRequired(string combinedProperty, string combinedProperty2)
        {
            _comparisonProperty = combinedProperty;
            _comparisonProperty2 = combinedProperty2;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool atLeastOneExists = false;
            ErrorMessage = ErrorMessageString;
            var currentValue = (string) value;
            if (!string.IsNullOrEmpty(currentValue))
            {
                atLeastOneExists = true;
            }

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (string)property.GetValue(validationContext.ObjectInstance);
            var property2 = validationContext.ObjectType.GetProperty(_comparisonProperty2);
            var value2 = (string)property2.GetValue(validationContext.ObjectInstance);
            if (!string.IsNullOrEmpty(value1))
            {
                atLeastOneExists = true;
            }
            if (!string.IsNullOrEmpty(value2))
            {
                atLeastOneExists = true;
            }
            if (!atLeastOneExists)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }

    public class IfThisHasValueThenThese4FieldsAreRequired : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        private readonly string _comparisonProperty2;
        private readonly string _comparisonProperty3;
        private readonly string _comparisonProperty4;

        public IfThisHasValueThenThese4FieldsAreRequired(string combinedProperty, string combinedProperty2, string combinedProperty3, string combinedProperty4)
        {
            _comparisonProperty = combinedProperty;
            _comparisonProperty2 = combinedProperty2;
            _comparisonProperty3 = combinedProperty3;
            _comparisonProperty4 = combinedProperty4;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value;
            if (!string.IsNullOrEmpty(currentValue))
            {
                var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
                var value1 = (string)property.GetValue(validationContext.ObjectInstance);
                var property2 = validationContext.ObjectType.GetProperty(_comparisonProperty2);
                var value2 = (string)property2.GetValue(validationContext.ObjectInstance);
                var property3 = validationContext.ObjectType.GetProperty(_comparisonProperty3);
                var value3 = (string)property3.GetValue(validationContext.ObjectInstance);
                var property4 = validationContext.ObjectType.GetProperty(_comparisonProperty4);
                var value4 = (string)property4.GetValue(validationContext.ObjectInstance);
                if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2) || string.IsNullOrEmpty(value3) ||
                    string.IsNullOrEmpty(value4))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }

    public class AtLeastOneOf2FieldsIsRequired : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public AtLeastOneOf2FieldsIsRequired(string combinedProperty)
        {
            _comparisonProperty = combinedProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool atLeastOneExists = false;
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value;
            if (!string.IsNullOrEmpty(currentValue))
            {
                atLeastOneExists = true;
            }

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (string)property.GetValue(validationContext.ObjectInstance);
            if (!string.IsNullOrEmpty(value1))
            {
                atLeastOneExists = true;
            }
            if (!atLeastOneExists)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }

    public class Only1Of2FieldsExists : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public Only1Of2FieldsExists(string combinedProperty)
        {
            _comparisonProperty = combinedProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (string)property.GetValue(validationContext.ObjectInstance);
            if (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(currentValue))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    public class IfThisFieldExistsSoMustDate : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public IfThisFieldExistsSoMustDate(string combinedProperty)
        {
            _comparisonProperty = combinedProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (string)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (DateTime?)property.GetValue(validationContext.ObjectInstance);
            if ((!string.IsNullOrEmpty(currentValue) && value != null) || string.IsNullOrEmpty(currentValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    public class IfThisDateFieldExistsSoMustStringField : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public IfThisDateFieldExistsSoMustStringField(string combinedProperty)
        {
            _comparisonProperty = combinedProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = (DateTime?)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            var value1 = (string)property.GetValue(validationContext.ObjectInstance);
            if ((currentValue!= null && !string.IsNullOrEmpty(value1)) || currentValue == null)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    public class CannotBeFutureDated : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            bool isValidDate = DateTime.TryParse((string) value, out var date);
            if (isValidDate)
            {
                if(date > DateTime.Now)
                    return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}