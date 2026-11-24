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
                //validaciones
                if (invoice == null)
                    return "Error, no se permiten datos en blanco";

                //defaulttime = 01/01/0001 00:00:00
                if (invoice.InvoiceDate == default(DateTime))
                    return "Error, indicar la fecha de la factura";

                //la fecha de la factura no puede ser de manana 
                if (invoice.InvoiceDate.Date > DateTime.Today)
                    return "Error, la fecha de la factura no puede ser posterior a hoy";

                //negativos
                if (invoice.totalUsd <= 0)
                    return "Error, el total en dolares debe ser mayor a 0";

                if (invoice.totalColones <= 0)
                    return "Error, el total en colones debe ser mayor a 0";



                //validacion de la fk, creo que solo se usa esa porque de ahi vienen userID y packageID


                //si no existe reservacion, no existe factura
                var reservation = await this.dbContext.Reservations.FirstOrDefaultAsync(r => r.ID == invoice.ReservationId);
                if (reservation == null)
                    return "Error, la reservacion no existe";


                this.dbContext.Invoices.Add(invoice);

                await this.dbContext.SaveChangesAsync();

                msj = $"La factura {invoice.InvoiceNumber} se registro correctamente";

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