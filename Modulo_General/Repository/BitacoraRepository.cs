using Avatar_Mod_Administración.Entities;
using Dapper;
using System.Data;

namespace Avatar_Mod_Administración.Repository
{
    public class BitacoraRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public BitacoraRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<int> Registrar(Bitacora bitacora)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                var sql = @"INSERT INTO Bitacora (Fecha_Registro, Usuario, Descripcion, Tipo_Accion)
                            VALUES (GETDATE(), @Usuario, @Descripcion, @Tipo_Accion);
                            SELECT SCOPE_IDENTITY();";

                return await connection.ExecuteScalarAsync<int>(sql, new
                {
                    bitacora.Usuario,
                    bitacora.Descripcion,
                    bitacora.Tipo_Accion
                });
            }
        }

        public async Task<(IEnumerable<Bitacora> Data, int Total)> Consultar(BitacoraFiltroRequest filtro)
        {
            using IDbConnection connection = _dbConnectionFactory.CreateConnection();

            var parametros = new
            {
                FechaDesde = filtro.FechaDesde,
                FechaHasta = filtro.FechaHasta,
                Usuario = filtro.Usuario,
                Accion = filtro.Tipo_Accion,
                IdModulo = filtro.IdModulo,
                NombreModulo = filtro.NombreModulo,
                Pagina = filtro.Pagina,
                PorPagina = filtro.PorPagina,
                OrdenColumna = filtro.OrdenColumna,
                OrdenDireccion = filtro.OrdenDireccion
            };

            CommandType? commandType = CommandType.StoredProcedure;
            IEnumerable<dynamic> resultado = await connection.QueryAsync<object>("sp_ConsultarBitacora", parametros, null, null, commandType);

            List<Bitacora> lista = new List<Bitacora>();
            int total = 0;

            foreach (dynamic row in resultado)
            {
                lista.Add(new Bitacora
                {
                    ID_Bitacora = row.ID_Bitacora,
                    Fecha_Registro = row.Fecha_Registro,
                    Usuario = row.Usuario,
                    Descripcion = row.Descripcion,
                    Tipo_Accion = row.Tipo_Accion
                });
                total = row.Total;
            }

            return (Data: lista, Total: total);
        }

        public async Task<List<Bitacora>> ObtenerTodas(string? usuario = null)
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                string sql;
                object? parametros = null;

                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    sql = @"SELECT ID_Bitacora, Fecha_Registro, Usuario, Descripcion, Tipo_Accion 
                            FROM Bitacora 
                            WHERE Usuario = @Usuario
                            ORDER BY Fecha_Registro DESC";
                    parametros = new { Usuario = usuario };
                }
                else
                {
                    sql = @"SELECT ID_Bitacora, Fecha_Registro, Usuario, Descripcion, Tipo_Accion 
                            FROM Bitacora 
                            ORDER BY Fecha_Registro DESC";
                }

                var result = await connection.QueryAsync<Bitacora>(sql, parametros);
                return result.ToList();
            }
        }
    }
}