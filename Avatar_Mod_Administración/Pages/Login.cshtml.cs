using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;

namespace Avatar_Mod_Administración.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public LoginRequest Input { get; set; } = new();

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Verificación simple y rápida sin try-catch innecesario
            var sesion = _authService.ObtenerSesionActual();
            if (sesion != null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _authService.LoginAsync(Input.Email, Input.Contrasenna);

            if (response == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario y/o contraseña incorrectos");
                return Page();
            }

            _authService.GuardarSesion(response, Input.RecordarSesion);

            return RedirectToPage("/Index");
        }
    }
}