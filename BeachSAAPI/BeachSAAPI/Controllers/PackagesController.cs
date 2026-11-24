using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using BeachSAAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeachSAAPI.Controllers
{
    [ApiController]
    [Route("api/packages")]
    public class PackagesController : Controller
    {

        private DbContextBeachSA dbContext = null;

        private readonly IAuthorizationServices authorizationServices = null;

        public PackagesController(DbContextBeachSA _context, IAuthorizationServices pServices)
        {
            this.dbContext = _context;

            this.authorizationServices = pServices;
        }


        //List
        [HttpGet]
        [Route("List")]
        [Authorize]

        public List<Package> List()
        {
            return this.dbContext.Packages.ToList();
        }


        //Post
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create(Package package)
        {
            string msj = "";

            try
            {
                if(package == null)
                {
                    return this.Respond("Error, no se permiten datos en blanco ");
                }
                else
                {
                    if (package.PackageType == "Todo incluido")
                    {
                        package.Cost = 450;
                        package.PercentageTotalAmount = 45;
                        package.RemainingMonthlyInstallments = 24;
                    }
                    else if (package.PackageType == "Alimentación")
                    {
                        package.Cost = 275;
                        package.PercentageTotalAmount = 35;
                        package.RemainingMonthlyInstallments = 18;

                    }
                    else if (package.PackageType == "Hospedaje")
                    {
                        package.Cost = 210;
                        package.PercentageTotalAmount = 15;
                        package.RemainingMonthlyInstallments = 12;

                    }
                    else
                    {
                        return this.Respond("Error, el tipo de paquete no es válido");
                    }

                    this.dbContext.Packages.Add(package);
                    
                    using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                    {
                        await this.dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }

                    msj = $"El paquete {package.PackageType} se ha guardado correctamente";
                }

            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }

            return this.Respond(msj);
        }


        //Edit
        [HttpPut]
        [Route("Update")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Update(Package aux)
        {
            string msj = "";

            try
            {
                if(aux != null)
                {
                    Package temp = await this.dbContext.Packages.FirstOrDefaultAsync(r => r.ID == aux.ID);

                    if (temp != null)
                    {
                        temp.PackageType = aux.PackageType;

                        if (temp.PackageType == "Todo incluido")
                        {
                            temp.Cost = 450;
                            temp.PercentageTotalAmount = 45;
                            temp.RemainingMonthlyInstallments = 24;
                        }
                        else if (temp.PackageType == "Alimentación")
                        {
                            temp.Cost = 275;
                            temp.PercentageTotalAmount = 35;
                            temp.RemainingMonthlyInstallments = 18;
                        }
                        else if (temp.PackageType == "Hospedaje")
                        {
                            temp.Cost = 210;
                            temp.PercentageTotalAmount = 15;
                            temp.RemainingMonthlyInstallments = 12;
                        }

                        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                    {
                        await this.dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }

                        msj = $"El paquete {temp.PackageType} se ha actualizado correctamente";
                    }
                    else
                    {
                        msj = $"Error, no está registrado el con ID {aux.ID}";
                    }
                }
                else
                {
                    msj = "Error, no se permite datos en blanco";
                }

            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }

            return this.Respond(msj);
        }


        //Delete
        [HttpDelete]
        [Route("Delete")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int id)
        {
            string msj = "";

            try
            {
                if (id <= 0)
                {
                    msj = "Debe ingresar un ID válido";
                }
                else
                {
                    Package aux = await this.dbContext.Packages.FirstOrDefaultAsync(x => x.ID == id);
                    
                    if (aux != null)
                    {
                        this.dbContext.Packages.Remove(aux);
                        
                        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                    {
                        await this.dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                        
                        msj = $"El paquete {aux.PackageType} se eliminó correctamente";
                    }
                    else
                    {
                        msj = $"Error, no está registrado el paquete con el ID {id}";
                    }
                }
            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }

            return this.Respond(msj);

        }


        //Search
        [HttpGet]
        [Route("SearchById")]
        [Authorize]
        public async Task<Package> SearchById(int id)
        {
            return await this.dbContext.Packages.FirstOrDefaultAsync(x => x.ID == id);
        }


        [HttpGet]
        [Route("SearchDescrip")]
        [Authorize]
        public async Task<List<Package>> SearchDescrip(string packageType)
        {
            return await this.dbContext.Packages.Where(x => x.PackageType.Contains(packageType)).ToListAsync();
        }



    


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
