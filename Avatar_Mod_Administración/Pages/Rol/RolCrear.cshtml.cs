using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administraci�n.Entities;
using Avatar_Mod_Administraci�n.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administraci�n.Pages.Rol
{
    public class RolCrearModel : BasePageModel
    {
        private readonly IRolService _rolService;

        public RolCrearModel(
            IRolService rolService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<RolCrearModel> logger)
            : base(authService, usuarioService, logger)
        {
            _rolService = rolService;
        }

        [BindProperty]
        public RolInputDto Input { get; set; } = new();

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
                ModelState.AddModelError("Input.Nombre", "El nombre del rol es requerido");
            }
            else if (!Regex.IsMatch(Input.Nombre.Trim(), @"^[a-zA-Z������������\s]+$"))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre del rol solo puede contener letras y espacios");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var token = ObtenerToken()!;
            var dto = new RolCrearDto { Nombre = Input.Nombre.Trim() };

            var (ok, status, message) = await _rolService.CrearAsync(dto, token);

            if (ok)
            {
                TempData["Mensaje"] = message;
                return RedirectToPage("/Rol/Roles");
            }

            ModelState.AddModelError(string.Empty, message ?? "Error al crear el rol");
            return Page();
        }
    }

    public class RolInputDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;
    }
}