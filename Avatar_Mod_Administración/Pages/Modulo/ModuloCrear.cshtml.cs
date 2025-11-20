using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Modulo
{
    public class ModuloCrearModel : BasePageModel
    {
        private readonly IModuloService _moduloService;

        public ModuloCrearModel(
            IModuloService moduloService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ModuloCrearModel> logger)
            : base(authService, usuarioService, logger)
        {
            _moduloService = moduloService;
        }

        [BindProperty]
        public ModuloInputDto Input { get; set; } = new();

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
                return Page();
            }

            var token = ObtenerToken()!;
            var dto = new ModuloCrearDto
            {
                Nombre = Input.Nombre.Trim(),
                Activo = Input.Activo,
                Orden = Input.Orden
            };

            var (ok, status, message) = await _moduloService.CrearAsync(dto, token);

            if (ok)
            {
                TempData["Mensaje"] = message;
                return RedirectToPage("/Modulo/Modulos");
            }

            ModelState.AddModelError(string.Empty, message ?? "Error al crear el módulo");
            return Page();
        }
    }

    public class ModuloInputDto
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