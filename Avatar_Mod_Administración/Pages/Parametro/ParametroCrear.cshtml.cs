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

            if (string.IsNullOrWhiteSpace(Input.IdParametro))
            {
                ModelState.AddModelError("Input.IdParametro", "El ID del par�metro es requerido");
            }
            else
            {
                var idTrimmed = Input.IdParametro.Trim().ToUpper();

                if (idTrimmed.Length > 10)
                {
                    ModelState.AddModelError("Input.IdParametro", "El ID del par�metro no puede exceder 10 caracteres");
                }
                else if (!Regex.IsMatch(idTrimmed, @"^[A-Z_]+$"))
                {
                    ModelState.AddModelError("Input.IdParametro", "El ID del par�metro solo puede contener letras may�sculas y guiones bajos");
                }
            }

            if (string.IsNullOrWhiteSpace(Input.Valor))
            {
                ModelState.AddModelError("Input.Valor", "El valor del par�metro es requerido");
            }
            else if (Input.Valor.Trim().Length > 500)
            {
                ModelState.AddModelError("Input.Valor", "El valor del par�metro no puede exceder 500 caracteres");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var token = ObtenerToken()!;
            var dto = new ParametroCrearDto
            {
                IdParametro = Input.IdParametro.Trim().ToUpper(),
                Valor = Input.Valor.Trim()
            };

            var (ok, status, message) = await _parametroService.CrearAsync(dto, token);

            if (ok)
            {
                TempData["Mensaje"] = message;
                return RedirectToPage("/Parametro/Parametros");
            }

            ModelState.AddModelError(string.Empty, message ?? "Error al crear el par�metro");
            return Page();
        }
    }

    public class ParametroInputDto
    {
        [Required(ErrorMessage = "El ID del par�metro es requerido")]
        public string IdParametro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor es requerido")]
        public string Valor { get; set; } = string.Empty;
    }
}