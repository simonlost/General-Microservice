using System.Runtime.Serialization;

namespace GeneralInsurance.DataAccess.Entities
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "1.0.0.1")]
    [DataContract(Name = "Channel", Namespace = "http://schemas.datacontract.org/2004/07/KISInterfaces")]
    public enum Channel : int
    {
        [EnumMember]
        IVR = 1,

        [EnumMember]
        CSC = 2,
        [EnumMember]
        KBREP = 3,
        [EnumMember]
        INET = 4,
        [EnumMember]
        PSHOP = 5,
        [EnumMember]
        TEXTBANK = 6,
        [EnumMember]
        SS = 7,
        [EnumMember]
        CONSUMERFINANCE = 8,
        [EnumMember]
        BUSBANK = 9,
        [EnumMember]
        UNKNOWN = 99,
        [EnumMember]
        ACTIVATE = 11,
        [EnumMember]
        ACTIVATEB = 12,
        [EnumMember]
        ACTIVATEKB = 13,
        [EnumMember]
        EXTERNAL = 14,
        [EnumMember]
        INTERNAL = 10,
        [EnumMember]
        ISL = 15,
        [EnumMember]
        K2 = 16,
        [EnumMember]
        ECN = 17,
        [EnumMember]
        PAS = 18,
        [EnumMember]
        MOBILE = 19
    }
}