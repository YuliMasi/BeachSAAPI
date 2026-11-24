using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    /// <summary>
    /// Define las consultas al servicio de GoMeta.
    /// </summary>
    public interface IGometaService
    {
        /// <summary>
        /// Consulta los datos de una cédula.
        /// </summary>
        /// <param name="cedula">Número de cédula, solo dígitos.</param>
        /// <returns>Un usuario con el nombre y el tipo de cédula, o null.</returns>
        Task<User?> ConsultarCedulaAsync(string cedula);

        /// <summary>
        /// Consulta el tipo de cambio de venta del BCCR.
        /// </summary>
        /// <returns>El tipo de cambio, o null si no se pudo obtener.</returns>
        Task<double?> ObtenerTipoCambioAsync();
    }
}