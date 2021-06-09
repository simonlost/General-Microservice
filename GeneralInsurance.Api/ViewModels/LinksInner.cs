using System.Runtime.Serialization;

namespace GeneralInsurance.Api.ViewModels
{
    [DataContract]
    public class LinksInner
    {
        [DataMember(Name = "rel")]
        public string Rel { get; set; }

        [DataMember(Name = "href")]
        public string Href { get; set; }
    }
}