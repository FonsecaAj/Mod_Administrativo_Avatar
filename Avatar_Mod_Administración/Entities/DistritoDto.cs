using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class DistritoDto
    {
        [JsonPropertyName("ID_Distrito")]
        public int IdDistrito { get; set; }

        [JsonPropertyName("ID_Provincia")]
        public int IdProvincia { get; set; }

        [JsonPropertyName("ID_Canton")]
        public int IdCanton { get; set; }

        [JsonPropertyName("Nombre_Distrito")]
        public string NombreDistrito { get; set; } = string.Empty;
    }
}
