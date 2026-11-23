using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using BeachSAAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BeachSAAPI.Controllers
{

    [ApiController]
    [Route("api/invoices")]
    public class InvoicesController : Controller
    {


        private DbContextBeachSA dbContext = null;

        private readonly IAuthorizationServices authorizationServices = null;

        public InvoicesController(DbContextBeachSA _context, IAuthorizationServices iServices)
        {
            this.dbContext = _context;

            this.authorizationServices = iServices;
        }





        //List
        [HttpGet]
        [Route("List")]
        //[Authorize]

        public List<Invoice> List()
        {
            return this.dbContext.Invoices.ToList();
        }


        //Post
        [HttpPost]
        [Route("Create")]
        //[Authorize]

        public async Task<string> Create(Invoice invoice)
        {
            string msj = "";

            try
            {


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
        //[Authorize]

        public async Task<string> Update(Invoice aux)
        {
            string msj = "";

            try
            {


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
        //[Authorize]

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
                    Invoice aux = await this.dbContext.Invoices.FirstOrDefaultAsync(x => x.ID == id);

                    if (aux != null)
                    {
                        this.dbContext.Invoices.Remove(aux);

                        await this.dbContext.SaveChangesAsync();

                        msj = $"La factura {aux.InvoiceNumber} se eliminó correctamente";
                    }
                    else
                    {
                        msj = $"Error, la factura no existe {id}";
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
       // [Authorize]
        public async Task<Invoice> SearchById(int id)
        {
            return await this.dbContext.Invoices.FirstOrDefaultAsync(x => x.ID == id);
        }


        [HttpGet]
        [Route("SearchDescrip")]
        [Authorize]
        public async Task<List<Invoice>> SearchDescrip(int value)
        {
            return await this.dbContext.Invoices.Where(x => x.InvoiceNumber == value).ToListAsync();
        }













        //end
    }
}