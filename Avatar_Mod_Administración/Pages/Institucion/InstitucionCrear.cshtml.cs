using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Institucion
{
    public class InstitucionCrearModel : BasePageModel
    {
        private readonly IInstitucionService _institucionService;

        public InstitucionCrearModel(
            IInstitucionService institucionService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<InstitucionCrearModel> logger)
            : base(authService, usuarioService, logger)
        {
            _institucionService = institucionService;
        }

        [BindProperty]
        public InstitucionInputDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            // Validación del nombre
            if (string.IsNullOrWhiteSpace(Input.Nombre))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre de la institución es requerido");
            }
            else
            {
                var nombreTrimmed = Input.Nombre.Trim();

                if (!Regex.IsMatch(nombreTrimmed, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre solo puede contener letras y espacios");
                }

                if (nombreTrimmed.Length < 3)
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre debe tener al menos 3 caracteres");
                }

                if (nombreTrimmed.Length > 100)
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre no puede exceder los 100 caracteres");
                }

                if (Regex.IsMatch(nombreTrimmed, @"\s{2,}"))
                {
                    ModelState.AddModelError("Input.Nombre", "El nombre no puede contener espacios consecutivos");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var token = ObtenerToken()!;
                var dto = new InstitucionCrearDto { Nombre = Input.Nombre.Trim() };

                // Usar mensajes dinámicos de la API
                var (ok, status, message) = await _institucionService.CrearAsync(dto, token);

                if (ok)
                {
                    TempData["Mensaje"] = message;  // Mensaje de la API
                    return RedirectToPage("/Institucion/Instituciones");
                }

                // Mostrar mensaje de error de la API
                ModelState.AddModelError(string.Empty, message ?? "Error al crear la institución");
                return Page();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la institución.");
                return Page();
            }
        }
    }

    public class InstitucionInputDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;
    }
}