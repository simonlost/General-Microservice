using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GeneralInsurance.Api.Services
{
    [ExcludeFromCodeCoverage]
    public class TokenValidationService : ITokenValidationService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly ILogger Logger = Log.ForContext<TokenValidationService>();

        private byte[] _jwtSigningX509Certificate;

        public TokenValidationService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public TokenValidationParameters GetTokenValidationParameters()
        {
            var signingCert = GetSigningCert();
            if(signingCert == null)
                throw new ApplicationException("Failed to obtain valid jwt signing cert.");

            return new TokenValidationParameters()
            {
                ValidateActor = false,
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = _configuration.GetValue<string>("JwtIssuer"),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RoleClaimType = "scopes",
                ClockSkew = TimeSpan.FromMinutes(5),
                IssuerSigningKey = new X509SecurityKey(new X509Certificate2(signingCert)),
                SaveSigninToken = true
            };
        }

        private byte[] GetSigningCert()
        {
            if (_jwtSigningX509Certificate != null)
                return _jwtSigningX509Certificate;
            _jwtSigningX509Certificate = GetSigningCertFromApigee();
            return _jwtSigningX509Certificate;
        }

        private byte[] GetSigningCertFromApigee()
        {
            var jwtSigningX509CertificateUrl = _configuration.GetValue<string>("JwtSigningX509CertificateUrl");
            if(string.IsNullOrEmpty(jwtSigningX509CertificateUrl))
                throw new ApplicationException("JwtSigningX509CertificateUrl configuration setting cannot be empty");
            var httpClient = _httpClientFactory.CreateClient(HttpClients.ApigeeClient);
            var response = httpClient.GetAsync(jwtSigningX509CertificateUrl).Result;
            if(!response.IsSuccessStatusCode)
                throw new ApplicationException($"Cannot retrieve JWT 509 signing certificate from {jwtSigningX509CertificateUrl}");
            var pemEncodedCert = response.Content.ReadAsStringAsync().Result;
            Logger.Verbose($"JWT Signing Cert: {pemEncodedCert}", pemEncodedCert);
            Logger.Information($"JWT signing cert obtained from {jwtSigningX509CertificateUrl}",jwtSigningX509CertificateUrl);

            return Convert.FromBase64String(pemEncodedCert.Replace("-----BEGIN CERTIFICATE-----", "").Replace("-----END CERTIFICATE-----").Replace("\r","").Replace("\n",""));
        }
    }
}