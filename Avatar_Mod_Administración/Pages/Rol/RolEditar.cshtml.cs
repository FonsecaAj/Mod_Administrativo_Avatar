using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Rol
{
    public class RolEditarModel : BasePageModel
    {
        private readonly IRolService _rolService;

        public RolEditarModel(
            IRolService rolService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<RolEditarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _rolService = rolService;
        }

        [BindProperty]
        public RolEditarDto Input { get; set; } = new();

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
                var rol = await _rolService.ObtenerPorIdAsync(Id, token);
                if (rol == null)
                {
                    TempData["Error"] = "El rol solicitado no existe";
                    return RedirectToPage("/Rol/Roles");
                }

                Input = new RolEditarDto { Nombre = rol.Nombre };
                NombreOriginal = rol.Nombre;

                return Page();
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al cargar el rol";
                return RedirectToPage("/Rol/Roles");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            if (string.IsNullOrWhiteSpace(Input.Nombre))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre del rol es requerido");
            }
            else if (!Regex.IsMatch(Input.Nombre.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre del rol solo puede contener letras y espacios");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var token = ObtenerToken()!;
            var dto = new RolCrearDto { Nombre = Input.Nombre.Trim() };

            // Usar mensajes dinámicos de la API
            var (ok, status, message) = await _rolService.ActualizarAsync(Id, dto, token);

            if (ok)
            {
                TempData["Mensaje"] = message;  // Mensaje de la API
                return RedirectToPage("/Rol/Roles");
            }

            // Mostrar mensaje de error de la API
            ModelState.AddModelError(string.Empty, message ?? "Error al actualizar el rol");
            return Page();
        }
    }

    public class RolEditarDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;
    }
}