using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM11_Profesor
{
    public class EditarModel : PageModel
    {
        private readonly IProfesorApiClient _api;

        public EditarModel(IProfesorApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public ProfesorDto Profesor { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var prof = await _api.ObtenerPorIdAsync(id);
            if (prof == null)
            {
                MensajeError = "Profesor no encontrado.";
                return RedirectToPage("Index");
            }

            Profesor = prof;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ValidarMayorDeEdad();

            if (!ModelState.IsValid)
            {
                MensajeError = "Hay errores de validación en el formulario.";
                return Page();
            }

            var ok = await _api.ActualizarAsync(Profesor);

            if (ok)
            {
             
                return RedirectToPage("Index");
            }

            MensajeError = "No se pudo actualizar el profesor.";
            return Page();
        }

        private void ValidarMayorDeEdad()
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - Profesor.FechaNacimiento.Year;
            if (Profesor.FechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            if (edad < 18)
            {
                ModelState.AddModelError("Profesor.FechaNacimiento",
                    "El profesor debe ser mayor de edad.");
            }
        }
    }
}
