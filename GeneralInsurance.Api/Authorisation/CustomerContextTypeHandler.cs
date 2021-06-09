using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GeneralInsurance.Api.Authorisation
{
    public class CustomerContextTypeHandler : BaseIdentityContextTypeHandler
    {
        protected override IdentityContextTypes SpecifiedContextTypeToHandle => IdentityContextTypes.customer;

        protected override Task DoHandleRequirementAsync(AuthorizationHandlerContext context, AcceptedContextTypeRequirement requirement)
        {
            if (ResourcePathContainsCustomerId((context)))
            {
                var customerIdFromResourcePath = GetCustomerIdFromResourcePath(context);
                var approvedCustomerId = GetAuthorisedCustomerId(context);
                if (customerIdFromResourcePath == approvedCustomerId)
                {
                    context.Succeed(requirement);
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
            return context.User.Claims.Single(c => c.Type == "id").Value;
        }
    }
}