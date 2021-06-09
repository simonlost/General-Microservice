using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace GeneralInsurance.Api.Models
{
    public class InsuredNewResponse : IJsonConvertable
    {
        [DataMember(Name="customerid")]
        public int? CustomerId { get; set; }

        [DataMember(Name="customerFound")]
        public bool? CustomerFound { get; set; }

        [DataMember(Name="Errors")]
        public List<Error> Errors { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InsuredNewResponse {\n");
            sb.Append(" CustomerId: ").Append(CustomerId).Append("\n");
            sb.Append(" CustomerFound: ").Append(CustomerFound).Append("\n");
            foreach (var err in Errors)
            {
                sb.Append(" Error - Coded: ").Append(err.Code).Append(" , Description: ").Append(err.Description).Append("\n");
            }

            sb.Append("}\n");
            return sb.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class Error
    {
        [DataMember(Name="Code")]
        public int Code { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }


}