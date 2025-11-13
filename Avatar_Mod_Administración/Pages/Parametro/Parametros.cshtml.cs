using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Services;
using Avatar_Mod_Administración.Entities;

namespace Avatar_Mod_Administración.Pages.Parametro
{
    public class ParametrosModel : BasePageModel
    {
        private readonly IParametroService _parametroService;

        public ParametrosModel(
            IParametroService parametroService,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ParametrosModel> logger)
            : base(authService, usuarioService, logger)
        {
            _parametroService = parametroService;
        }

        public List<ParametroApi> Parametros { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }
        public int TotalParametros { get; set; }
        private const int ParametrosPorPagina = 5;

        public async Task<IActionResult> OnGetAsync(int pagina = 1)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;

            PaginaActual = pagina < 1 ? 1 : pagina;

            TotalParametros = await _parametroService.ObtenerTotalAsync(token);
            TotalPaginas = (int)Math.Ceiling(TotalParametros / (double)ParametrosPorPagina);

            if (PaginaActual > TotalPaginas && TotalPaginas > 0)
                PaginaActual = TotalPaginas;

            Parametros = await _parametroService.ObtenerTodosAsync(token, PaginaActual, ParametrosPorPagina);

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(string id)
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken()!;
            var resultado = await _parametroService.EliminarAsync(id, token);

            if (resultado)
                TempData["Mensaje"] = "Parámetro eliminado exitosamente";
            else
                TempData["Error"] = "No se pudo eliminar el parámetro";

            return RedirectToPage();
        }
    }
}