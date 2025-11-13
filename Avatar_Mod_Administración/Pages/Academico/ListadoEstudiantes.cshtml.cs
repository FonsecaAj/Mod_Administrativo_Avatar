using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Academico
{
    public class ListadoEstudiantesModel : PageModel
    {
        private readonly IListadoEstudiantesApiClient _api;

        public ListadoEstudiantesModel(IListadoEstudiantesApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public int Periodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Carrera { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Curso { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Grupo { get; set; }

        public List<EstudiantesListadoDto>? Estudiantes { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (Periodo == 0)
            {
                Message = "Ingrese el número de período para obtener el listado.";
                return;
            }

            var (ok, status, msg, data) = await _api.ObtenerListadoAsync(Periodo);

            if (!ok)
            {
                ErrorMessage = msg ?? $"Error {status}: No se pudo obtener el listado.";
                return;
            }

            Estudiantes = data;
            Message = msg;
        }
    }
}
