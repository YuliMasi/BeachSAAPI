using BeachSAAPI.Dto;
using Microsoft.AspNetCore.Mvc;

using BeachSAAPI.Models;
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


        //endpoint para listar todas las reservas
        [HttpGet]
        [Route("List")]
        public List<Reservation> List()
        {
            return this.dbContext.Reservations.ToList();
        }








        //end
    }
}   