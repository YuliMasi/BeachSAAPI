using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    public interface IGometaService
    {
        Task<User?> ConsultarCedulaAsync(string cedula);

        Task<double> ObtenerTipoCambioAsync();
    }
}