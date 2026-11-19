using BeachSAAPI.Dto;
using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    public interface IAuthorizationServices
    {
        AuthorizationResponse DevolverToken(User authorization);
    }
}
