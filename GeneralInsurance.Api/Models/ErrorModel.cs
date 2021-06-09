using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GeneralInsurance.Api.Models
{
    [DataContract]
    public partial class ErrorModel
    {
        [DataMember(Name = "errors")]
        public List<ErrorModelErrors> Errors { get; set; }
    }

    [DataContract]
    public partial class ErrorModelErrors
    {
        [DataMember(Name ="code")]
        public int? Code { get; set; }

        [DataMember(Name="description")]
        public string Description { get; set; }

    }
}