using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Pages.Permisos
{
    public class PermisosModel : BasePageModel
    {
        private readonly IRolService _rolService;
        private readonly IModuloService _moduloService;

        public PermisosModel(
            IRolService rolService,
            IModuloService moduloService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<PermisosModel> logger)
            : base(authService, usuarioService, logger)
        {
            _rolService = rolService;
            _moduloService = moduloService;
        }

        public List<RolApi> Roles { get; set; } = new();
        public List<Entities.Modulo> Modulos { get; set; } = new();

        [BindProperty]
        public int RolSeleccionado { get; set; }

        [BindProperty]
        public string ModulosSeleccionados { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken();

            try
            {
                Roles = await _rolService.ObtenerTodosAsync(token);
                Modulos = await _moduloService.ObtenerTodosAsync(token);

                _logger.LogInformation("Pagina de permisos cargada: {RolesCount} roles, {ModulosCount} módulos",
                    Roles.Count, Modulos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos de roles y módulos");
                TempData["Error"] = "Error al cargar los datos. Por favor, intente nuevamente.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken();

            // Validación Rol es requerido
            if (RolSeleccionado == 0)
            {
                TempData["Error"] = "Debe seleccionar un rol";
                _logger.LogWarning("Intento de guardar sin seleccionar rol");
                return RedirectToPage();
            }

            try
            {
                // Parsear IDs de módulos seleccionados
                var idsModulos = string.IsNullOrWhiteSpace(ModulosSeleccionados)
                    ? new List<int>()
                    : ModulosSeleccionados.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => {
                            if (int.TryParse(s, out int id))
                                return id;
                            return 0;
                        })
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList();

                _logger.LogInformation("Actualizando permisos del rol {RolId}: {ModulosCount} módulos",
                    RolSeleccionado, idsModulos.Count);

                var resultado = await _rolService.AsignarModulosAsync(
                    RolSeleccionado,
                    idsModulos,
                    token);

                if (resultado)
                {
                    TempData["Mensaje"] = $"Permisos actualizados exitosamente. Total de modulos asignados: {idsModulos.Count}";
                    _logger.LogInformation("Permisos actualizados correctamente para rol {RolId}", RolSeleccionado);
                }
                else
                {
                    TempData["Error"] = "Error al actualizar permisos. Verifique que el rol exista y que los modulos sean validos.";
                    _logger.LogWarning("Fallo al actualizar permisos para rol {RolId}", RolSeleccionado);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar actualizacion de permisos para rol {RolId}", RolSeleccionado);
                TempData["Error"] = "Ocurrio un error inesperado al guardar los permisos.";
            }

            return RedirectToPage();
        }
    }
}