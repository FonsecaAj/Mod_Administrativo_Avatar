using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class ProvinciaDto
    {
        [JsonPropertyName("ID_Provincia")]
        public int IdProvincia { get; set; }

        [JsonPropertyName("Nombre_Provincia")]
        public string NombreProvincia { get; set; } = string.Empty;
    }
}
