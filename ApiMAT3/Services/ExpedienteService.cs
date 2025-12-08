using ApiMAT3.Entities;
using ApiMAT3.Repository;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace ApiMAT3.Services
{
    public class ExpedienteService : IExpedienteService
    {
        private readonly IExpedienteRepository _repo;
        private readonly BitacoraConsumer _bitacoraConsumer;
        private readonly IAutenticacionService _auth;
        private readonly IHttpContextAccessor _http;
        private readonly string _dominioPermitido;

        public ExpedienteService(
            IExpedienteRepository repo,
            IConfiguration configuration,
            BitacoraConsumer bitacoraConsumer,
            IAutenticacionService auth,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _bitacoraConsumer = bitacoraConsumer;
            _auth = auth;
            _http = httpContextAccessor;
            _dominioPermitido = configuration.GetValue<string>("DominioPermitido") ?? "cuc.cr";
        }

        //  Helper para obtener usuario desde el token
        private async Task<string> ObtenerUsuarioActualAsync()
        {
            var authorization = _http.HttpContext?.Request?.Headers["Authorization"].ToString();
            var usuario = await _auth.ObtenerUsuarioDelTokenAsync(authorization);
            return string.IsNullOrWhiteSpace(usuario) ? "Anónimo" : usuario;
        }

      
        private async Task RegistrarBitacoraAsync(string tipoAccion, object detalle)
        {
            try
            {
                var usuario = await ObtenerUsuarioActualAsync();
                await _bitacoraConsumer.RegistrarAccionAsync(usuario, tipoAccion, detalle);
            }
            catch
            {
          
            }
        }

       

        public IEnumerable<Expediente> ObtenerTodos()
        {
            var expedientes = _repo.ObtenerTodos();

            _ = RegistrarBitacoraAsync("CONSULTA", new
            {
                accion = "SELECT",
                resultado = "El usuario consulta la lista de expedientes"
            });

            return expedientes;
        }

        public Expediente ObtenerPorId(string numeroIdentificacion)
        {
            var expediente = _repo.ObtenerPorId(numeroIdentificacion);

            _ = RegistrarBitacoraAsync("CONSULTA", new
            {
                accion = "SELECT",
                resultado = $"El usuario consulta el expediente con identificación {numeroIdentificacion}"
            });

            return expediente;
        }

        public void Crear(Expediente expediente)
        {
            Validar(expediente);
            _repo.Crear(expediente);

            _ = RegistrarBitacoraAsync("CREAR", new
            {
                accion = "INSERT",
                resultado = expediente
            });
        }

        public void Actualizar(Expediente expediente)
        {
            Validar(expediente);
            var anterior = _repo.ObtenerPorId(expediente.Numero_Identificacion);
            _repo.Actualizar(expediente);

            _ = RegistrarBitacoraAsync("ACTUALIZAR", new
            {
                accion = "UPDATE",
                resultado = new
                {
                    anterior,
                    nuevo = expediente
                }
            });
        }

        public void Eliminar(string numeroIdentificacion)
        {
            if (string.IsNullOrWhiteSpace(numeroIdentificacion))
                throw new ArgumentException("El número de identificación es requerido.");

            var eliminado = _repo.ObtenerPorId(numeroIdentificacion);
            _repo.Eliminar(numeroIdentificacion);

            _ = RegistrarBitacoraAsync("ELIMINAR", new
            {
                accion = "DELETE",
                resultado = eliminado
            });
        }

       
        private void Validar(Expediente expediente)
        {
            if (string.IsNullOrWhiteSpace(expediente.Numero_Identificacion) ||
                string.IsNullOrWhiteSpace(expediente.Tipo_Identificacion) ||
                string.IsNullOrWhiteSpace(expediente.Email) ||
                string.IsNullOrWhiteSpace(expediente.Nombre_Completo) ||
                expediente.Fecha_Nacimiento == default ||
                expediente.ID_Provincia <= 0 ||
                expediente.ID_Canton <= 0 ||
                expediente.DistritoID <= 0 ||
                string.IsNullOrWhiteSpace(expediente.Telefonos))
                throw new ArgumentException("Todos los campos son requeridos.");

            if (!Regex.IsMatch(expediente.Nombre_Completo, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$"))
                throw new ArgumentException("El nombre solo puede contener letras y espacios.");

            if (!Regex.IsMatch(expediente.Email, @$"^[^@\s]+@{_dominioPermitido}$"))
                throw new ArgumentException($"El correo debe pertenecer al dominio {_dominioPermitido}.");
        }
    }
}
