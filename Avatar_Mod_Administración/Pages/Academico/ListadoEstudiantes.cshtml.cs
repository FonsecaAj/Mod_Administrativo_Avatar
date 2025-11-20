using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_Mod_Administración.Pages.Academico
{
    public class ListadoEstudiantesModel : BasePageModel
    {
        private readonly IListadoEstudiantesApiClient _api;

        public ListadoEstudiantesModel(
            IListadoEstudiantesApiClient api,
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<ListadoEstudiantesModel> logger)
            : base(authService, usuarioService, logger)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public int Periodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Carrera { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Curso { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Grupo { get; set; }

        public List<EstudiantesListadoDto>? Estudiantes { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await InicializarSesionAsync();
            if (result != null) return result;

            var token = ObtenerToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                ErrorMessage = "Token no disponible, vuelva a iniciar sesión.";
                return Page();
            }

            if (Periodo == 0)
            {
                Message = "Ingrese el número de período para obtener el listado.";
                return Page();
            }

            var (ok, status, msg, data) = await _api.ObtenerListadoAsync(Periodo, token);

            if (!ok)
            {
                ErrorMessage = msg ?? $"Error {status}: No se pudo obtener el listado.";
                return Page();
            }

            Estudiantes = data;
            Message = msg;
            return Page();
        }
    }
}
