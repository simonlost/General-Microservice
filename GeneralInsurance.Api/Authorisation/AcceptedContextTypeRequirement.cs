using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace GeneralInsurance.Api.Authorisation
{
    public class AcceptedContextTypeRequirement : IAuthorizationRequirement
    {
        public List<IdentityContextTypes> AcceptedIdentityContextTypes { get; }

        public AcceptedContextTypeRequirement(params IdentityContextTypes[] acceptedIdentityContextTypes)
        {
            AcceptedIdentityContextTypes = acceptedIdentityContextTypes.ToList();
        }
    }
}