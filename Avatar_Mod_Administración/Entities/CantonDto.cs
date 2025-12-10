using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class CantonDto
    {
        [JsonPropertyName("ID_Canton")]
        public int IdCanton { get; set; }

        [JsonPropertyName("ID_Provincia")]
        public int IdProvincia { get; set; }

        [JsonPropertyName("Nombre_Canton")]
        public string NombreCanton { get; set; } = string.Empty;
    }
}
