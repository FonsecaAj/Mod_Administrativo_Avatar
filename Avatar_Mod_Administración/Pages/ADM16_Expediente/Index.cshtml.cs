using Avatar_Mod_Administración.Entities;
using Avatar_Mod_Administración.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avatar_Mod_Administración.Pages.ADM16_Expediente
{
    public class IndexModel : BasePageModel
    {
        private readonly IExpedienteApiClient _expedienteClient;
        private readonly IUbicacionesApiClient _ubicacionesClient;

        public IndexModel(
            IExpedienteApiClient expedienteClient,
            IUbicacionesApiClient ubicacionesClient,
            IAuthService auth,
            IUsuarioService usuarioService,
            ILogger<IndexModel> logger)
            : base(auth, usuarioService, logger)
        {
            _expedienteClient = expedienteClient;
            _ubicacionesClient = ubicacionesClient;
        }

        // ===== Formulario principal (crear / editar) =====

        [BindProperty]
        public ExpedienteRequestDto ExpedienteForm { get; set; } = new();

        [BindProperty]
        public bool EsEdicion { get; set; }

        // ===== Combos de dirección (interdependientes) =====

        public List<SelectListItem> TiposIdentificacion { get; set; } = new();
        public List<SelectListItem> Provincias { get; set; } = new();
        public List<SelectListItem> Cantones { get; set; } = new();
        public List<SelectListItem> Distritos { get; set; } = new();

        // ===== Listado de expedientes =====

        public List<ExpedienteDto> Expedientes { get; set; } = new();

        [TempData] public string? Mensaje { get; set; }
        [TempData] public string? MensajeError { get; set; }

        // =========================================================
        // GET: carga pantalla, combos y (opcional) un expediente en edición
        // =========================================================
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            // Tipos de identificación para el combo
            TiposIdentificacion = ObtenerTiposIdentificacion();

            // Siempre cargamos provincias al entrar
            await CargarCombosDireccionAsync(null, null);

            // Si viene un id, estamos editando
            if (!string.IsNullOrEmpty(id))
            {
                var exp = await _expedienteClient.ObtenerPorIdAsync(id);
                if (exp != null)
                {
                    ExpedienteForm = new ExpedienteRequestDto
                    {
                        Numero_Identificacion = exp.Numero_Identificacion,
                        Tipo_Identificacion = exp.Tipo_Identificacion,
                        Email = exp.Email,
                        Nombre_Completo = exp.Nombre_Completo,
                        Fecha_Nacimiento = exp.Fecha_Nacimiento,
                        ID_Provincia = exp.ID_Provincia,
                        ID_Canton = exp.ID_Canton,
                        DistritoID = exp.DistritoID,
                        Telefonos = exp.Telefonos
                    };

                    EsEdicion = true;

                    // Cargar cantones y distritos según el expediente
                    await CargarCombosDireccionAsync(
                        ExpedienteForm.ID_Provincia,
                        ExpedienteForm.ID_Canton
                    );
                }
            }

            // Listado de expedientes
            Expedientes = (await _expedienteClient.ObtenerTodosAsync()).ToList();

            return Page();
        }

        // =========================================================
        // POST: Guardar (crear o actualizar)
        // =========================================================
        public async Task<IActionResult> OnPostGuardarAsync()
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            TiposIdentificacion = ObtenerTiposIdentificacion();

            // Cargar combos respetando lo que el usuario seleccionó
            await CargarCombosDireccionAsync(
                ExpedienteForm.ID_Provincia,
                ExpedienteForm.ID_Canton);

            // ===== Validaciones básicas en el web (el backend también valida) =====
            if (string.IsNullOrWhiteSpace(ExpedienteForm.Numero_Identificacion))
                ModelState.AddModelError("ExpedienteForm.Numero_Identificacion", "La identificación es obligatoria.");

            if (string.IsNullOrWhiteSpace(ExpedienteForm.Tipo_Identificacion))
                ModelState.AddModelError("ExpedienteForm.Tipo_Identificacion", "Debe seleccionar un tipo de identificación.");

            if (string.IsNullOrWhiteSpace(ExpedienteForm.Email))
                ModelState.AddModelError("ExpedienteForm.Email", "El correo es obligatorio.");

            if (string.IsNullOrWhiteSpace(ExpedienteForm.Nombre_Completo))
                ModelState.AddModelError("ExpedienteForm.Nombre_Completo", "El nombre completo es obligatorio.");

            if (ExpedienteForm.ID_Provincia <= 0)
                ModelState.AddModelError("ExpedienteForm.ID_Provincia", "Debe seleccionar una provincia.");

            if (ExpedienteForm.ID_Canton <= 0)
                ModelState.AddModelError("ExpedienteForm.ID_Canton", "Debe seleccionar un cantón.");

            if (ExpedienteForm.DistritoID <= 0)
                ModelState.AddModelError("ExpedienteForm.DistritoID", "Debe seleccionar un distrito.");

            if (!ModelState.IsValid)
            {
                // Recargamos el listado para que la tabla no se vea vacía
                Expedientes = (await _expedienteClient.ObtenerTodosAsync()).ToList();
                return Page();
            }

            (bool ok, int status, string message) resultado;

            if (EsEdicion)
                resultado = await _expedienteClient.ActualizarAsync(ExpedienteForm);
            else
                resultado = await _expedienteClient.CrearAsync(ExpedienteForm);

            if (!resultado.ok)
            {
                MensajeError = resultado.message;
                Expedientes = (await _expedienteClient.ObtenerTodosAsync()).ToList();
                return Page();
            }

            Mensaje = resultado.message;
            return RedirectToPage(); // vuelve en modo crear limpio
        }

        // =========================================================
        // POST: Editar (cargar datos en el formulario desde la tabla)
        // =========================================================
        public async Task<IActionResult> OnPostEditarAsync(string id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            TiposIdentificacion = ObtenerTiposIdentificacion();

            var exp = await _expedienteClient.ObtenerPorIdAsync(id);
            if (exp == null)
            {
                MensajeError = "No se encontró el expediente seleccionado.";
                return RedirectToPage();
            }

            ExpedienteForm = new ExpedienteRequestDto
            {
                Numero_Identificacion = exp.Numero_Identificacion,
                Tipo_Identificacion = exp.Tipo_Identificacion,
                Email = exp.Email,
                Nombre_Completo = exp.Nombre_Completo,
                Fecha_Nacimiento = exp.Fecha_Nacimiento,
                ID_Provincia = exp.ID_Provincia,
                ID_Canton = exp.ID_Canton,
                DistritoID = exp.DistritoID,
                Telefonos = exp.Telefonos
            };

            EsEdicion = true;

            await CargarCombosDireccionAsync(
                ExpedienteForm.ID_Provincia,
                ExpedienteForm.ID_Canton);

            Expedientes = (await _expedienteClient.ObtenerTodosAsync()).ToList();

            return Page();
        }

        // =========================================================
        // POST: Eliminar
        // =========================================================
        public async Task<IActionResult> OnPostEliminarAsync(string id)
        {
            var redirect = await InicializarSesionAsync();
            if (redirect != null) return redirect;

            var (ok, status, message) = await _expedienteClient.EliminarAsync(id);

            if (!ok)
                MensajeError = message;
            else
                Mensaje = message;

            return RedirectToPage();
        }

        // =========================================================
        // Helpers
        // =========================================================

        private async Task CargarCombosDireccionAsync(int? idProvincia = null, int? idCanton = null)
        {
            // ===== PROVINCIAS =====
            var provincias = await _ubicacionesClient.ObtenerProvinciasAsync();
            Provincias = provincias
                .Select(p => new SelectListItem
                {
                    Value = p.ID_Provincia.ToString(),
                    Text = p.Nombre_Provincia
                })
                .ToList();

            // Inicializamos vacíos
            Cantones = new List<SelectListItem>();
            Distritos = new List<SelectListItem>();

            // ===== CANTONES =====
            if (idProvincia.HasValue && idProvincia.Value > 0)
            {
                var cantones = await _ubicacionesClient.ObtenerCantonesAsync(idProvincia.Value);
                Cantones = cantones
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Canton.ToString(),
                        Text = c.Nombre_Canton
                    })
                    .ToList();
            }

            // ===== DISTRITOS =====
            if (idProvincia.HasValue && idProvincia.Value > 0 &&
                idCanton.HasValue && idCanton.Value > 0)
            {
                var distritos = await _ubicacionesClient.ObtenerDistritosAsync(idProvincia.Value, idCanton.Value);
                Distritos = distritos
                    .Select(d => new SelectListItem
                    {
                        Value = d.ID_Distrito.ToString(),
                        Text = d.Nombre_Distrito
                    })
                    .ToList();
            }
        }

        private List<SelectListItem> ObtenerTiposIdentificacion()
        {
            return new List<SelectListItem>
            {
                new() { Value = "Nacional",  Text = "Nacional"  },
                new() { Value = "DIMEX",     Text = "DIMEX"     },
                new() { Value = "Pasaporte", Text = "Pasaporte" }
            };
        }
    }
}
