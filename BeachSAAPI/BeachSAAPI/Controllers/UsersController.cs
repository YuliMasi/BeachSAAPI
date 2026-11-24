using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using BeachSAAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BeachSAAPI.Controllers
{
    /// <summary>
    /// Usuarios, registro, inicio de sesión y perfil (api/users).
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private DbContextBeachSA dbContext = null;
        private readonly IAuthorizationServices authorizationServices = null;
        private readonly IGometaService gometaService;
        /// <summary>
        /// Recibe el contexto y los servicios de autenticación y GoMeta.
        /// </summary>
        /// <param name="_context">Contexto de la base de datos.</param>
        /// <param name="pServices">Servicio de autenticación.</param>
        /// <param name="pGometaService">Servicio de consulta a GoMeta.</param>
        public UsersController(DbContextBeachSA _context, IAuthorizationServices pServices, IGometaService pGometaService)
        {
            this.dbContext = _context;
            this.authorizationServices = pServices;
            this.gometaService = pGometaService;
        }

        /// <summary>
        /// Lista todos los usuarios sin contraseñas. Solo Admin.
        /// </summary>
        /// <returns>La lista solicitada.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("List")]
        public List<User> List()
        {
            List<User> users = this.dbContext.Users.AsNoTracking().ToList();
            users.ForEach(u => u.Password = null);
            return users;
        }

        /// <summary>
        /// Devuelve el perfil del usuario del token, sin contraseña.
        /// </summary>
        /// <returns>200 con el usuario, o 404 si no existe.</returns>
        [Authorize]
        [HttpGet]
        [Route("Profile")]
        public IActionResult Profile()
        {
            string email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            User temp = this.dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);

            if (temp == null)
            {
                return NotFound("Usuario no encontrado");
            }

            temp.Password = null;
            return Ok(temp);
        }

        /// <summary>
        /// Valida las credenciales y devuelve el token JWT. No requiere token.
        /// </summary>
        /// <param name="temp">Correo y contraseña del usuario.</param>
        /// <returns>200 con el token, o 401 si las credenciales no son válidas.</returns>
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

        /// <summary>
        /// Registra un usuario con cualquier rol. Solo Admin.
        /// </summary>
        /// <param name="temp">Usuario a registrar.</param>
        /// <returns>Respuesta HTTP 200 con el resultado, o 400 con el mensaje de error.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("Create")]
        public async Task<IActionResult> Create(User temp)
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
                    temp.Password = PasswordHasher.Hash(temp.Password);
                    this.dbContext.Users.Add(temp);
                    using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                    {
                        await this.dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    msj = $"Usuario {temp.FullName} almacenado correctamente ";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }
            return this.Respond(msj);
        }

        /// <summary>
        /// Registra un cliente nuevo con la cédula como identificador. No requiere token.
        /// </summary>
        /// <param name="temp">Datos del cliente.</param>
        /// <returns>Respuesta HTTP 200 con el resultado, o 400 con el mensaje de error.</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User temp)
        {
            if (temp == null || string.IsNullOrEmpty(temp.Email) || string.IsNullOrEmpty(temp.Password))
            {
                return BadRequest("No se permiten datos vacíos");
            }

            if (temp.ID <= 0)
            {
                return BadRequest("Debe indicar una cédula válida");
            }

            if (this.dbContext.Users.Any(u => u.ID == temp.ID))
            {
                return BadRequest("Ya existe una cuenta con esa cédula");
            }

            bool exists = this.dbContext.Users.Any(u => u.Email == temp.Email);
            if (exists)
            {
                return BadRequest("Ya existe una cuenta con ese correo");
            }

            temp.Role = "Customer";
            temp.Password = PasswordHasher.Hash(temp.Password);

            try
            {
                this.dbContext.Users.Add(temp);
                await this.dbContext.SaveChangesAsync();
                return Ok($"Usuario {temp.FullName} registrado correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error, {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Modifica un usuario; el cliente solo el suyo y solo el Admin cambia roles. La contraseña solo cambia si viene una nueva.
        /// </summary>
        /// <param name="temp">Usuario con los cambios.</param>
        /// <returns>Respuesta HTTP 200 con el resultado, o 400 con el mensaje de error.</returns>
        [HttpPost]
        [Authorize]
        [Route("Update")]
        public async Task<IActionResult> Update(User temp)
        {
            string msj = "";
            try
            { 
                if (temp == null)
                {
                    msj = "Error, no se permiten datos vacíos";
                }
                else
                {
                    bool isAdmin = User.IsInRole("Admin");
                    string currentEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var aux = this.dbContext.Users.FirstOrDefault(x => x.Email.Equals(temp.Email));
                    if (aux == null)
                    {
                        msj = $"Error, no existe el usuario {temp.FullName} registrado";
                    }
                    else if (!isAdmin && aux.Email != currentEmail)
                    {
                        msj = "Error, solo puede modificar su propio perfil";
                    }
                    else
                    {
                        aux.IdType = temp.IdType;
                        aux.FullName = temp.FullName;
                        aux.PhoneNumber = temp.PhoneNumber;
                        aux.Address = temp.Address;
                        aux.Email = temp.Email;
                        if (!string.IsNullOrEmpty(temp.Password))
                        {
                            aux.Password = PasswordHasher.IsHash(temp.Password) ? temp.Password : PasswordHasher.Hash(temp.Password);
                        }
                        if (isAdmin && !string.IsNullOrEmpty(temp.Role))
                        {
                            aux.Role = temp.Role;
                        }

                        this.dbContext.Users.Update(aux);
                        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                    {
                        await this.dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                        msj = $"Cambios aplicados a {aux.Email} correctamente..";
                    }

                }
            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }
            return this.Respond(msj);
        }

        /// <summary>
        /// Elimina un usuario por su correo; los administradores no se pueden eliminar. Solo Admin.
        /// </summary>
        /// <param name="email">Correo del usuario.</param>
        /// <returns>Respuesta HTTP 200 con el resultado, o 400 con el mensaje de error.</returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string email)
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
                            using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                    {
                        await this.dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                            msj = $"Usuario {aux.FullName} eliminado correctamente";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }

            return this.Respond(msj);
        }

        /// <summary>
        /// Busca usuarios por nombre, sin contraseñas. Solo Admin.
        /// </summary>
        /// <param name="name">Nombre o parte del nombre a buscar.</param>
        /// <returns>La lista solicitada.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("SearchFullName")]
        public List<User> SearchFullName(string name)
        {
            List<User> users = this.dbContext.Users.AsNoTracking().Where(r => r.FullName.Contains(name)).ToList();
            users.ForEach(u => u.Password = null);
            return users;
        }

        /// <summary>
        /// Busca una cédula entre los usuarios y, si no existe, la consulta en GoMeta. Solo el Admin o el dueño ven los datos de un usuario existente.
        /// </summary>
        /// <param name="cedula">Número de cédula, solo dígitos.</param>
        /// <returns>200 con esNuevo y los datos, 400, 404 o 502.</returns>
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
                    string currentEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!User.IsInRole("Admin") && existente.Email != currentEmail)
                    {
                        return Ok(new { esNuevo = false });
                    }

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
    


        /// <summary>
        /// Convierte el mensaje en una respuesta HTTP.
        /// </summary>
        /// <param name="msj">Mensaje del resultado; si empieza con "Error" o "Debe" se responde 400.</param>
        /// <returns>400 si es un error; 200 en otro caso.</returns>
        private IActionResult Respond(string msj)
        {
            if (msj.StartsWith("Error") || msj.StartsWith("Debe"))
            {
                return BadRequest(msj);
            }

            return Ok(msj);
        }
}
}
