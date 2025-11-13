using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.ADM12_Periodo
{
    public class EliminarModel : BasePageModel
    {
        private readonly IPeriodoApiClient _api;

        public EliminarModel(
            IPeriodoApiClient api,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<EliminarModel> logger)
            : base(auth, usuarioService, logger)
        {
            _api = api;
        }

        public PeriodoDto? Periodo { get; set; }

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            Periodo = await _api.ObtenerPorIdAsync(id);

            if (Periodo == null)
            {
                MensajeError = "Periodo no encontrado.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var ok = await _api.EliminarAsync(id);

            if (ok)
            {
                Mensaje = "Periodo eliminado correctamente.";
            }
            else
            {
                MensajeError = "No se puede eliminar el periodo. Puede estar en uso.";
            }

            return RedirectToPage("Index");
        }
    }
}
