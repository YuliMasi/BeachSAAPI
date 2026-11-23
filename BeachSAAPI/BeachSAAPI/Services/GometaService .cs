using BeachSAAPI.Dto;
using BeachSAAPI.Models;

namespace BeachSAAPI.Services
{
    public class GometaService : IGometaService
    {
        private readonly HttpClient httpClient;

        public GometaService(HttpClient client)
        {
            this.httpClient = client;
        }

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
    }
}