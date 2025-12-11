using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaPeriodoDto
    {
        [JsonPropertyName("id_Periodo")]
        public int ID_Periodo { get; set; }

        [JsonPropertyName("año")]
        public int Año { get; set; }

        [JsonPropertyName("numero_Periodo")]
        public int Numero_Periodo { get; set; }

        [JsonPropertyName("fecha_Inicio")]
        public DateTime Fecha_Inicio { get; set; }

        [JsonPropertyName("fecha_Fin")]
        public DateTime Fecha_Fin { get; set; }


    }
}
