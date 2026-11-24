using System.Text.Json.Serialization;

namespace BeachSAAPI.Dto
{
    /// <summary>
    /// Respuesta del servicio de cédulas de GoMeta.
    /// </summary>
    public class GometaResponse
    {
        /// <summary>
        /// Personas encontradas para la cédula.
        /// </summary>
        [JsonPropertyName("results")]
        public List<GometaPerson> Results { get; set; } = new();

        /// <summary>
        /// Cantidad de resultados.
        /// </summary>
        [JsonPropertyName("resultcount")]
        public int ResultCount { get; set; }
    }

    /// <summary>
    /// Persona encontrada en el servicio de cédulas.
    /// </summary>
    public class GometaPerson
    {
        /// <summary>
        /// Número de cédula.
        /// </summary>
        [JsonPropertyName("cedula")]
        public string Cedula { get; set; }

        /// <summary>
        /// Nombre completo.
        /// </summary>
        [JsonPropertyName("fullname")]
        public string FullName { get; set; }

        /// <summary>
        /// Nombre.
        /// </summary>
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }

        /// <summary>
        /// Primer apellido.
        /// </summary>
        [JsonPropertyName("lastname1")]
        public string LastName1 { get; set; }

        /// <summary>
        /// Segundo apellido.
        /// </summary>
        [JsonPropertyName("lastname2")]
        public string LastName2 { get; set; }

        /// <summary>
        /// Tipo de cédula estimado por GoMeta.
        /// </summary>
        [JsonPropertyName("guess_type")]
        public string GuessType { get; set; }
    }
}