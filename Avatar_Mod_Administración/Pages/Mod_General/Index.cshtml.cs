using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Avatar_Mod_Administración.Pages;
using System.Text.Json;

namespace Avatar_Mod_Administración.Pages.Mod_General
{
    public class IndexModel : BasePageModel
    {
        private readonly IBitacoraService _bitacoraService;

        public List<BitacoraListadoVm> Registros { get; set; } = new();
        public BitacoraFiltroDto Filtro { get; set; } = new();

        public IndexModel(
            IAuthService authService,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger,
            IBitacoraService bitacoraService
        ) : base(authService, usuarioService, logger)
        {
            _bitacoraService = bitacoraService;
        }

        public async Task<IActionResult> OnGet(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string? usuario,
            string? accion,
            string? modulo)
        {
            var redir = await InicializarSesionAsync();
            if (redir != null) return redir;

            Filtro = new BitacoraFiltroDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Usuario = usuario,
                Accion = accion,
                Modulo = modulo
            };

            var result = await _bitacoraService.ConsultarAsync(Filtro);

            Registros = result.Data.Select(x => new BitacoraListadoVm
            {
                IdBitacora = x.iD_Bitacora,
                FechaBitacora = x.fecha_Registro,
                Usuario = x.usuario,
                Accion = x.tipo_Accion,
                JsonLimpio = ParseJsonSeguro(x.detalle?.ToString())
            }).ToList();

            return Page();
        }

        private object ParseJsonSeguro(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new { info = "Sin detalle" };

            try
            {
                return JsonSerializer.Deserialize<object>(raw);
            }
            catch
            {
                return new { error = "JSON inválido", raw };
            }
        }
    }

    public class BitacoraListadoVm
    {
        public int IdBitacora { get; set; }
        public DateTime FechaBitacora { get; set; }
        public string Usuario { get; set; } = "";
        public string Accion { get; set; } = "";
        public object JsonLimpio { get; set; } = new();
    }
}
