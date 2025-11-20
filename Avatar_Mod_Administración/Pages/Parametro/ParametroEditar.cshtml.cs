using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Avatar_Mod_Administración.Pages.Parametro
{
    public class ParametroEditarModel : BasePageModel
    {
        private readonly IParametroService _parametroService;

        public ParametroEditarModel(
            IParametroService parametroService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ParametroEditarModel> logger)
            : base(authService, usuarioService, logger)
        {
            _parametroService = parametroService;
        }

        [BindProperty]
        public ParametroEditarDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            var parametro = await _parametroService.ObtenerPorIdAsync(Id, token);
            if (parametro == null)
                return NotFound();

            Input = new ParametroEditarDto
            {
                IdParametro = parametro.IdParametro,
                Valor = parametro.Valor
            };

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

            var resultado = await _parametroService.ActualizarAsync(Id, dto, token);

            if (resultado)
            {
                TempData["Mensaje"] = "Parámetro actualizado exitosamente";
                return RedirectToPage("/Parametro/Parametros");
            }

            ModelState.AddModelError(string.Empty, "Error al actualizar el parámetro");
            return Page();
        }
    }

    public class ParametroEditarDto
    {
        [Required(ErrorMessage = "El ID del parámetro es requerido")]
        public string IdParametro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor es requerido")]
        public string Valor { get; set; } = string.Empty;
    }
}