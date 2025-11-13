using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avatar_Mod_Administración.Pages.ADM11_Profesor
{
    public class IndexModel : PageModel
    {
        private readonly IProfesorApiClient _api;

        public IndexModel(IProfesorApiClient api)
        {
            _api = api;
        }

        public List<ProfesorDto> Profesores { get; set; } = new();

    
        [BindProperty(SupportsGet = true)]
        public string? BuscarPor { get; set; } = "Todos"; 

        [BindProperty(SupportsGet = true)]
        public string? Texto { get; set; }

      
        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;
        public int TotalPaginas { get; set; }

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            var lista = (await _api.ObtenerTodosAsync()).ToList();

        
            if (!string.IsNullOrWhiteSpace(Texto))
            {
                var t = Texto.Trim().ToLower();

                lista = BuscarPor switch
                {
                    "Identificacion" => lista.Where(p =>
                        p.NumeroIdentificacion?
                            .Contains(Texto, StringComparison.OrdinalIgnoreCase) ?? false).ToList(),

                    "Nombre" => lista.Where(p =>
                        p.NombreCompleto?.ToLower().Contains(t) ?? false).ToList(),

                    "Email" => lista.Where(p =>
                        p.Email?.ToLower().Contains(t) ?? false).ToList(),

                    _ => lista.Where(p =>
                            (p.NumeroIdentificacion?
                                .Contains(Texto, StringComparison.OrdinalIgnoreCase) ?? false)
                            || (p.NombreCompleto?.ToLower().Contains(t) ?? false)
                            || (p.Email?.ToLower().Contains(t) ?? false)
                        ).ToList()
                };
            }

         
            lista = lista.OrderBy(p => p.NombreCompleto).ToList();

         
            var totalRegistros = lista.Count;
            TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)TamanoPagina);

            if (Pagina < 1) Pagina = 1;
            if (Pagina > TotalPaginas && TotalPaginas > 0) Pagina = TotalPaginas;

            Profesores = lista
                .Skip((Pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToList();
        }
    }
}
