using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Rubros
{
    public class Obtener_DesgloseModel : PageModel
    {

        private readonly IRubroApiClient _api;

        public Obtener_DesgloseModel(IRubroApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public int ID_Grupo { get; set; }
        public List<RubroDto>? Rubros { get; set; }
        public string? ErrorMessage { get; set; }
        public string? InfoMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Si no se ingresó un ID todavía, mostramos solo el mensaje inicial
            if (ID_Grupo == 0)
            {
                InfoMessage = "Ingrese el ID del grupo para consultar su desglose.";
                return;
            }

            var (ok, statusCode, message, data) = await _api.ObtenerDesglose(ID_Grupo);

            if (!ok)
            {
                ErrorMessage = message ?? $"Error {statusCode}: No se pudo obtener el desglose.";
                return;
            }

            if (data == null || data.Count == 0)
            {
                InfoMessage = "No hay desglose cargado para este grupo.";
                return;
            }

            Rubros = data;
            InfoMessage = message;
        }

    }
}
