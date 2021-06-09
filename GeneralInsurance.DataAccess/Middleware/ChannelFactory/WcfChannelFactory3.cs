using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using GeneralInsurance.DataAccess.Middleware.Abstractions;

namespace GeneralInsurance.DataAccess.Middleware.ChannelFactory
{
    public class WcfChannelFactory3<T> : IWcfChannelFactory<T> where T : class
    {
        private readonly IConfiguration _configuration;
        private readonly IWcfCredentialsService _credentialsService;
        private readonly IProxyGenerator _proxyGenerator;
        public BasicHttpBinding Binding { get; }
        public ChannelFactory<T> ChannelFactory { get; }
        public static string MiddlewareEndpointConfigurationKey => "MiddlewareProvider:URL3";
        private static ILogger Logger { get { return Logger.ForContext<IWcfChannelFactory<T>>();
        } }

        public WcfChannelFactory3(IConfiguration configuration, IWcfCredentialsService credentialsService,
            IProxyGenerator proxyGenerator)
        {
            _configuration = configuration;
            _credentialsService = credentialsService;
            Binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress(new Uri(_configuration[MiddlewareEndpointConfigurationKey]));
            ChannelFactory = new ChannelFactory<T>(Binding, endpoint);
            SetSecurityAndCredentials();
            _proxyGenerator = proxyGenerator;
        }

        public BasicHttpSecurityMode SecurityMode
        {
            get
            {
                return _configuration[MiddlewareEndpointConfigurationKey].ToLower().StartsWith("https")
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.TransportCredentialsOnly;
            }
        }

        private void SetSecurityAndCredentials()
        {
            if (_credentialsService.UseAnonymousAccess)
            {
                return;
            }

            Binding.Security.Mode = SecurityMode;
            Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            ChannelFactory.Endpoint.EndpointBehaviors.Remove(typeof(ClientCredentials));
            ChannelFactory.Endpoint.EndpointBehaviors.Add(_credentialsService.Credentials);
        }


        public T CreateChannel()
        {
            var channel = ChannelFactory.CreateChannel();
            var interceptedChannel =
                _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(new WcfMethodInterceptor<T>(channel));
            return interceptedChannel;
        }
    }
}