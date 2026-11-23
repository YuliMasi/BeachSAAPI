using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using BeachSAAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachSAAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private DbContextBeachSA dbContext = null;
        private readonly IAuthorizationServices authorizationServices = null;
        private readonly IGometaService gometaService;
        public UsersController(DbContextBeachSA _context, IAuthorizationServices pServices, IGometaService pGometaService)
        {
            this.dbContext = _context;
            this.authorizationServices = pServices;
            this.gometaService = pGometaService;
        }

        [Authorize]
        [HttpGet]
        [Route("List")]
        public List<User> List()
        {
            return this.dbContext.Users.ToList();
        }

        [HttpPost]
        [Route("AuthenticationUser")]
        public IActionResult AuthenticationUser(User temp)
        {
            var authorization = authorizationServices.DevolverToken(temp);
 
            if (authorization.Result == true)
            {
                return Ok(authorization);
            }
            else
            {   
                return Unauthorized(authorization);
            }
        }

        [Authorize] 
        [HttpPut]
        [Route("Create")]
        public async Task<string> Create(User temp)
        {
            string msj = "";
            try
            {
                if (temp == null)
                {
                    msj = "No se permiten datos vacíos";
                }
                else
                {
                    this.dbContext.Users.Add(temp);
                    await this.dbContext.SaveChangesAsync();
                    msj = $"Usuario {temp.FullName} almacenado correctamente ";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error,{ex.InnerException}";
            }
            return msj;
        }

        [HttpPost]
        [Authorize]
        [Route("Update")]
        public async Task<string> Update(User temp)
        {
            string msj = "";
            try
            { 
                if (temp == null)
                {
                    msj = "No se permiten datos vacíos";
                }
                else
                {
                    var aux = this.dbContext.Users.FirstOrDefault(x => x.Email.Equals(temp.Email));
                    if (aux == null)
                    {
                        msj = $"No existe el usuario {temp.FullName} registrado";
                    }
                    else
                    {
                        aux.IdType = temp.IdType;
                        aux.FullName = temp.FullName;
                        aux.PhoneNumber = temp.PhoneNumber;
                        aux.Address = temp.Address;
                        aux.Email = temp.Email;
                        aux.Password = temp.Password;
                        aux.Role = temp.Role;

                        this.dbContext.Users.Update(aux);
                        await this.dbContext.SaveChangesAsync();
                        msj = $"Cambios aplicados a {aux.Email} correctamente..";
                    }

                }
            }
            catch (Exception ex)
            {
                msj = $"Error,{ex.InnerException}";
            }
            return msj;
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete")]
        public async Task<string> Delete(string email)
        {
            string msj = "";
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    msj = "debe definir el email del usuario a eliminar";
                }
                else
                {
                    var aux = this.dbContext.Users.FirstOrDefault(x => x.Email.Equals(email));

                    if (aux == null)
                    {
                        msj = $"Error, no existe el usuario con el email {email}";
                    }
                    else
                    {
                        if (aux.Role == "Admin")
                        {
                            msj = $"Error, no puedes borrar a {aux.FullName} porque es administrador";
                        }
                        else
                        {
                            this.dbContext.Users.Remove(aux);
                            await this.dbContext.SaveChangesAsync();
                            msj = $"Usuario {aux.FullName} eliminado correctamente";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msj = $"Error,{ex.InnerException}";
            }

            return msj;
        }

        [Authorize]
        [HttpGet]
        [Route("SearchFullName")]
        public List<User> SearchFullName(string name)
        {
            return this.dbContext.Users.Where(r => r.FullName.Contains(name)).ToList();
        }

        [Authorize]
        [HttpGet]
        [Route("CheckIDNumber")]
        public async Task<IActionResult> ConsultarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || !cedula.All(char.IsDigit))
                return BadRequest("Cédula inválida, solo números.");

            if (int.TryParse(cedula, out var id))
            {
                var existente = dbContext.Users.FirstOrDefault(x => x.ID == id);
                if (existente != null)
                {
                    existente.Password = ""; 
                    return Ok(new { esNuevo = false, usuario = existente });
                }
            }

            try
            {
                var usuario = await gometaService.ConsultarCedulaAsync(cedula);

                if (usuario == null)
                    return NotFound("No se encontró la cédula en Gometa.");

                return Ok(new { esNuevo = true, usuario });
            }
            catch (Exception ex)
            {
                return StatusCode(502, $"Error consultando Gometa: {ex.Message}");
            }
        }
    }
}
