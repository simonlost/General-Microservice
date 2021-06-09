using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneralInsurance.Api.Models;
using GeneralInsurance.Api.Services;
using GeneralInsurance.Api.Validators;
using GeneralInsurance.DataAccess.Middleware;
using GeneralInsurance.Interfaces.Interfaces;
using GeneralInsurance.Interfaces.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;

namespace GeneralInsurance.Api.Controllers
{


    public class InsuranceApiController : BaseController
    {
        private readonly IInsuranceRequestValidator _insuranceRequestValidator;
        private readonly ICustomerAccessValidation _customerAccessValidator;
        private readonly ICustomerMiddlewareStore _customerStore;

        public InsuranceApiController(ILineService linkService, IInsuranceRequestValidator insuranceRequestValidator,
            ICustomerAccessValidation customerAccessValidation, ICustomerMiddlewareStore customerStore)
        {
            _insuranceRequestValidator = insuranceRequestValidator;
            _customerAccessValidator = customerAccessValidation;
            _customerStore = customerStore;
        }

        [HttpPost]
        [Route("/v1/insurance/insured/new")]
        [Authorize(Policy = "HasReadAccess")]
        public virtual IActionResult V1InsuranceInsuredNewPost([FromHeader(Name = "x-fapi-interaction-id")]
            string x_fapi_interaction_id, [FromBody] InsuredNewResponse requestBody)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, GetErrors(ModelState));
            }

            AndoCustomer toValidate = new AndoCustomer()
            {
                CorllationId = Guid.Parse(x_fapi_interaction_id),
                CustomerId = requestBody.CustomerId,
                CustomerType = requestBody.CustomerType,
                DateOfBirth = !string.IsNullOrEmpty(requestBody.Dob) ? (DateTime?)DateTime.Parse(requestBody.Dob) : null,
                FirstName = requestBody.FirstName,
                EmailAddress = requestBody.Email
            };
            string gender;
            if (requestBody.Gender != null)
            {
                gender = requestBody.Gender;
            }
            else
            {
                gender = "";
            }

            if (gender.Equals("male"))
            {
                gender = "Male";
            }
            else if (gender.Equals("female"))
            {
                gender = "Female";
            }

            toValidate.Gender = gender;
            toValidate.HomePhone = requestBody.HomePhone;
            toValidate.InsuredType = requestBody.InsuredType;
            toValidate.MiddleName = requestBody.MiddleName;
            toValidate.MobilePhone = requestBody.MobilePhone;
            toValidate.OrganisationName = requestBody.OrganisationName;
            toValidate.Surname = requestBody.Surname;
            toValidate.Title = requestBody.Title;
            toValidate.WorkPhone = requestBody.WorkPhone;

            SearchResult theResult;
            if (requestBody.CustomerId.HasValue)
            {
                theResult = _customerAccessValidation.Validate(toValidate);
            }
            else
            {
                theResult = _customerAccessValidation.Search(toValidate);
            }
            // return 200 unless create triggered then return 201
            InsuredNewResponse theResponse = new InsuredNewResponse()
            {
                CustomerFound = theResult.CustomerFound,
                CustomerId = theResult.AccessNumber
            };
            if (theResult.Errors != null && theResult.Errors.Count > 0)
            {
                theResponse.Errors = new List<Models.Error>();
                foreach (var err in theResult.Errors)
                {
                    theResponse.Errors.Add(new Models.Error() {Code = err.Code, Description = err.Description});
                }
            }
            return StatusCode(theResult.Created ? 201 : 200, theResponse);
        }

        private ErrorModel GetErrors(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                var errors = new ErrorModel { Errors = new List<ErrorModelErrors>() };
                foreach (var a in ModelState.Values)
                {
                    foreach (var e in a.Errors)
                    {
                        if (e.Exception != null)
                        {
                            errors.Errors.Add(new ErrorModelErrors() { Code = 1001, Description = e.Exception.Message });
                        }
                        else
                        {
                            errors.Errors.Add(new ErrorModelErrors() { Code = 1001, Description = e.ErrorMessage });
                        }
                    }
                }
                return errors;
            }

            return null;
        }
    }
}