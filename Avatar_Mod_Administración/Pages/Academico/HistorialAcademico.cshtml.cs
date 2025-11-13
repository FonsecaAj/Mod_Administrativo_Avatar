using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Academico
{
    public class HistorialAcademicoModel : PageModel
    {

        private readonly IHistorialAcademicoApiClient _api;


        public HistorialAcademicoModel(IHistorialAcademicoApiClient api)
        {
            _api = api;
        }


        [BindProperty(SupportsGet = true)]
        public string? Tipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Identificacion { get; set; }

        public List<HistorialAcademicoDto>? Historial { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Tipo) || string.IsNullOrWhiteSpace(Identificacion))
            {
                Message = "Ingrese el tipo y la identificación del estudiante para consultar el historial.";
                return;
            }

            var (ok, status, msg, data) = await _api.ObtenerHistorialAsync(Tipo, Identificacion);

            if (!ok)
            {
                ErrorMessage = msg ?? $"Error {status}: No se pudo obtener el historial académico.";
                return;
            }

            Historial = data;
            Message = msg;
        }


    }
}
