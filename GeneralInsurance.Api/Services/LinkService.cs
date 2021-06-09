using Microsoft.AspNetCore.Mvc;
using GeneralInsurance.Api.ViewModels;

namespace GeneralInsurance.Api.Services
{
    public class LinkService : ILineService 
    {
        public LinksInner GetLink(IUrlHelper urlHelper, string relDescriptor, string routeName, object values = null)
        {
            return new LinksInner(){Rel = relDescriptor, Href = urlHelper.Action(routeName,values)};
        }
    }
}