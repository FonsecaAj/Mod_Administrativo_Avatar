using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Rubros
{
    public class DesgloseModel : PageModel
    {
        private readonly IRubroApiClient _apiRubros;
        private readonly INotaApiClient _apiNota;

        public DesgloseModel(IRubroApiClient api, INotaApiClient apiNota)
        {
            _apiRubros = api;
            _apiNota = apiNota;
        }

        [BindProperty(SupportsGet = true)]
        public int ID_Grupo { get; set; }

        [BindProperty]
        public DesgloseRequest Desglose { get; set; } = new();

        public List<RubroDto>? Rubros { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public bool SinDesglose => Rubros == null || Rubros.Count == 0;


        //Sección de notas
        //[BindProperty(SupportsGet = true)]
        //public int ID_Estudiante { get; set; }

        //public List<NotaDto>? Notas { get; set; }

        public async Task OnGetAsync()
        {
            if (ID_Grupo == 0)
            {
                Message = "Ingrese el ID del grupo para consultar su desglose.";
                return;
            }

            var (ok, status, msg, data) = await _apiRubros.ObtenerDesglose(ID_Grupo);

            // Si el microservicio devolvió un 404 not found (no existe el grupo), lo tratamos como caso normal
            if (status == 404)
            {
                Rubros = new List<RubroDto>(); // Lista vacía para activar el estado “sin desglose”
                Message = "El grupo seleccionado no existe"; // Sin mensaje extra
                return;
            }

            // Si hubo cualquier otro error, sí mostramos el mensajito al usuario
            if (!ok)
            {
                ErrorMessage = msg ?? $"Error {status}: No se pudo obtener el desglose.";
                return;
            }

            Rubros = data;
            Message = msg;
        }

        public async Task<IActionResult> OnPostCargarAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            foreach (var r in Desglose.Rubros)
                r.ID_Grupo = Desglose.ID_Grupo;

            var (ok, statusCode, message) = await _apiRubros.CargarDesglose(Desglose);

            if (!ok)
            {
                ErrorMessage = message ?? $"Error {statusCode}: No se pudo cargar el desglose.";
                return Page();
            }

            Message = message;
            return RedirectToPage(new { ID_Grupo = Desglose.ID_Grupo });
        }


        //Sección de notas

        // Obtener notas por grupo y estudiante
        //public async Task<IActionResult> OnPostObtenerNotasAsync()
        //{
        //    if (ID_Grupo == 0 || ID_Estudiante == 0)
        //    {
        //        ErrorMessage = "Debe ingresar el ID del grupo y del estudiante.";
        //        return Page();
        //    }

        //    var (ok, status, msg, data) = await _apiNota.ObtenerNotas(ID_Estudiante, ID_Grupo);

        //    if (!ok)
        //    {
        //        ErrorMessage = msg ?? $"Error {status}: No se pudieron obtener las notas.";
        //        return Page();
        //    }

        //    Notas = data;
        //    Message = msg;
        //    return Page();
        //}

        //// Guardar nota de un rubro específico
        //public async Task<IActionResult> OnPostAsignarNotaAsync(int idRubro, int nota)
        //{
        //    if (nota < 1 || nota > 100)
        //    {
        //        ErrorMessage = "La nota debe estar entre 1 y 100.";
        //        return Page();
        //    }

        //    var request = new NotaRequest
        //    {
        //        ID_Estudiante = ID_Estudiante,
        //        ID_Curso = ID_Grupo, // asumiendo que el grupo equivale al curso
        //        ID_Rubro = idRubro,
        //        Valor_Nota = nota
        //    };

        //    var (ok, status, msg) = await _apiNota.AsignarNota(request);

        //    if (!ok)
        //    {
        //        ErrorMessage = msg ?? $"Error {status}: No se pudo asignar la nota.";
        //        return Page();
        //    }

        //    Message = msg ?? "Nota asignada correctamente.";
        //    return RedirectToPage(new { ID_Grupo, ID_Estudiante });
        //}




    }
}
