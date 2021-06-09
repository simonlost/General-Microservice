using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneralInsurance.Api.Models;
using GeneralInsurance.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GeneralInsurance.DataAccess.Middleware.ChannelFactory;
using GeneralInsurance.DataAccess.Entities;

namespace GeneralInsurance.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmokeTestApiController : BaseController
    {
        private readonly ISearchMiddlewareStore _customerSearch;

        public SmokeTestApiController(ILineService linkService, ISearchMiddlewareStore customerSearch) : base(
            linkService)
        {
            _customerSearch = customerSearch;
        }

        [HttpGet]
        [Route("/v1/insurance/insured/smoketest", Name = nameof(SmokeTest))]
        [Authorize(Policy = "HasReadAccess")]
        public virtual IActionResult SmokeTest([FromHeader(Name = "x-fapi-interaction-id")]
            string x_fapi_interaction_id)
        {
            try
            {
                AndoCustomer customer = new AndoCustomer()
                {
                    CorollationId = new Guid(), CustomerType = "consumer", DateOfBirth = DateTime.Parse("1965-07-15"),
                    EmailAddress = "jobloggs@gmail.com", FirstName = "Jennifer", Gender ="Female", InsuredType = "Primary", MobilePhone = "021659125"
                };
                var result = _customerSearch.Search(customer);
                InsuredNewResponse theResponse = new InsuredNewResponse()
                {
                    CustomerFound = false,
                    CustomerId = null
                };
                return StatusCode(200, theResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }


    }
}