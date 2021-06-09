using System.ServiceModel.Description;

namespace GeneralInsurance.DataAccess.Middleware.ChannelFactory
{
    public interface IWcfCredentialsService
    {
        ClientCredentials Credentials { get; }
        bool UseAnonymousAccess { get; }
    }
}