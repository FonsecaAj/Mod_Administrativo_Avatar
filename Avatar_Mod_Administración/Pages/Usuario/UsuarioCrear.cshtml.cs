using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using System.ComponentModel.DataAnnotations;
using TipoIdentificacionEntity = Avatar_Mod_Administración.Entities.TipoIdentificacion;
using RolEntity = Avatar_Mod_Administración.Entities.Rol;
using UsuarioCrearDto = Avatar_Mod_Administración.Entities.UsuarioCrearDto;

namespace Avatar_Mod_Administración.Pages.Usuario
{
    public class UsuarioCrearModel : BasePageModel
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioCrearModel(
            IUsuarioService usuarioService,
            IAuthService authService,
            ILogger<UsuarioCrearModel> logger)
            : base(authService, usuarioService, logger)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public UsuarioInputDto Input { get; set; } = new();

        public List<TipoIdentificacionEntity> TiposIdentificacion { get; set; } = new();
        public List<RolEntity> Roles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            await CargarCatalogosAsync(ObtenerToken()!);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            // Validación manual del nombre (no vacío ni solo espacios)
            if (string.IsNullOrWhiteSpace(Input.Nombre))
            {
                ModelState.AddModelError("Input.Nombre", "El nombre no puede estar vacío o contener solo espacios");
            }

            if (!ModelState.IsValid)
            {
                await CargarCatalogosAsync(token);
                return Page();
            }

            // Validar dominio
            var email = Input.Email.Trim().ToLower();
            if (!email.EndsWith("@cuc.cr") && !email.EndsWith("@cuc.ac.cr"))
            {
                ModelState.AddModelError("Input.Email", "El email debe pertenecer a los dominios cuc.cr o cuc.ac.cr");
                await CargarCatalogosAsync(token);
                return Page();
            }

            // Validar y asignar rol según dominioa
            if (email.EndsWith("@cuc.cr"))
            {
                Input.RolDeseado = "estudiante";
            }
            else if (email.EndsWith("@cuc.ac.cr"))
            {
                if (string.IsNullOrWhiteSpace(Input.RolDeseado))
                {
                    Input.RolDeseado = "profesor";
                }
                else
                {
                    var rolLower = Input.RolDeseado.ToLower();
                    if (rolLower != "profesor" && rolLower != "administrador")
                    {
                        ModelState.AddModelError("Input.RolDeseado", "Los usuarios con dominio cuc.ac.cr solo pueden ser profesores o administradores");
                        await CargarCatalogosAsync(token);
                        return Page();
                    }
                }
            }

            var dto = new UsuarioCrearDto
            {
                Email = Input.Email.Trim(),
                IdTipoIdentificacion = Input.IdTipoIdentificacion,
                Identificacion = Input.Identificacion.Trim(),
                Nombre = Input.Nombre.Trim(),
                Contrasenna = Input.Contrasenna,
                RolDeseado = Input.RolDeseado
            };

            var resultado = await _usuarioService.CrearAsync(dto, token);

            if (resultado)
            {
                TempData["Mensaje"] = "Usuario creado exitosamente";
                return RedirectToPage("/Usuario/Usuarios");
            }

            ModelState.AddModelError(string.Empty, "Error al crear el usuario. Verifique que el email no esté registrado.");
            await CargarCatalogosAsync(token);
            return Page();
        }

        private async Task CargarCatalogosAsync(string token)
        {
            TiposIdentificacion = await _usuarioService.ObtenerTiposIdentificacionAsync(token);
            Roles = await _usuarioService.ObtenerRolesAsync(token);
        }
    }

    public class UsuarioInputDto
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

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Contrasenna { get; set; } = string.Empty;

        public string? RolDeseado { get; set; }
    }
}