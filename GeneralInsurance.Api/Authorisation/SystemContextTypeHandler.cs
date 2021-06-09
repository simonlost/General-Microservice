using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GeneralInsurance.Api.Authorisation
{
    public class SystemContextTypeHandler : BaseIdentityContextTypeHandler
    {
        protected override IdentityContextTypes SpecifiedContextTypeToHandle => IdentityContextTypes.system;

        protected override Task DoHandleRequirementAsync(AuthorizationHandlerContext context, AcceptedContextTypeRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        protected override string GetAuthorisedCustomerId(AuthorizationHandlerContext context)
        {
            return null;
        }
    }
}