using BeachSAAPI.Dto;
using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    /// <summary>
    /// Define la validación de credenciales y la emisión de tokens.
    /// </summary>
    public interface IAuthorizationServices
    {
        /// <summary>
        /// Valida las credenciales y devuelve un token JWT.
        /// </summary>
        /// <param name="authorization">Correo y contraseña enviados al iniciar sesión.</param>
        /// <returns>El token y el resultado de la validación.</returns>
        AuthorizationResponse DevolverToken(User authorization);
    }
}
