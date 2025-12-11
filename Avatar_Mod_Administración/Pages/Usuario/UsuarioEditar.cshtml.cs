using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using System.ComponentModel.DataAnnotations;
using TipoIdentificacionEntity = Avatar_Mod_Administración.Entities.TipoIdentificacion;
using RolEntity = Avatar_Mod_Administración.Entities.Rol;
using UsuarioCrearDto = Avatar_Mod_Administración.Entities.UsuarioCrearDto;

namespace Avatar_Mod_Administración.Pages.Usuario
{
    public class UsuarioEditarModel : BasePageModel
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioEditarModel(
            IUsuarioService usuarioService,
            IAuthService authService,
            ILogger<UsuarioEditarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public UsuarioEditarDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; } = string.Empty;

        public List<TipoIdentificacionEntity> TiposIdentificacion { get; set; } = new();
        public List<RolEntity> Roles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            try
            {
                await CargarCatalogosAsync(token);

                var usuario = await _usuarioService.ObtenerPorEmailAsync(Email, token);
                if (usuario == null)
                    return NotFound();

                Input = new UsuarioEditarDto
                {
                    Email = usuario.Email,
                    IdTipoIdentificacion = usuario.IdTipoIdentificacion,
                    Identificacion = usuario.Identificacion,
                    Nombre = usuario.Nombre,
                    RolDeseado = usuario.RolNombre
                };

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("/Usuario/Usuarios");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            if (string.IsNullOrWhiteSpace(Input.Nombre))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre no puede estar vacío o contener solo espacios");
            }

            if (!ModelState.IsValid)
            {
                await CargarCatalogosAsync(token);
                return Page();
            }

            var emailNuevo = Input.Email.Trim().ToLower();
            if (!emailNuevo.EndsWith("@cuc.cr") && !emailNuevo.EndsWith("@cuc.ac.cr"))
            {
                ModelState.AddModelError("Input.Email", "El email debe pertenecer a los dominios cuc.cr o cuc.ac.cr");
                await CargarCatalogosAsync(token);
                return Page();
            }

            if (emailNuevo.EndsWith("@cuc.cr"))
            {
                Input.RolDeseado = "estudiante";
            }
            else if (emailNuevo.EndsWith("@cuc.ac.cr"))
            {
                if (!string.IsNullOrWhiteSpace(Input.RolDeseado))
                {
                    var rolLower = Input.RolDeseado.ToLower();
                    if (rolLower != "profesor" && rolLower != "administrador")
                    {
                        ModelState.AddModelError("Input.RolDeseado", "Los usuarios con dominio cuc.ac.cr solo pueden ser profesores o administradores");
                        await CargarCatalogosAsync(token);
                        return Page();
                    }
                }
                else
                {
                    Input.RolDeseado = "profesor";
                }
            }

            var dto = new UsuarioCrearDto
            {
                Email = Input.Email,
                IdTipoIdentificacion = Input.IdTipoIdentificacion,
                Identificacion = Input.Identificacion,
                Nombre = Input.Nombre.Trim(),
                Contrasenna = Input.Contrasenna ?? string.Empty,
                RolDeseado = Input.RolDeseado
            };

            var (ok, status, message) = await _usuarioService.ActualizarAsync(Email, dto, token);

            if (ok)
            {
                TempData["Mensaje"] = message;
                return RedirectToPage("/Usuario/Usuarios");
            }

            ModelState.AddModelError(string.Empty, message ?? "Error desconocido");
            await CargarCatalogosAsync(token);
            return Page();
        }

        private async Task CargarCatalogosAsync(string token)
        {
            TiposIdentificacion = await _usuarioService.ObtenerTiposIdentificacionAsync(token);
            Roles = await _usuarioService.ObtenerRolesAsync(token);
        }
    }

    public class UsuarioEditarDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de identificación es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de identificación")]
        public int IdTipoIdentificacion { get; set; }

        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Contrasenna { get; set; }

        public string? RolDeseado { get; set; }
    }
}