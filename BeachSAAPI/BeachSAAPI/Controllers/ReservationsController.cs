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
    /// Reservaciones y su facturación (Reservations). El cliente solo accede a las suyas.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : Controller
    {
        private readonly DbContextBeachSA dbContext = null;
        private readonly IAuthorizationServices authorizationServices = null;
        private readonly IGometaService gometaService;
        private readonly IInvoicePdfService invoicePdfService;
        private readonly IEmailService emailService;
        /// <summary>
        /// Recibe el contexto y los servicios de autenticación, GoMeta, PDF y correo.
        /// </summary>
        /// <param name="_context">Contexto de la base de datos.</param>
        /// <param name="rServices">Servicio de autenticación.</param>
        /// <param name="_gometaService">Servicio de consulta a GoMeta.</param>
        /// <param name="_invoicePdfService">Servicio que genera el PDF de la factura.</param>
        /// <param name="_emailService">Servicio que envía correos.</param>
        public ReservationsController(DbContextBeachSA _context, IAuthorizationServices rServices, IGometaService _gometaService, IInvoicePdfService _invoicePdfService, IEmailService _emailService)
        {
            this.dbContext = _context;
            this.authorizationServices = rServices;
            this.gometaService = _gometaService;
            this.invoicePdfService = _invoicePdfService;
            this.emailService = _emailService;
        }



        //[HttpGet] List : Permite mostrar la lista de la información correspondiente
        /// <summary>
        /// Lista las reservaciones: todas para el administrador y las propias para el cliente.
        /// </summary>
        /// <returns>La lista solicitada.</returns>
        [Authorize]
        [HttpGet]
        [Route("List")]
        public async Task<List<Reservation>> List()
        {
            if (this.IsAdmin())
            {
                return await this.dbContext.Reservations.ToListAsync();
            }

            User current = await this.CurrentUser();
            int userId = current?.ID ?? 0;

            return await this.dbContext.Reservations.Where(r => r.UserId == userId).ToListAsync();
        }


        //[HttpPost]Create(Object) :  Permite registrar  los datos en la base datos
        /// <summary>
        /// Registra la reservación, calcula descuento, IVA y colones, crea la factura en una transacción y la envía por correo.
        /// </summary>
        /// <param name="reservation">Datos de la reservación.</param>
        /// <returns>200 con el detalle de la factura, o 400 con el error.</returns>
        [Authorize]
        [HttpPost]
        [Route("Create")] 
        public async Task<IActionResult> Create(Reservation reservation)
        {
            string msj = "";

            try
            {

                //validaciones
                if (reservation == null)
                    return this.Respond("Error, no se permiten datos en blanco");

                if (reservation.GuestQuantity <= 0)
                    return this.Respond("Error, la cantidad de huespedes debe ser mayor a 0");

                //DateTime.Today es de la clase DateTime, regresa la fecha de hoy, sin hora, solo fecha
                if (reservation.CheckInDate.Date < DateTime.Today)
                    return this.Respond("Error, la fecha de entrada no puede ser antes de hoy");


                //noches, pasar los dias a la variable noches
                int nights = reservation.AccommodationDays;

                //noches positivas nada mas: con esto siempre se calcula salida posterior a entrada
                if (reservation.AccommodationDays <= 0)
                    return this.Respond("Error, la cantidad de noches debe ser mayor a 0");


                //calcular el dia de salida a partir de los dias que indica el cliente: AddDays() es un metodo de la clase DateTime, que suma dias a una fecha
                //a CheckoutDate se le suma las noches indicadas
                reservation.CheckOutDate = reservation.CheckInDate.AddDays(nights);


                //validaciones de metodo de pago 

               
                //forma de pago escrita? si no:
                if (string.IsNullOrWhiteSpace(reservation.PaymentMethod))
                    return this.Respond("Error, indicar la forma de pago");

                //variable para la forma de pago en minusculas o mayusculas
                string pago = reservation.PaymentMethod.ToLower();



                //validacion de pagos

                //cheque si el pago es con cheque, se debe indicar el numero de cheque y el banco
                if (pago == "efectivo")
                {
                    
                }
                else if (pago == "cheque")
                {
                    if (string.IsNullOrWhiteSpace(reservation.CheckNumber))
                        return this.Respond("Error, con cheque debe indicar el numero de cheque");

                    if (string.IsNullOrWhiteSpace(reservation.Bank))
                        return this.Respond("Error, con cheque debe indicar el banco");
                }
                else if (pago == "tarjeta")
                {
                    
                }
                else
                {
                    return this.Respond("Error, la forma de pago debe ser Efectivo, Cheque o Tarjeta");
                }


                //si no es cheque, no se guardan datos de cheque
                if (pago != "cheque")
                {
                    reservation.CheckNumber = null;
                    reservation.Bank = null;
                }


                //Validacion de fk:

                //validacion paquete 
                var package = await this.dbContext.Packages.FirstOrDefaultAsync(p => p.ID == reservation.PackageId);
                if (package == null)
                    return this.Respond("Error, el paquete no existe");

                //validacion usuario
                var user = await this.CurrentUser();
                if (this.IsAdmin() && reservation.UserId > 0)
                    user = await this.dbContext.Users.FirstOrDefaultAsync(u => u.ID == reservation.UserId);
                if (user == null)
                    return this.Respond("Error, el usuario no existe");

                reservation.UserId = user.ID;


                //calculos renales matematicos

                //costo de la noche por persona: el subtotal es el costo del paquete por la cantidad de noches por la cantidad de huespedes
                double subtotal = package.Cost * nights * reservation.GuestQuantity;

                //descuento
                double descuento = 0;

                //si el pago es en efectivo, se aplican descuentos segun la cantidad de noches
                if (pago == "efectivo")
                {
                    if (nights >= 13) descuento = 0.25;
                    else if (nights >= 10) descuento = 0.20;
                    else if (nights >= 7) descuento = 0.15;
                    else if (nights >= 3) descuento = 0.10;
                }

                double iva = 0.13; // 13% de IVA

                //calcular total en dolares y colones
                double montoDescuento = subtotal * descuento;
                double subtotalConDescuento = subtotal - montoDescuento;
                double montoIva = subtotalConDescuento * iva;
                double totalUsd = subtotalConDescuento + montoIva;

                //voy a usar http://apis.gometa.org/tdc/tdc.json

                //se asigna el resultado de la llamada a la api de gometa a la variable tipoCambio
                double? tipoCambio = await gometaService.ObtenerTipoCambioAsync();

                //validacion de gometa, si no se puede obtener el tipo de cambio, se devuelve un error
                if (tipoCambio == null)
                    return this.Respond("Error, no se pudo obtener el tipo de cambio");



                //cambio a colones de lo del total apagar 
                double totalColones = totalUsd * tipoCambio.Value;


                // Actualizar los valores de la reservacion
                reservation.Discount = descuento;
                reservation.IVA = iva;

                // Iniciar una transaccion
                //una transacción permite guardar varias operaciones juntas
                // En este caso, es para asegurar que la reservación y la factura se guarden juntas
                // Si ocurre un error al guardar alguna de las dos, se deshacen los cambios
                using var transaction = await this.dbContext.Database.BeginTransactionAsync();

                // Guardar la reservacion
                this.dbContext.Reservations.Add(reservation);

                await this.dbContext.SaveChangesAsync();

                // Crear la factura automaticamente
                Invoice invoice = new Invoice();

                invoice.UserId = reservation.UserId;
                invoice.PackageId = reservation.PackageId;
                invoice.ReservationId = reservation.ID;

                invoice.InvoiceDate = DateTime.Now;

                invoice.Subtotal = subtotal;
                invoice.Discount = descuento;
                invoice.DiscountAmount = montoDescuento;
                invoice.IVA = iva;
                invoice.IvaAmount = montoIva;
                invoice.ExchangeRate = tipoCambio.Value;

                invoice.totalUsd = totalUsd;
                invoice.totalColones = totalColones;

                // Guardar la factura
                this.dbContext.Invoices.Add(invoice);

                await this.dbContext.SaveChangesAsync();

                // Utilizar el ID generado como numero de factura
                invoice.InvoiceNumber = invoice.ID;

                await this.dbContext.SaveChangesAsync();

                // Confirmar la transaccion
                await transaction.CommitAsync();

                bool emailSent = false;
                try
                {
                    byte[] pdf = this.invoicePdfService.Generate(invoice, reservation, user, package);
                    emailSent = await this.emailService.SendInvoiceAsync(user.Email, user.FullName, invoice.InvoiceNumber, pdf);
                }
                catch (Exception)
                {
                    emailSent = false;
                }


                return Ok(new
                {
                    emailSent = emailSent,
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
                        subtotal = subtotal,
                        discount = descuento,
                        discountAmount = montoDescuento,
                        iva = iva,
                        ivaAmount = montoIva,
                        exchangeRate = tipoCambio.Value,
                        totalUsd = totalUsd,
                        totalColones = totalColones
                    }
                });


            }
            catch (Exception ex)
            {

                msj = $"Error,{ex.InnerException?.Message ?? ex.Message}";
            }
            return this.Respond(msj);
        }




        //[HttpPut]Edit(Object):  Permite realizar el proceso de modificación de un registro anteriormente creado.

        /// <summary>
        /// Modifica la reservación y recalcula su factura en la misma transacción.
        /// </summary>
        /// <param name="aux">Reservación con los cambios.</param>
        /// <returns>Respuesta HTTP 200 con el resultado, o 400 con el mensaje de error.</returns>
        [Authorize]
            [HttpPut]
            [Route("Update")]
            public async Task<IActionResult> Edit(Reservation aux)
            {
                string msj = "";
                try
                {

                //validaciones

                if (aux == null)
                    return this.Respond("Error, no se permiten datos en blanco");
                
                //consulta reservacion existente
                Reservation temp = await this.dbContext.Reservations.FirstOrDefaultAsync(r => r.ID == aux.ID);
                if (temp == null)
                    return this.Respond($"Error, no existe ninguna reservacion con el ID {aux.ID}");
                if (!await this.CanAccess(temp))
                    return this.Respond("Error, no tiene permiso sobre esta reservacion");
                //cuanto huespedes
                if (aux.GuestQuantity <= 0)
                    return this.Respond("Error, la cantidad de huespedes debe ser mayor a 0");
                //cambio de fecha? actualziar
                if (aux.CheckInDate.Date != temp.CheckInDate.Date && aux.CheckInDate.Date < DateTime.Today)
                    return this.Respond("Error, la fecha de entrada no puede ser antes de hoy");
                
              

                //noches 
                int nights = aux.AccommodationDays;

                //mas de 1 noche
                if (nights <= 0)
                    return this.Respond("Error, la cantidad de noches debe ser mayor a 0");

             
               
                // tipo de pago valido? si no :         
                if (string.IsNullOrWhiteSpace(aux.PaymentMethod))
                    return this.Respond("Error, indicar la forma de pago");
                
                //pago en minusculas o mayusculas 
                string pago = aux.PaymentMethod.Trim().ToLower();

                // validacion de pago 

                if (pago == "cheque")
                {
                    if (string.IsNullOrWhiteSpace(aux.CheckNumber))
                        return this.Respond("Error, con cheque debe indicar el numero de cheque");

                    if (string.IsNullOrWhiteSpace(aux.Bank))
                        return this.Respond("Error, con cheque debe indicar el banco");
                }
                else if (pago != "efectivo" && pago != "tarjeta")
                {
                    return this.Respond("Error, la forma de pago debe ser Efectivo, Cheque o Tarjeta");
                }

                //buscar el paquete  para recalcular
                int packageId = aux.PackageId > 0 ? aux.PackageId : temp.PackageId;
                var package = await this.dbContext.Packages.FirstOrDefaultAsync(p => p.ID == packageId);
                if (package == null)
                    return this.Respond("Error, el paquete no existe");

                //recalculos si se editan numeros
                double subtotal = package.Cost * nights * aux.GuestQuantity;

                double descuento = 0;
                if (pago == "efectivo")
                {
                    if (nights >= 13) descuento = 0.25;
                    else if (nights >= 10) descuento = 0.20;
                    else if (nights >= 7) descuento = 0.15;
                    else if (nights >= 3) descuento = 0.10;
                }

                double iva = 0.13;

                double montoDescuento = subtotal * descuento;
                double subtotalConDescuento = subtotal - montoDescuento;
                double montoIva = subtotalConDescuento * iva;
                double totalUsd = subtotalConDescuento + montoIva;

                double? tipoCambio = await gometaService.ObtenerTipoCambioAsync();
                if (tipoCambio == null)
                    return this.Respond("Error, no se pudo obtener el tipo de cambio");

                double totalColones = totalUsd * tipoCambio.Value;


                //actualziar datos
                temp.PackageId = package.ID;
                temp.AccommodationDays = aux.AccommodationDays;
                temp.Discount = descuento;
                temp.IVA = iva;
                temp.PaymentMethod = pago;
                temp.GuestQuantity = aux.GuestQuantity;
                temp.CheckInDate = aux.CheckInDate;
                temp.CheckOutDate = aux.CheckInDate.AddDays(nights);

                if (pago == "cheque")
                {
                    temp.CheckNumber = aux.CheckNumber;
                    temp.Bank = aux.Bank;
                }
                else
                {
                    temp.CheckNumber = null;
                    temp.Bank = null;
                }


                this.dbContext.Reservations.Update(temp);

                Invoice invoice = await this.dbContext.Invoices.FirstOrDefaultAsync(i => i.ReservationId == temp.ID);
                if (invoice != null)
                {
                    invoice.PackageId = package.ID;
                    invoice.Subtotal = subtotal;
                    invoice.Discount = descuento;
                    invoice.DiscountAmount = montoDescuento;
                    invoice.IVA = iva;
                    invoice.IvaAmount = montoIva;
                    invoice.ExchangeRate = tipoCambio.Value;
                    invoice.totalUsd = totalUsd;
                    invoice.totalColones = totalColones;
                }

                using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                {
                    await this.dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                           msj = $"Reservacion del {temp.CheckInDate} modificada correctamente/" +
                           $"Total en colones: {totalColones} y total en dolares: {totalUsd}";
                             }
                                 catch (Exception ex)
                             {
                             msj = $"Error,{ex.InnerException?.Message ?? ex.Message}";
            }
                                 return this.Respond(msj);
                              }


        //[HttpDelete] Delete(ID) : Permite eliminar un registro por medio de su identificador
        /// <summary>
        /// Cancela la reservación y elimina su factura.
        /// </summary>
        /// <param name="id">Identificador de la reservación.</param>
        /// <returns>Respuesta HTTP 200 con el resultado, o 400 con el mensaje de error.</returns>
        [Authorize]
        [HttpDelete]
            [Route("Delete")]
            public async Task<IActionResult> Delete(int id)
            {
                string msj = "";
                try
                {
                    Reservation aux = await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == id);

                    if (aux != null && !await this.CanAccess(aux))
                    {
                        msj = "Error, no tiene permiso sobre esta reservacion";
                    }
                    else if (aux != null)
                    {
                        var invoices = this.dbContext.Invoices.Where(x => x.ReservationId == aux.ID);
                        this.dbContext.Invoices.RemoveRange(invoices);
                        this.dbContext.Reservations.Remove(aux);
                        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
                        {
                            await this.dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }

                        msj = $"Reservacion del dia {aux.CheckInDate} eliminada correctamente";
                    }
                    else
                    {
                        msj = $"Error, no existe ninguna reservacion con el ID {id}";
                    }
                }
                catch (Exception ex)
                {
                    msj = $"Error,{ex.InnerException?.Message ?? ex.Message}";
                }
                return this.Respond(msj);
            }


            //[HttpGet] Search(ID) : Recibe como parámetro un valor especifica para identificar un registro en la base datos.
            /// <summary>
            /// Busca una reservación por su identificador.
            /// </summary>
            /// <param name="id">Identificador de la reservación.</param>
            /// <returns>La reservación, o null si no existe o no es del usuario.</returns>
            [Authorize]
            [HttpGet]
            [Route("Search")]
            public async Task<Reservation> Search(int id)
            {
                Reservation temp = await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == id);

                if (temp != null && !await this.CanAccess(temp))
                {
                    return null;
                }

                return temp;
            }


        //[HttpGet] SearchDescrip(value): Recibe un valor para buscar un registro por medio desu descripción.


        /// <summary>
        /// Busca reservaciones por forma de pago.
        /// </summary>
        /// <param name="value">Forma de pago a buscar.</param>
        /// <returns>Las reservaciones encontradas.</returns>
        [HttpGet]
        [Route("SearchDescrip")]
        [Authorize]
        public async Task<List<Reservation>> SearchDescrip(string value)
        {
            var query = this.dbContext.Reservations.Where(x => x.PaymentMethod.Contains(value));

            if (!this.IsAdmin())
            {
                User current = await this.CurrentUser();
                int userId = current?.ID ?? 0;
                query = query.Where(x => x.UserId == userId);
            }

            return await query.ToListAsync();
        }


        //end
    


        /// <summary>
        /// Indica si el usuario del token es administrador.
        /// </summary>
        /// <returns>true si su rol es Admin.</returns>
        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        /// <summary>
        /// Obtiene el usuario del token.
        /// </summary>
        /// <returns>El usuario, o null si no existe.</returns>
        private async Task<User> CurrentUser()
        {
            string email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await this.dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Indica si el usuario puede usar una reservación.
        /// </summary>
        /// <param name="reservation">Reservación a revisar.</param>
        /// <returns>true si es Admin o si la reservación es suya.</returns>
        private async Task<bool> CanAccess(Reservation reservation)
        {
            if (this.IsAdmin())
            {
                return true;
            }

            User current = await this.CurrentUser();
            return current != null && reservation.UserId == current.ID;
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
