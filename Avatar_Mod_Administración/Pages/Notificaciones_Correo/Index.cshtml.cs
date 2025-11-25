using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.Notificaciones_Correo
{
    public class IndexModel : BasePageModel
    {
        private readonly INotificacionesApiClient _api;

        public IndexModel(
            INotificacionesApiClient api,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(authService, usuarioService, logger)
        {
            _api = api;
        }

        [BindProperty]
        public NotificacionRequestDto Formulario { get; set; } = new();

        public string? Resultado { get; set; }
        public bool EsError { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken();

            var sanitizer = new HtmlSanitizer();
            Formulario.Mensaje = sanitizer.Sanitize(Formulario.Mensaje);

            if (!ValidarCorreos(Formulario.Email))
            {
                ModelState.AddModelError("Formulario.Email", "Uno o más correos tienen formato inválido.");
                return Page();
            }

            var resp = await _api.EnviarCorreoAsync(Formulario, token);

            Resultado = resp?.Message;
            EsError = resp?.StatusCode != 200;

            // AGREGAR ESTO: Si fue exitoso, limpiamos el formulario para que quede vacío
            if (!EsError)
            {
                Formulario = new NotificacionRequestDto();
                ModelState.Clear(); // Limpia los errores previos o validaciones visuales
            }

            return Page();
        }

        private bool ValidarCorreos(string emails)
        {
            var lista = emails.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var e in lista)
            {
                var email = e.Trim();
                if (!email.Contains("@") || !email.Contains("."))
                    return false;
            }

            return true;
        }
    }
}
