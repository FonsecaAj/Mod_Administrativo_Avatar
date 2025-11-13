using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.Rubros
{
    public class Asignar_Nota_RubrosModel : PageModel
    {

        private readonly IRubroApiClient _apiRubros;
        private readonly INotaApiClient _apiNotas;

        public Asignar_Nota_RubrosModel(IRubroApiClient apiRubros, INotaApiClient apiNotas)
        {
            _apiRubros = apiRubros;
            _apiNotas = apiNotas;
        }

        [BindProperty(SupportsGet = true)]
        public int ID_Grupo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ID_Estudiante { get; set; }

        public List<RubroDto>? Rubros { get; set; }
        public List<NotaDto>? Notas { get; set; }

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        // ======= Obtener rubros y notas =======
        public async Task OnGetAsync()
        {
            if (ID_Grupo == 0)
            {
                Message = "Ingrese el ID del grupo y del estudiante para consultar.";
                return;
            }

            // Obtener rubros
            var (okRubros, statusR, msgR, dataR) = await _apiRubros.ObtenerDesglose(ID_Grupo);
            if (!okRubros)
            {
                if (statusR == 404)
                    Message = "El grupo no tiene desglose registrado.";
                else
                    ErrorMessage = msgR ?? $"Error {statusR}: No se pudieron obtener los rubros.";
                return;
            }

            Rubros = dataR ?? new();

            // Obtener notas del estudiante (solo si se ingresó)
            if (ID_Estudiante != 0)
            {
                var (okNotas, statusN, msgN, dataN) = await _apiNotas.ObtenerNotas(ID_Estudiante, ID_Grupo);
                if (!okNotas)
                {
                    if (statusN == 404)
                        Message = "El estudiante no tiene notas registradas en este grupo.";
                    else
                        ErrorMessage = msgN ?? $"Error {statusN}: No se pudieron obtener las notas.";
                    return;
                }

                Notas = dataN ?? new();
                Message = msgN;
            }
        }

        // ======= Asignar nota a un rubro =======
        public async Task<IActionResult> OnPostAsignarNotaAsync(int idRubro, decimal valorNota)
        {
            if (ID_Grupo == 0 || ID_Estudiante == 0)
            {
                ErrorMessage = "Debe especificar el grupo y el estudiante.";
                return Page();
            }

            if (valorNota < 0 || valorNota > 100)
            {
                ErrorMessage = "La nota debe estar entre 0 y 100.";
                return Page();
            }

            var request = new NotaRequest
            {
                ID_Estudiante = ID_Estudiante,
                ID_Rubro = idRubro,
                Valor_Nota = valorNota,
                ID_Curso = ID_Grupo // este es tu idCurso (grupo)
            };

            var (ok, status, msg) = await _apiNotas.AsignarNota(request);

            if (!ok)
            {
                ErrorMessage = msg ?? $"Error {status}: No se pudo asignar la nota.";
                return Page();
            }

            Message = msg ?? "Nota asignada correctamente.";
            return RedirectToPage(new { ID_Grupo, ID_Estudiante });
        }
    }



}
       


