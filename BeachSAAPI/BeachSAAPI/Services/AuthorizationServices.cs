using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeachSAAPI.Services
{
    /// <summary>
    /// Valida credenciales y genera tokens JWT de ocho minutos.
    /// </summary>
    public class AuthorizationServices : IAuthorizationServices
    {
        private readonly DbContextBeachSA dbContext = null;
        private readonly IConfiguration configuration = null;

        /// <summary>
        /// Recibe la configuración y el contexto de la base de datos.
        /// </summary>
        /// <param name="pConfig">Configuración de la aplicación (appsettings.json).</param>
        /// <param name="pContext">Contexto de la base de datos.</param>
        public AuthorizationServices(IConfiguration pConfig, DbContextBeachSA pContext)
        {
            this.dbContext = pContext;
            this.configuration = pConfig;
        }

        /// <summary>
        /// Valida el correo y la contraseña, migra contraseñas en texto plano a SHA-256 y genera el token.
        /// </summary>
        /// <param name="authorization">Correo y contraseña enviados al iniciar sesión.</param>
        /// <returns>El token y Result en true, o Result en false si las credenciales no son válidas.</returns>
        public AuthorizationResponse DevolverToken(User authorization)
        {
            User temp = this.dbContext.Users.FirstOrDefault(x => x.Email.Equals(authorization.Email));

            if (temp != null && !PasswordHasher.Verify(authorization.Password, temp.Password))
            {
                temp = null;
            }

            if (temp != null && !PasswordHasher.IsHash(temp.Password))
            {
                temp.Password = PasswordHasher.Hash(authorization.Password);
                this.dbContext.SaveChanges();
            }

            string token = "";

           
            if (temp != null)
            {    
                token = this.GenerarToken(temp);

                return new AuthorizationResponse() { Token = token, Result = true, Msj = "Ok" };
            }
            else
            {
                return new AuthorizationResponse() { Token = "", Result = false, Msj = "Authentication failed..." };
            }
        }


        /// <summary>
        /// Genera un token JWT firmado con el correo y el rol del usuario, válido por ocho minutos.
        /// </summary>
        /// <param name="user">Usuario para el que se genera el token.</param>
        /// <returns>El token JWT como texto.</returns>
        private string GenerarToken(User user)
        {
            string tokenCreado = "";
            var key = this.configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(key);
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Email));
            claims.AddClaim(new Claim(ClaimTypes.Role, string.IsNullOrEmpty(user.Role) ? "Customer" : user.Role));
            var credencialesToken = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims, 
                Expires = DateTime.UtcNow.AddMinutes(8), 
                SigningCredentials = credencialesToken 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfi = tokenHandler.CreateToken(tokenDescriptor);
            tokenCreado = tokenHandler.WriteToken(tokenConfi);
            return tokenCreado;
        }
    }
}
