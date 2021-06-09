using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GeneralInsurance.Api.ViewModels
{
    [DataContract]
    public class DataResponse<T> : BaseResponse where T : class, new()
    {
        public DataResponse(T data, List<LinksInner> links) : base(links)
        {
            Data = data;
        }

        [DataMember]
        public T Data { get; set; }
    }
}