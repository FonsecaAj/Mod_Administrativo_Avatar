using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM11_Profesor
{
    public class EditarModel : BasePageModel
    {
        private readonly IProfesorApiClient _api;

        public EditarModel(
            IProfesorApiClient api,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<EditarModel> logger)
            : base(auth, usuarioService, logger)
        {
            _api = api;
            CargarTiposIdentificacion();
        }

        [BindProperty]
        public ProfesorDto Profesor { get; set; } = new();

        public List<SelectListItem> TiposIdentificacion { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var prof = await _api.ObtenerPorIdAsync(id);
            if (prof == null)
            {
                MensajeError = "Profesor no encontrado.";
                return RedirectToPage("Index");
            }

            Profesor = prof;
            SeleccionarTipoIdentificacion();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            ValidarMayorDeEdad();

            if (string.IsNullOrWhiteSpace(Profesor.TipoIdentificacion))
                ModelState.AddModelError("Profesor.TipoIdentificacion", "Debe seleccionar un tipo de identificación.");

            if (!ModelState.IsValid)
            {
                CargarTiposIdentificacion();
                SeleccionarTipoIdentificacion();
                return Page();
            }

            // 👇 AQUÍ ESTABA EL PROBLEMA
            var (ok, code, msg) = await _api.ActualizarAsync(Profesor);

            if (ok)
            {
                TempData["Mensaje"] = msg;
                return RedirectToPage("Index");
            }

            MensajeError = msg;
            CargarTiposIdentificacion();
            SeleccionarTipoIdentificacion();
            return Page();
        }

        private void ValidarMayorDeEdad()
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - Profesor.FechaNacimiento.Year;

            if (Profesor.FechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            if (edad < 18)
                ModelState.AddModelError("Profesor.FechaNacimiento", "Debe ser mayor de edad.");
        }

        private void CargarTiposIdentificacion()
        {
            TiposIdentificacion = new()
            {
                new SelectListItem("Cédula nacional", "Cédula nacional"),
                new SelectListItem("Pasaporte", "Pasaporte"),
                new SelectListItem("DIMEX", "DIMEX"),
                new SelectListItem("Otro", "Otro")
            };
        }

        private void SeleccionarTipoIdentificacion()
        {
            foreach (var item in TiposIdentificacion)
            {
                item.Selected = (item.Value == Profesor.TipoIdentificacion);
            }
        }
    }
}
