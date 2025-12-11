using System.Text.Json.Serialization;

namespace Avatar_Mod_Administración.Entities
{
    public class MatriculaLookupsResponse
    {
        [JsonPropertyName("periodos")]
        public List<MatriculaPeriodoDto> Periodos { get; set; } = new();

        [JsonPropertyName("cursos")]
        public List<MatriculaCursoDto> Cursos { get; set; } = new();

        [JsonPropertyName("grupos")]
        public List<MatriculaGrupoDto> Grupos { get; set; } = new();
    }
}
