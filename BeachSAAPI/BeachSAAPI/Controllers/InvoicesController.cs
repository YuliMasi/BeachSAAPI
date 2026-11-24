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
        private readonly IInvoicePdfService invoicePdfService;
        private readonly IEmailService emailService;

        public InvoicesController(DbContextBeachSA _context, IAuthorizationServices iServices, IInvoicePdfService iPdfService, IEmailService iEmailService)
        {
            this.dbContext = _context;

            this.authorizationServices = iServices;
            this.invoicePdfService = iPdfService;
            this.emailService = iEmailService;
        }





        //List
        [HttpGet]
        [Route("List")]
        [Authorize]

        public List<Invoice> List()
        {
            return this.dbContext.Invoices.ToList();
        }


        //Post
        [HttpPost]
        [Route("Create")]
        [Authorize]

        public async Task<IActionResult> Create(Invoice invoice)
        {
            string msj = "";

            try
            {
                //validaciones
                if (invoice == null)
                    return this.Respond("Error, no se permiten datos en blanco");

                //defaulttime = 01/01/0001 00:00:00
                if (invoice.InvoiceDate == default(DateTime))
                    return this.Respond("Error, indicar la fecha de la factura");

                //la fecha de la factura no puede ser de manana 
                if (invoice.InvoiceDate.Date > DateTime.Today)
                    return this.Respond("Error, la fecha de la factura no puede ser posterior a hoy");

                //negativos
                if (invoice.totalUsd <= 0)
                    return this.Respond("Error, el total en dolares debe ser mayor a 0");

                if (invoice.totalColones <= 0)
                    return this.Respond("Error, el total en colones debe ser mayor a 0");



                //validacion de la fk, creo que solo se usa esa porque de ahi vienen userID y packageID


                //si no existe reservacion, no existe factura
                var reservation = await this.dbContext.Reservations.FirstOrDefaultAsync(r => r.ID == invoice.ReservationId);
                if (reservation == null)
                    return this.Respond("Error, la reservacion no existe");


                this.dbContext.Invoices.Add(invoice);

                await this.dbContext.SaveChangesAsync();

                msj = $"La factura {invoice.InvoiceNumber} se registro correctamente";

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
        [Authorize]

        public async Task<IActionResult> Update(Invoice aux)
        {
            string msj = "";

            try
            {


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
        [Authorize]

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
                msj = $"Error, {ex.InnerException?.Message ?? ex.Message}";
            }

            return this.Respond(msj);

        }


        //Search
        [HttpGet]
        [Route("SearchById")]
        [Authorize]
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



        //SearchDetails (Para hacer la factura)
        [HttpGet]
        [Route("SearchDetails")]
        [Authorize]
        public async Task<IActionResult> SearchDetails(int reservationId)
        {
            // Buscar la factura de la reservacion
            var invoice = await this.dbContext.Invoices.FirstOrDefaultAsync(x => x.ReservationId == reservationId);

            if (invoice == null)
            {
                return NotFound("No existe una factura para esta reservacion");
            }
                

            // Buscar la reservacion
            var reservation = await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == invoice.ReservationId);

            // Buscar el cliente
            var user = await this.dbContext.Users.FirstOrDefaultAsync(x => x.ID == invoice.UserId);

            // Buscar el paquete
            var package = await this.dbContext.Packages.FirstOrDefaultAsync(x => x.ID == invoice.PackageId);

            if (reservation == null || user == null || package == null)
            {
                return NotFound("No se encontraron todos los datos de la factura");
            }
                

            // Devolver los datos en formato JSON
            return Ok(new
            {
                invoiceNumber = invoice.InvoiceNumber,
                invoiceDate = invoice.InvoiceDate,

                customer = new
                {
                    fullName = user.FullName,
                    id = user.ID,
                    idType = user.IdType,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber
                },

                reservation = new
                {
                    id = reservation.ID,
                    packageType = package.PackageType,
                    guestQuantity = reservation.GuestQuantity,
                    accommodationDays = reservation.AccommodationDays,
                    checkInDate = reservation.CheckInDate,
                    checkOutDate = reservation.CheckOutDate,
                    paymentMethod = reservation.PaymentMethod,
                    checkNumber = reservation.CheckNumber,
                    bank = reservation.Bank
                },

                details = new
                {
                    subtotal = invoice.Subtotal,
                    discount = invoice.Discount,
                    discountAmount = invoice.DiscountAmount,
                    iva = invoice.IVA,
                    ivaAmount = invoice.IvaAmount,
                    exchangeRate = invoice.ExchangeRate,
                    totalUsd = invoice.totalUsd,
                    totalColones = invoice.totalColones
                }
            });
        }













        //end
    


        [HttpGet]
        [Route("Pdf")]
        [Authorize]
        public async Task<IActionResult> Pdf(int reservationId)
        {
            var data = await this.LoadInvoiceData(reservationId);

            if (data == null)
            {
                return NotFound("No existe una factura para esta reservacion");
            }

            byte[] pdf = this.invoicePdfService.Generate(data.Value.invoice, data.Value.reservation, data.Value.user, data.Value.package);

            return File(pdf, "application/pdf", $"Factura-{data.Value.invoice.InvoiceNumber:D6}.pdf");
        }


        [HttpPost]
        [Route("SendEmail")]
        [Authorize]
        public async Task<IActionResult> SendEmail(int reservationId)
        {
            var data = await this.LoadInvoiceData(reservationId);

            if (data == null)
            {
                return NotFound("No existe una factura para esta reservacion");
            }

            byte[] pdf = this.invoicePdfService.Generate(data.Value.invoice, data.Value.reservation, data.Value.user, data.Value.package);

            bool sent = await this.emailService.SendInvoiceAsync(data.Value.user.Email, data.Value.user.FullName, data.Value.invoice.InvoiceNumber, pdf);

            if (!sent)
            {
                return StatusCode(500, "No se pudo enviar la factura por correo");
            }

            return Ok($"Factura enviada a {data.Value.user.Email}");
        }


        private async Task<(Invoice invoice, Reservation reservation, User user, Package package)?> LoadInvoiceData(int reservationId)
        {
            var invoice = await this.dbContext.Invoices.FirstOrDefaultAsync(x => x.ReservationId == reservationId);
            if (invoice == null)
            {
                return null;
            }

            var reservation = await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == invoice.ReservationId);
            var user = await this.dbContext.Users.FirstOrDefaultAsync(x => x.ID == invoice.UserId);
            var package = await this.dbContext.Packages.FirstOrDefaultAsync(x => x.ID == invoice.PackageId);

            if (reservation == null || user == null || package == null)
            {
                return null;
            }

            return (invoice, reservation, user, package);
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
