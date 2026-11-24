using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeachSAAPI.Services
{
    public class AuthorizationServices : IAuthorizationServices
    {
        private readonly DbContextBeachSA dbContext = null;
        private readonly IConfiguration configuration = null;

        public AuthorizationServices(IConfiguration pConfig, DbContextBeachSA pContext)
        {
            this.dbContext = pContext;
            this.configuration = pConfig;
        }

        public AuthorizationResponse DevolverToken(User authorization)
        {
            User temp = this.dbContext.Users.FirstOrDefault(x =>
            x.Email.Equals(authorization.Email) && x.Password.Equals(authorization.Password));

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
