using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.ADM14_Prematricula
{
    public class EliminarModel : BasePageModel
    {
        private readonly IPrematriculaApiClient _prematriculaApi;

        public EliminarModel(
            IPrematriculaApiClient prematriculaApi,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<EliminarModel> logger)
            : base(auth, usuarioService, logger)
        {
            _prematriculaApi = prematriculaApi;
        }

        public PrematriculaDto? Prematricula { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            Prematricula = await _prematriculaApi.ObtenerPorIdAsync(id);

            if (Prematricula == null)
                return RedirectToPage("Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            await _prematriculaApi.EliminarAsync(id);

            return RedirectToPage("Index");
        }
    }
}
