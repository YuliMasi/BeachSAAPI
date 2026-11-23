using BeachSAAPI.Dto;
using BeachSAAPI.Models;
using BeachSAAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace BeachSAAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : Controller
    {
        private readonly DbContextBeachSA dbContext = null;
        private readonly IAuthorizationServices authorizationServices = null;
        private readonly IGometaService gometaService;
        public ReservationsController(DbContextBeachSA _context, IAuthorizationServices rServices, IGometaService _gometaService)
        {
            this.dbContext = _context;
            this.authorizationServices = rServices;
            this.gometaService = _gometaService;
        }



        //[HttpGet] List : Permite mostrar la lista de la información correspondiente
        [Authorize]
        [HttpGet]
        [Route("List")]
        public List<Reservation> List()
        {
            return this.dbContext.Reservations.ToList();
        }


        //[HttpPost]Create(Object) :  Permite registrar  los datos en la base datos
        [Authorize]
        [HttpPost]
        [Route("Create")] 
        public async Task<string> Create(Reservation reservation)
        {
            string msj = "";

            try
            {

                //validaciones
                if (reservation == null)
                    return "Error, no se permiten datos en blanco";

                if (reservation.GuestQuantity <= 0)
                    return "Error, la cantidad de huespedes debe ser mayor a 0";

                //DateTime.Today es de la clase DateTime, regresa la fecha de hoy, sin hora, solo fecha
                if (reservation.CheckInDate.Date < DateTime.Today)
                    return "Error, la fecha de entrada no puede ser antes de hoy";


                //noches, pasar los dias a la variable noches
                int nights = reservation.AccommodationDays;

                //noches positivas nada mas: con esto siempre se calcula salida posterior a entrada
                if (reservation.AccommodationDays <= 0)
                    return "Error, la cantidad de noches debe ser mayor a 0";


                //calcular el dia de salida a partir de los dias que indica el cliente: AddDays() es un metodo de la clase DateTime, que suma dias a una fecha
                //a CheckoutDate se le suma las noches indicadas
                reservation.CheckOutDate = reservation.CheckInDate.AddDays(nights);


                //validaciones de metodo de pago 

               
                //forma de pago escrita? si no:
                if (string.IsNullOrWhiteSpace(reservation.PaymentMethod))
                    return "Error, indicar la forma de pago";

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
                        return "Error, con cheque debe indicar el numero de cheque";

                    if (string.IsNullOrWhiteSpace(reservation.Bank))
                        return "Error, con cheque debe indicar el banco";
                }
                else if (pago == "tarjeta")
                {
                    
                }
                else
                {
                    return "Error, la forma de pago debe ser Efectivo, Cheque o Tarjeta";
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
                    return "Error, el paquete no existe";

                //validacion usuario
                 var user = await this.dbContext.Users.FirstOrDefaultAsync(u => u.ID == reservation.UserId);
                if (user == null)
                    return "Error, el usuario no existe";


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
                    return "Error, no se pudo obtener el tipo de cambio";



                //cambio a colones de lo del total apagar 
                double totalColones = totalUsd * tipoCambio.Value;
                

                //se actualizan los valores de descuento, iva y total en la reservacion antes de guardarla en la base de datos
                reservation.Discount = descuento;
                reservation.IVA = iva;


                //crear
                this.dbContext.Reservations.Add(reservation);

                await this.dbContext.SaveChangesAsync();
                msj = $"Reservacion para dia  {reservation.CheckInDate} registrada correctamente/" +
                    //n2:formato de #c
                    $"Total en colones: {totalColones:N2} y total en dolares: {totalUsd}";
            }
            catch (Exception ex)
            {

                msj = $"Error,{ex.InnerException.ToString()}";
            }
            return msj;
        }




        //[HttpPut]Edit(Object):  Permite realizar el proceso de modificación de un registro anteriormente creado.

        [Authorize]
            [HttpPut]
            [Route("Update")]
            public async Task<string> Edit(Reservation aux)
            {
                string msj = "";
                try
                {

                //validaciones

                if (aux == null)
                    return "Error, no se permiten datos en blanco";
                
                //consulta reservacion existente
                Reservation temp = await this.dbContext.Reservations.FirstOrDefaultAsync(r => r.ID == aux.ID);
                if (temp == null)
                    return $"Error, no existe ninguna reservacion con el ID {aux.ID}";
                //cuanto huespedes
                if (aux.GuestQuantity <= 0)
                    return "Error, la cantidad de huespedes debe ser mayor a 0";
                //cambio de fecha? actualziar
                if (aux.CheckInDate.Date != temp.CheckInDate.Date && aux.CheckInDate.Date < DateTime.Today)
                    return "Error, la fecha de entrada no puede ser antes de hoy";
                
              

                //noches 
                int nights = aux.AccommodationDays;

                //mas de 1 noche
                if (nights <= 0)
                    return "Error, la cantidad de noches debe ser mayor a 0";

             
               
                // tipo de pago valido? si no :         
                if (string.IsNullOrWhiteSpace(aux.PaymentMethod))
                    return "Error, indicar la forma de pago";
                
                //pago en minusculas o mayusculas 
                string pago = aux.PaymentMethod.Trim().ToLower();

                // validacion de pago 

                if (pago == "cheque")
                {
                    if (string.IsNullOrWhiteSpace(aux.CheckNumber))
                        return "Error, con cheque debe indicar el numero de cheque";

                    if (string.IsNullOrWhiteSpace(aux.Bank))
                        return "Error, con cheque debe indicar el banco";
                }
                else if (pago != "efectivo" && pago != "tarjeta")
                {
                    return "Error, la forma de pago debe ser Efectivo, Cheque o Tarjeta";
                }

                //buscar el paquete  para recalcular
                var package = await this.dbContext.Packages.FirstOrDefaultAsync(p => p.ID == temp.PackageId);

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

                double subtotalConDescuento = subtotal - (subtotal * descuento);
                double totalUsd = subtotalConDescuento + (subtotalConDescuento * iva);

                double? tipoCambio = await gometaService.ObtenerTipoCambioAsync();
                if (tipoCambio == null)
                    return "Error, no se pudo obtener el tipo de cambio";

                double totalColones = totalUsd * tipoCambio.Value;


                //actualziar datos
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
                            await this.dbContext.SaveChangesAsync();

                           msj = $"Reservacion del {temp.CheckInDate} modificada correctamente/" +
                           $"Total en colones: {totalColones} y total en dolares: {totalUsd}";
                             }
                                 catch (Exception ex)
                             {
                             msj = $"Error,{ex.InnerException.ToString()}";
            }
                                 return msj;
                              }


        //[HttpDelete] Delete(ID) : Permite eliminar un registro por medio de su identificador
        [Authorize]
        [HttpDelete]
            [Route("Delete")]
            public async Task<string> Delete(int id)
            {
                string msj = "";
                try
                {
                    Reservation aux = await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == id);

                    if (aux != null)
                    {
                        this.dbContext.Reservations.Remove(aux);
                        await this.dbContext.SaveChangesAsync();

                        msj = $"Reservacion del dia {aux.CheckInDate} eliminada correctamente";
                    }
                    else
                    {
                        msj = $"Error, no existe ninguna reservacion con el ID {id}";
                    }
                }
                catch (Exception ex)
                {
                    msj = $"Error,{ex.InnerException.ToString()}";
                }
                return msj;
            }


            //[HttpGet] Search(ID) : Recibe como parámetro un valor especifica para identificar un registro en la base datos.
            [Authorize]
            [HttpGet]
            [Route("Search")]
            public async Task<Reservation> Search(int id)
            {
                return await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == id);
            }


        //[HttpGet] SearchDescrip(value): Recibe un valor para buscar un registro por medio desu descripción.


        [HttpGet]
        [Route("SearchDescrip")]
        [Authorize]
        public async Task<List<Reservation>> SearchDescrip(string value)
        {
            return await this.dbContext.Reservations.Where(x => x.PaymentMethod.Contains(value)).ToListAsync();
        }


        //end
    }
    }
   