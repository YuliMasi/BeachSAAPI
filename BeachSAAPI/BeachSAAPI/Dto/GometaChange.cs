using System.Text.Json.Serialization;
namespace BeachSAAPI.Dto
{
    public class GometaChange
    {

        [JsonPropertyName("venta")]
        public double Venta { get; set; }

        [JsonPropertyName("compra")]
        public double Compra { get; set; }

    }
}
