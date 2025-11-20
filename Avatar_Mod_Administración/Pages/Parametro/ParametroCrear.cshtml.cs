using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Parametro
{
    public class ParametroCrearModel : BasePageModel
    {
        private readonly IParametroService _parametroService;

        public ParametroCrearModel(
            IParametroService parametroService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ParametroCrearModel> logger)
            : base(authService, usuarioService, logger)
        {
            _parametroService = parametroService;
        }

        [BindProperty]
        public ParametroInputDto Input { get; set; } = new();

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

            var token = ObtenerToken()!;

            if (string.IsNullOrWhiteSpace(Input.IdParametro))
            {
                ModelState.AddModelError("Input.IdParametro", "El ID del parámetro es requerido");
            }
            else
            {
                var idTrimmed = Input.IdParametro.Trim().ToUpper();

                if (idTrimmed.Length > 10)
                {
                    ModelState.AddModelError("Input.IdParametro", "El ID del parámetro no puede exceder 10 caracteres");
                }
                else if (!Regex.IsMatch(idTrimmed, @"^[A-Z_]+$"))
                {
                    ModelState.AddModelError("Input.IdParametro", "El ID del parámetro solo puede contener letras mayúsculas y guiones bajos");
                }
            }

            if (string.IsNullOrWhiteSpace(Input.Valor))
            {
                ModelState.AddModelError("Input.Valor", "El valor del parámetro es requerido");
            }
            else if (Input.Valor.Trim().Length > 500)
            {
                ModelState.AddModelError("Input.Valor", "El valor del parámetro no puede exceder 500 caracteres");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var dto = new ParametroCrearDto
            {
                IdParametro = Input.IdParametro.Trim().ToUpper(),
                Valor = Input.Valor.Trim()
            };

            // Usar mensajes dinámicos de la API
            var (ok, status, message) = await _parametroService.CrearAsync(dto, token);

            if (ok)
            {
                TempData["Mensaje"] = message;  // Mensaje de la API
                return RedirectToPage("/Parametro/Parametros");
            }

            // Mostrar mensaje de error de la API
            ModelState.AddModelError(string.Empty, message ?? "Error al crear el parámetro");
            return Page();
        }
    }

    public class ParametroInputDto
    {
        [Required(ErrorMessage = "El ID del parámetro es requerido")]
        public string IdParametro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor es requerido")]
        public string Valor { get; set; } = string.Empty;
    }
}