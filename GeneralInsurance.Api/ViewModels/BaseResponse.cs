using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GeneralInsurance.Api.ViewModels
{
    [DataContract]
    public class BaseResponse
    {
        public BaseResponse(List<LinksInner> links)
        {
            Links = links;
        }

        [DataMember(Name="_links")]
        public List<LinksInner> Links { get; set; }
    }
}