using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.ADM12_Periodo
{
    public class IndexModel : BasePageModel
    {
        private readonly IPeriodoApiClient _api;

        public IndexModel(
            IPeriodoApiClient api,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(auth, usuarioService, logger)
        {
            _api = api;
        }

        public List<PeriodoDto> Periodos { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var lista = (await _api.ObtenerTodosAsync()).ToList();

            Periodos = lista
                .OrderBy(p => p.Anno)
                .ThenBy(p => p.NumeroPeriodo)
                .ToList();

            return Page();
        }
    }
}
