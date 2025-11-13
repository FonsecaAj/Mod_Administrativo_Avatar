
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administracion.Pages.ADM10_Cursos
{
    public class IndexModel : PageModel
    {
        private readonly ICursoApiClient _cursoClient;

        public IndexModel(ICursoApiClient cursoClient)
        {
            _cursoClient = cursoClient;
        }

        public List<CursoDto> Cursos { get; set; } = new();
        public List<SelectListItem> Carreras { get; set; } = new();
        public List<SelectListItem> Niveles { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int IdCarreraSeleccionada { get; set; }

        [BindProperty(SupportsGet = true)]
        public int NivelSeleccionado { get; set; }

        public async Task OnGetAsync()
        {
        
            var lookups = await _cursoClient.ObtenerLookupsAsync();

            if (lookups != null)
            {
                Carreras = lookups.Carreras
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
                    .ToList();

                Niveles = lookups.Niveles
                    .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Nombre })
                    .ToList();
            }

            Carreras.Insert(0, new SelectListItem("Seleccione una carrera", "0"));
            Niveles.Insert(0, new SelectListItem("Todos los niveles", "0"));

   
            var cursos = await _cursoClient.ObtenerTodosAsync();

            if (IdCarreraSeleccionada > 0)
                cursos = cursos.Where(c => c.ID_Carrera == IdCarreraSeleccionada);

            if (NivelSeleccionado > 0)
                cursos = cursos.Where(c => c.Nivel == NivelSeleccionado);

            Cursos = cursos.ToList();
        }
    }
}
