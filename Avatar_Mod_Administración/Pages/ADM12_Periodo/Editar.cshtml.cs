using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM12_Periodo
{
    public class EditarModel : PageModel
    {
        private readonly IPeriodoApiClient _api;

        public EditarModel(IPeriodoApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public PeriodoDto Periodo { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var periodo = await _api.ObtenerPorIdAsync(id);
            if (periodo == null)
            {
                MensajeError = "Periodo no encontrado.";
                return RedirectToPage("Index");
            }

            Periodo = periodo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ValidarFechasYEstado();

            if (!ModelState.IsValid)
            {
                MensajeError = "Hay errores de validación en el formulario.";
                return Page();
            }

            var ok = await _api.ActualizarAsync(Periodo);

            if (ok)
            {
                Mensaje = "Periodo actualizado correctamente.";
                return RedirectToPage("Index");
            }

            MensajeError = "No se pudo actualizar el periodo. Revise solapamientos o duplicados.";
            return Page();
        }

        private void ValidarFechasYEstado()
        {
            if (Periodo.FechaFin < Periodo.FechaInicio)
            {
                ModelState.AddModelError("Periodo.FechaFin",
                    "La fecha de fin debe ser mayor o igual a la fecha de inicio.");
            }

            var estadoCalculado = CalcularEstadoPorFechas(Periodo.FechaInicio, Periodo.FechaFin);

            if (!string.Equals(Periodo.Estado, estadoCalculado, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Periodo.Estado",
                    $"Para esas fechas el estado correcto sería '{estadoCalculado}'.");
            }
        }

        private string CalcularEstadoPorFechas(DateTime inicio, DateTime fin)
        {
            var hoy = DateTime.Today;

            if (inicio.Date > hoy) return "Futuro";
            if (fin.Date < hoy) return "Cerrado";
            return "Activo";
        }
    }
}
