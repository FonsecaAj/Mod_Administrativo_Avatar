using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Modulo
{
    public class ModuloEditarModel : BasePageModel
    {
        private readonly IModuloService _moduloService;

        public ModuloEditarModel(
            IModuloService moduloService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ModuloEditarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _moduloService = moduloService;
        }

        [BindProperty]
        public ModuloEditarDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string NombreOriginal { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            try
            {
                var modulo = await _moduloService.ObtenerPorIdAsync(Id, token);
                if (modulo == null)
                {
                    TempData["Error"] = "El módulo solicitado no existe";
                    return RedirectToPage("/Modulo/Modulos");
                }

                Input = new ModuloEditarDto
                {
                    Nombre = modulo.Nombre,
                    Activo = modulo.Activo,
                    Orden = modulo.Orden
                };

                NombreOriginal = modulo.Nombre;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el módulo {Id}", Id);
                TempData["Error"] = "Error al cargar el módulo";
                return RedirectToPage("/Modulo/Modulos");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            var moduloOriginal = await _moduloService.ObtenerPorIdAsync(Id, token);
            if (moduloOriginal == null)
            {
                TempData["Error"] = "El módulo no existe";
                return RedirectToPage("/Modulo/Modulos");
            }

            if (string.IsNullOrWhiteSpace(Input.Nombre))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre del módulo es requerido");
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
                NombreOriginal = moduloOriginal.Nombre;
                return Page();
            }

            try
            {
                var dto = new ModuloCrearDto
                {
                    Nombre = Input.Nombre.Trim(),
                    Activo = Input.Activo,
                    Orden = Input.Orden
                };

                // Usar mensajes dinámicos de la API 
                var (ok, status, message) = await _moduloService.ActualizarAsync(Id, dto, token);

                if (ok)
                {
                    TempData["Mensaje"] = message;  // Mensaje de la API
                    return RedirectToPage("/Modulo/Modulos");
                }

                // Mostrar mensaje de error de la API
                ModelState.AddModelError(string.Empty, message ?? "Error al actualizar el módulo");
                NombreOriginal = moduloOriginal.Nombre;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el módulo {Id}", Id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el módulo. Intente nuevamente.");
                NombreOriginal = moduloOriginal.Nombre;
                return Page();
            }
        }
    }

    public class ModuloEditarDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "El orden es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El orden debe ser un número mayor o igual a 0")]
        public int Orden { get; set; } = 0;
    }
}