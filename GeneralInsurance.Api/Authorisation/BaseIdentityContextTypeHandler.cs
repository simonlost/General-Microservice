using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneralInsurance.Api.Authorisation
{
    public abstract class BaseIdentityContextTypeHandler : AuthorizationHandler<AcceptedContextTypeRequirement>
    {
        protected abstract IdentityContextTypes SpecifiedContextTypeToHandle { get; }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            AcceptedContextTypeRequirement requirement)
        {
            if (!IsIdentityContextTypeSupported(requirement))
            {
                return;
            }

            if (!IsClaimForThisContextType(context))
            {
                return;
            }

            await DoHandleRequirementAsync(context, requirement);
        }

        protected abstract Task DoHandleRequirementAsync(AuthorizationHandlerContext context, AcceptedContextTypeRequirement requirement);

        private bool IsClaimForThisContextType(AuthorizationHandlerContext context)
        {
            return context.User.HasClaim(c =>
                c.Type == "identityContextType" && c.Value == SpecifiedContextTypeToHandle.ToString());
        }

        private bool IsIdentityContextTypeSupported(AcceptedContextTypeRequirement requirement)
        {
            return requirement.AcceptedIdentityContextTypes.Any(c => c == SpecifiedContextTypeToHandle);
        }

        protected static bool ResourcePathContainsCustomerId(AuthorizationHandlerContext context)
        {
            return ((ActionContext) context.Resource).RouteData.Values.Keys.Contains("customerid",
                StringComparer.CurrentCultureIgnoreCase);
        }

        protected static string GetCustomerIdFromResourcePath(AuthorizationHandlerContext context)
        {
            return ((ActionContext) context.Resource).RouteData.Values.Single(v => v.Key.ToLower() == "customerid")
                .Value.ToString();
        }

        protected abstract string GetAuthorisedCustomerId(AuthorizationHandlerContext context);
    }
}