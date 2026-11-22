using BeachSAAPI.Dto;
using BeachSAAPI.Models;
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

        public ReservationsController(DbContextBeachSA _context)
        {
            this.dbContext = _context;
        }



        //           [HttpGet] List : Permite mostrar la lista de la información correspondiente
        //[Authorize]
        [HttpGet]
        [Route("List")]
        public List<Reservation> List()
        {
            return this.dbContext.Reservations.ToList();
        }


        //          [HttpPost]Create(Object) :  Permite registrar  los datos en la base datos
        //[Authorize]
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


                //noches
                int nights = reservation.AccommodationDays;


                //validaciones de metodo de pago 

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


                //validacion paquete 
                var package = await this.dbContext.Packages.FirstOrDefaultAsync(p => p.ID == reservation.PackageId);
                if (package == null)
                    return "Error, el paquete no existe";

                //calcular subtotal, descuento, iva y total

                    //costo de la noche por persona
   
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

                //por ahora fijo, voy a usar http://apis.gometa.org/tdc/tdc.json
                double tipoCambio = 550;

                //cambio a colones
                double totalColones = totalUsd * tipoCambio;



                reservation.Discount = descuento;
                reservation.IVA = iva;


                //crear
                this.dbContext.Reservations.Add(reservation);

                await this.dbContext.SaveChangesAsync();
                msj = $"Reservacion para dia  {reservation.CheckInDate} registrada correctamente/" +
                    $"Total en colones: {totalColones} y total en dolares: {totalUsd}";
            }
            catch (Exception ex)
            {

                msj = $"Error,{ex.InnerException.ToString()}";
            }
            return msj;
        }




        //          [HttpPut]Edit(Object):  Permite realizar el proceso de modificación de un registro anteriormente creado.

        [Authorize]
            [HttpPut]
            [Route("Update")]
            public async Task<string> Edit(Reservation aux)
            {
                string msj = "";
                try
                {
                    if (aux != null)
                    {
                        Reservation temp = await this.dbContext.Reservations.FirstOrDefaultAsync(r => r.ID == aux.ID);

                        if (temp != null)
                        {

                            temp.AccommodationDays = aux.AccommodationDays;
                            temp.Discount = aux.Discount;
                            temp.IVA = aux.IVA;
                            temp.PaymentMethod = aux.PaymentMethod;
                            temp.CheckNumber = aux.CheckNumber;
                            temp.Bank = aux.Bank;
                            temp.GuestQuantity = aux.GuestQuantity;
                            temp.CheckInDate = aux.CheckInDate;
                            temp.CheckOutDate = aux.CheckOutDate;

                            this.dbContext.Reservations.Update(temp);
                            await this.dbContext.SaveChangesAsync();

                            msj = $"Reservacion del dia {temp.CheckInDate} modificada correctamente";
                        }
                        else
                        {
                            msj = $"Error, no existe ninguna reservacion con el ID {aux.ID}";
                        }
                    }
                    else
                    {
                        msj = "Error, no se permiten datos en blanco";
                    }
                }
                catch (Exception ex)
                {
                    msj = $"Error,{ex.InnerException.ToString()}";
                }
                return msj;
            }



            //          [HttpDelete] Delete(ID) : Permite eliminar un registro por medio de su identificador
            //[Authorize]
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


            //          [HttpGet] Search(ID) : Recibe como parámetro un valor especifica para identificar un registro en la base datos.
            //[Authorize]
            [HttpGet]
            [Route("Search")]
            public async Task<Reservation> Search(int id)
            {
                return await this.dbContext.Reservations.FirstOrDefaultAsync(x => x.ID == id);
            }


            //          [HttpGet] SearchDescrip(value): Recibe un valor para buscar un registro por medio desu descripción.
            // [Authorize]
            [HttpGet]
            [Route("SearchPaymentMethod")]
            public async Task<List<Reservation>> SearchPaymentMethod(string paymentMethod)
            {
                return await this.dbContext.Reservations.Where(x => x.PaymentMethod.Contains(paymentMethod)).ToListAsync();
            }





            //end
        }
    }
   