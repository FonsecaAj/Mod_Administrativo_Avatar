namespace ApiMAT2.Entities
{
    public class MatriculaLookupsDto
    {
        public IEnumerable<PeriodoMatriculaDto> Periodos { get; set; } = new List<PeriodoMatriculaDto>();
        public IEnumerable<CursoMatriculaDto> Cursos { get; set; } = new List<CursoMatriculaDto>();
        public IEnumerable<GrupoMatriculaDto> Grupos { get; set; } = new List<GrupoMatriculaDto>();
    }

}

