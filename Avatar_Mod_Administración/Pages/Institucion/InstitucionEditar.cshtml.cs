using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Institucion
{
    public class InstitucionEditarModel : BasePageModel
    {
        private readonly IInstitucionService _institucionService;

        public InstitucionEditarModel(
            IInstitucionService institucionService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<InstitucionEditarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _institucionService = institucionService;
        }

        [BindProperty]
        public InstitucionEditarDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string NombreOriginal { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            try
            {
                var token = ObtenerToken()!;
                var institucion = await _institucionService.ObtenerPorIdAsync(Id, token);
                if (institucion == null)
                {
                    TempData["Error"] = "La institución solicitada no existe";
                    return RedirectToPage("/Institucion/Instituciones");
                }

                Input = new InstitucionEditarDto { Nombre = institucion.Nombre };
                NombreOriginal = institucion.Nombre;

                return Page();
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al cargar la institución";
                return RedirectToPage("/Institucion/Instituciones");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;
            var institucionOriginal = await _institucionService.ObtenerPorIdAsync(Id, token);
            if (institucionOriginal == null)
            {
                TempData["Error"] = "La institución no existe";
                return RedirectToPage("/Institucion/Instituciones");
            }

            // Validaciones...
            if (string.IsNullOrWhiteSpace(Input.Nombre))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre es requerido");
            }
            else
            {
                var nombreTrimmed = Input.Nombre.Trim();

                if (!Regex.IsMatch(nombreTrimmed, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre solo puede contener letras y espacios");
                }

                if (nombreTrimmed.Length < 3 || nombreTrimmed.Length > 100)
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre debe tener entre 3 y 100 caracteres");
                }

                if (Regex.IsMatch(nombreTrimmed, @"\s{2,}"))
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre no puede contener espacios consecutivos");
                }
            }

            if (!ModelState.IsValid)
            {
                NombreOriginal = institucionOriginal.Nombre;
                return Page();
            }

            try
            {
                var dto = new InstitucionCrearDto { Nombre = Input.Nombre.Trim() };
                var institucionActualizada = await _institucionService.ActualizarAsync(Id, dto, token);

                if (institucionActualizada != null)
                {
                    TempData["Mensaje"] = $"Institución actualizada exitosamente a '{institucionActualizada.Nombre}'";
                    return RedirectToPage("/Institucion/Instituciones");
                }

                ModelState.AddModelError(string.Empty, "Error al actualizar la institución.");
                NombreOriginal = institucionOriginal.Nombre;
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la institución.");
                NombreOriginal = institucionOriginal.Nombre;
                return Page();
            }
        }
    }

    public class InstitucionEditarDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;
    }
}