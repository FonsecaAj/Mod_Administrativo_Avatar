using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;
using Microsoft.Extensions.Logging;

namespace Avatar_Mod_Administración.Pages.Perfil
{
    public class IndexModel : BasePageModel
    {
        private readonly IPerfilUsuarioApiClient _perfilApi;

        // -------- Datos expuestos a la Vista --------
        public PerfilUsuarioDto? Perfil { get; set; }

        public IndexModel(
            IAuthService authService,
            IUsuarioService usuarioService,
            IPerfilUsuarioApiClient perfilApiClient,
            ILogger<IndexModel> logger)
            : base(authService, usuarioService, logger)
        {
            _perfilApi = perfilApiClient;
        }

        public async Task<IActionResult> OnGet()
        {
            // validar sesion + renovar token + setear viewdata
            var validar = await InicializarSesionAsync();
            if (validar != null)
                return validar;

            try
            {
                var token = ObtenerToken();
                var email = UsuarioEmail; // Ya viene por sesión

                var (ok, statusCode, message, data) = await _perfilApi.ObtenerPerfilAsync(email, token);

                if (!ok || data == null)
                {
                    _logger.LogWarning("Error obteniendo perfil: {Message}", message);
                    Perfil = null;
                }
                else
                {
                    Perfil = data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando perfil del usuario");
                Perfil = null;
            }

            return Page();
        }
    }
}
