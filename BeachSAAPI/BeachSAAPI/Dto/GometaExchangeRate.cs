using System.Text.Json.Serialization;
namespace BeachSAAPI.Dto
{
    public class GometaExchangeRate
    {

        [JsonPropertyName("venta")]
        public double Venta { get; set; }

    }
}
