using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class IndexModel : PageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;
        private readonly IPeriodoApiClient _periodoApi;
        private readonly ICursoApiClient _cursoApi;

        public IndexModel(
            IPrematriculaApiClient prematriculaApi,
            IPeriodoApiClient periodoApi,
            ICursoApiClient cursoApi)
        {
            _prematriculaApi = prematriculaApi;
            _periodoApi = periodoApi;
            _cursoApi = cursoApi;
        }

        [BindProperty(SupportsGet = true)]
        public PrematriculaFiltro Filtro { get; set; } = new();

        public IEnumerable<PrematriculaDto> Prematriculas { get; set; } = new List<PrematriculaDto>();

        public IEnumerable<SelectListItem> Periodos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Carreras { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            
            var periodos = await _periodoApi.ObtenerTodosAsync(); 

            var periodosFuturos = (periodos ?? Enumerable.Empty<PeriodoDto>())
                .Where(p => p.FechaInicio > DateTime.Today);

            Periodos = periodosFuturos.Select(p => new SelectListItem
            {
                Value = p.IdPeriodo.ToString(),
                Text = $"{p.Anno}-{p.NumeroPeriodo}"
            });

    
            var lookups = await _cursoApi.ObtenerLookupsAsync(); 
            var carrerasLookup = lookups?.Carreras ?? Enumerable.Empty<LookupItem>();

            Carreras = carrerasLookup.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre
            });


            var cursos = await _cursoApi.ObtenerTodosAsync();
            var cursosLista = cursos ?? Enumerable.Empty<CursoDto>();

            Cursos = cursosLista.Select(c => new SelectListItem
            {
                Value = c.ID_Curso.ToString(), 
                Text = c.Nombre
            });

            
            Prematriculas = await _prematriculaApi.ObtenerTodosAsync(Filtro);
        }
    }
}
