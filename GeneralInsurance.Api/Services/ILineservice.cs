using GeneralInsurance.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GeneralInsurance.Api.Services
{
    public interface ILineService
    {
        LinksInner GetLink(IUrlHelper urlHelper, string relDescriptor, string routeName, object values = null);
    }
}