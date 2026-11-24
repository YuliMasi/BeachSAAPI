using BeachSAAPI.Dto;
using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    /// <summary>
    /// Consulta cédulas y el tipo de cambio en apis.gometa.org.
    /// </summary>
    public class GometaService : IGometaService
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Recibe el cliente HTTP configurado para GoMeta.
        /// </summary>
        /// <param name="client">Cliente HTTP configurado con la dirección de GoMeta.</param>
        public GometaService(HttpClient client)
        {
            this.httpClient = client;
        }

        /// <summary>
        /// Consulta una cédula en GoMeta y arma un usuario con sus datos básicos.
        /// </summary>
        /// <param name="cedula">Número de cédula, solo dígitos.</param>
        /// <returns>Un usuario con el nombre y el tipo de cédula, o null si no existe.</returns>
        public async Task<User?> ConsultarCedulaAsync(string cedula)
        {
            var response = await httpClient.GetAsync($"cedulas/{cedula}");

            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadFromJsonAsync<GometaResponse>();
            var persona = data?.Results?.FirstOrDefault();

            if (persona == null)
                return null;

            // Si no viene fullname, lo armamos con las partes
            var nombre = !string.IsNullOrWhiteSpace(persona.FullName)
                ? persona.FullName
                : $"{persona.FirstName} {persona.LastName1} {persona.LastName2}".Trim();

            return new User
            {
                ID = int.TryParse(cedula, out var id) ? id : 0,
                IdType = persona.GuessType,   // ej. FISICA / JURIDICA
                FullName = nombre,
                // El resto lo completa el cliente en el registro
                PhoneNumber = 0,
                Address = "",
                Email = "",
                Password = "",
                Role = "Customer"
            };
        }


        /// <summary>
        /// Consulta el tipo de cambio de venta del BCCR en GoMeta.
        /// </summary>
        /// <returns>El tipo de cambio, o null si no se pudo obtener.</returns>
        public async Task<double?> ObtenerTipoCambioAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("tdc/tdc.json");

                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content.ReadFromJsonAsync<GometaExchangeRate>();

                if (data == null || data.Venta <= 0)
                    return null;

                return data.Venta;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}