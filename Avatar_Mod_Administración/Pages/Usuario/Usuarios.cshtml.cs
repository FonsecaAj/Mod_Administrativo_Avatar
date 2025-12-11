using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using UsuarioEntity = Avatar_Mod_Administración.Entities.Usuario;
using TipoIdentificacionEntity = Avatar_Mod_Administración.Entities.TipoIdentificacion;
using RolEntity = Avatar_Mod_Administración.Entities.Rol;

namespace Avatar_Mod_Administración.Pages.Usuario
{
    public class UsuariosModel : BasePageModel
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosModel(
            IUsuarioService usuarioService,
            IAuthService authService,
            ILogger<UsuariosModel> logger)
            : base(authService, usuarioService, logger)
        {
            _usuarioService = usuarioService;
        }

        public List<UsuarioEntity> Usuarios { get; set; } = new();
        public List<TipoIdentificacionEntity> TiposIdentificacion { get; set; } = new();
        public List<RolEntity> Roles { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FiltroIdentificacion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroNombre { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroTipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroDominio { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroRol { get; set; }

        [BindProperty(SupportsGet = true)]
        public string OrdenarPor { get; set; } = "Email";

        [BindProperty(SupportsGet = true)]
        public bool Descendente { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int ElementosPorPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            try
            {
                // Cargar catálogos
                TiposIdentificacion = await _usuarioService.ObtenerTiposIdentificacionAsync(token);
                Roles = await _usuarioService.ObtenerRolesAsync(token);

                // Aplicar filtros
                var usuariosFiltrados = await AplicarFiltrosAsync(token);

                // Ordenar
                usuariosFiltrados = OrdenarUsuarios(usuariosFiltrados);

                // Paginar
                TotalRegistros = usuariosFiltrados.Count;
                TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)ElementosPorPagina);
                PaginaActual = Math.Max(1, Math.Min(PaginaActual, TotalPaginas == 0 ? 1 : TotalPaginas));

                Usuarios = usuariosFiltrados
                    .Skip((PaginaActual - 1) * ElementosPorPagina)
                    .Take(ElementosPorPagina)
                    .ToList();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                Usuarios = new List<UsuarioEntity>();
            }

            return Page();
        }

        private async Task<List<UsuarioEntity>> AplicarFiltrosAsync(string token)
        {
            var usuarios = await _usuarioService.FiltrarAsync(FiltroIdentificacion, FiltroNombre, FiltroTipo, token);

            if (!string.IsNullOrWhiteSpace(FiltroDominio))
            {
                usuarios = usuarios.Where(u => u.Email.EndsWith($"@{FiltroDominio}")).ToList();
            }

            if (FiltroRol.HasValue)
            {
                usuarios = usuarios.Where(u => u.IdRol == FiltroRol.Value).ToList();
            }

            return usuarios;
        }

        private List<UsuarioEntity> OrdenarUsuarios(List<UsuarioEntity> usuarios)
        {
            var ordenados = OrdenarPor switch
            {
                "Email" => Descendente ? usuarios.OrderByDescending(u => u.Email) : usuarios.OrderBy(u => u.Email),
                "TipoIdentificacion" => Descendente ? usuarios.OrderByDescending(u => u.TipoIdentificacion) : usuarios.OrderBy(u => u.TipoIdentificacion),
                "Identificacion" => Descendente ? usuarios.OrderByDescending(u => u.Identificacion) : usuarios.OrderBy(u => u.Identificacion),
                "Nombre" => Descendente ? usuarios.OrderByDescending(u => u.Nombre) : usuarios.OrderBy(u => u.Nombre),
                "RolNombre" => Descendente ? usuarios.OrderByDescending(u => u.RolNombre) : usuarios.OrderBy(u => u.RolNombre),
                _ => usuarios.OrderBy(u => u.Email)
            };

            return ordenados.ToList();
        }

        public async Task<IActionResult> OnPostEliminarAsync(string email)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            var (ok, status, message) = await _usuarioService.EliminarAsync(email, token);

            if (ok)
                TempData["Mensaje"] = message;
            else
                TempData["Error"] = message;

            return RedirectToPage();
        }
    }
}