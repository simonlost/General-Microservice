using GeneralInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeneralInsurance.Api.Controllers
{
    [Produces("application/json")]
    public abstract class BaseController : Controller
    {
        protected ILineService LinkHelper { get; }

        protected BaseController(ILineService linkService)
        {
            LinkHelper = linkService;
        }
    }
}