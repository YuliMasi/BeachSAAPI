using System.Text.Json.Serialization;
namespace BeachSAAPI.Dto
{
    /// <summary>
    /// Respuesta del servicio de tipo de cambio de GoMeta.
    /// </summary>
    public class GometaExchangeRate
    {

        /// <summary>
        /// Tipo de cambio de venta del BCCR.
        /// </summary>
        [JsonPropertyName("venta")]
        public double Venta { get; set; }

    }
}
