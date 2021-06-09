using Microsoft.IdentityModel.Tokens;

namespace GeneralInsurance.Api.Services
{
    public interface ITokenValidationService
    {
        TokenValidationParameters GetTokenValidationParameters();
    }
}