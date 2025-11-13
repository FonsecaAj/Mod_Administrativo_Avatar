using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM11_Profesor
{
    public class CrearModel : BasePageModel
    {
        private readonly IProfesorApiClient _api;

        public CrearModel(
            IProfesorApiClient api,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<CrearModel> logger)
            : base(auth, usuarioService, logger)
        {
            _api = api;
        }

        [BindProperty]
        public ProfesorDto Profesor { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            ValidarMayorDeEdad();

            if (!ModelState.IsValid)
            {
                MensajeError = "Hay errores de validación.";
                return Page();
            }

            var ok = await _api.CrearAsync(Profesor);

            if (ok)
                return RedirectToPage("Index");

            MensajeError = "No se pudo crear el profesor.";
            return Page();
        }

        private void ValidarMayorDeEdad()
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - Profesor.FechaNacimiento.Year;

            if (Profesor.FechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            if (edad < 18)
                ModelState.AddModelError("Profesor.FechaNacimiento", "El profesor debe ser mayor de edad.");
        }
    }
}
