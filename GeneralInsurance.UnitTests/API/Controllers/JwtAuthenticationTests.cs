using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;
using System.Reflection;
using Xunit;

namespace GeneralInsurance.UnitTests.API.Controllers
{
    public class JwtAuthenticationTests
    {
        [Fact]
        public void AllVersionsGreaterThan1EnforceClaimsOnHttpMethods()
        {
            Assembly assemnly = Assembly.GetAssembly(typeof(GeneralInsurance.Api.Controllers.BaseController));
            var methods = Assembly.GetType()
                .SelectMany(typeof => typeof.GetMethods())
                .Where(m->m.GetCustomAttributes(typeof(HttpMethodAttribute), true).Length > 0)
                .ToList();
            methods.All(m => m.GetCustomAttributes(typeof(AuthorizeAttribute), true).Length > 0).Should().BeTrue();
        }
    }
}