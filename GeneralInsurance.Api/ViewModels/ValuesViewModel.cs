using System.Runtime.Serialization;

namespace GeneralInsurance.Api.ViewModels
{
    [DataContract]
    public class ValuesViewModel
    {
        [DataMember]
        public string Value { get; set; }
    }
}