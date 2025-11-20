using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Academico
{
    public class HistorialAcademicoModel : BasePageModel
    {
        private readonly IHistorialAcademicoApiClient _api;
        private readonly IAuthService _authService;
        private readonly ILogger<HistorialAcademicoModel> _logger;

        public HistorialAcademicoModel(IHistorialAcademicoApiClient api, IAuthService authService, IUsuarioService usuarioService, ILogger<HistorialAcademicoModel> logger)
            : base(authService, usuarioService, logger)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public string? Tipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Identificacion { get; set; }

        public List<HistorialAcademicoDto>? Historial { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //  Inicializar sesión y verificar token
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                ErrorMessage = "Token no disponible, vuelva a iniciar sesión.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Tipo) || string.IsNullOrWhiteSpace(Identificacion))
            {
                Message = "Ingrese el tipo y la identificación del estudiante para consultar el historial.";
                return Page();
            }

            var (ok, status, msg, data) = await _api.ObtenerHistorialAsync(Tipo, Identificacion, token);

            if (!ok)
            {
                ErrorMessage = msg ?? $"Error {status}: No se pudo obtener el historial académico.";
                return Page();
            }

            Historial = data;
            Message = msg;
            return Page();
        }
    }
}
