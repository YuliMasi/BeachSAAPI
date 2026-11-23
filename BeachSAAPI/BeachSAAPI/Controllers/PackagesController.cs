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
        [Authorize]
        [HttpGet]
        [Route("List")]

        public List<Package> List()
        {
            return this.dbContext.Packages.ToList();
        }


        //Post
        [HttpPost]
        [Route("Create")]
        [Authorize]

        public async Task<string> Create(Package package)
        {
            string msj = "";

            try
            {
                if(package == null)
                {
                    return "Error, no se permiten datos en blanco ";
                }
                else
                {
                    this.dbContext.Packages.Add(package);
                    
                    await this.dbContext.SaveChangesAsync();

                    msj = $"El paquete {package.PackageType} se ha guardado correctamente";
                }

            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException.ToString()}";
            }

            return msj;
        }


        //Edit
        [HttpPut]
        [Route("Update")]
        [Authorize]

        public async Task<string> Update(Package aux)
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
                        temp.Cost = aux.Cost;
                        temp.PercentageTotalAmount = aux.PercentageTotalAmount;
                        temp.RemainingMonthlyInstallments = aux.RemainingMonthlyInstallments;

                        await this.dbContext.SaveChangesAsync();

                        msj = $"El paquete {temp.PackageType} se ha actualizado correctamente";
                    }
                    else
                    {
                        msj = $"Error, no está registrado el paquete {aux.PackageType}";
                    }
                }
                else
                {
                    msj = "Error, no se permite datos en blanco";
                }

            }
            catch (Exception ex)
            {
                msj = $"Error, {ex.InnerException.ToString()}";
            }

            return msj;
        }


        //Delete
        [HttpDelete]
        [Route("Delete")]
        [Authorize]

        public async Task<string> Delete(int id)
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
                        
                        await this.dbContext.SaveChangesAsync();
                        
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
                msj = $"Error, {ex.InnerException.ToString()}";
            }

            return msj;

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



    }
}
