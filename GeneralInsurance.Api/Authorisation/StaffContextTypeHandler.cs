using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace GeneralInsurance.Api.Authorisation
{
    public class StaffContextTypeHandler : BaseIdentityContextTypeHandler
    {
        protected override IdentityContextTypes SpecifiedContextTypeToHandle => IdentityContextTypes.staff;

        protected override Task DoHandleRequirementAsync(AuthorizationHandlerContext context, AcceptedContextTypeRequirement requirement)
        {
            if (ResourcePathContainsCustomerId((context)))
            {
                var delegated = context.User.Claims.Any(c => c.Type == "delegated") &&
                                bool.Parse(context.User.Claims.Single(c => c.Type == "delegated").Value);
                if (delegated)
                {
                    var customerIdFromResourcePath = GetCustomerIdFromResourcePath(context);
                    var approvedCustomerId = GetAuthorisedCustomerId(context);
                    if (customerIdFromResourcePath == approvedCustomerId)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        protected override string GetAuthorisedCustomerId(AuthorizationHandlerContext context)
        {
            return JObject.Parse(context.User.Claims.Single(c => c.Type == "identityContext").Value)["originatorId"].Value<string>();
        }
    }
}