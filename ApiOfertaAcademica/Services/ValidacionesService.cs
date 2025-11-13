using System.Text.RegularExpressions;
using ApiACD3.Entities;

namespace ApiACD3.Services
{
    public static class ValidacionesService
    {
        public static (bool EsValido, string Mensaje) ValidarCurso(Curso curso)
        {
            if (curso == null)
                return (false, "El curso no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(curso.Identificador) ||
                string.IsNullOrWhiteSpace(curso.Nombre))
                return (false, "Todos los campos son requeridos y no pueden estar vacíos.");

            if (curso.Nivel < 1 || curso.Nivel > 12)
                return (false, "El nivel debe estar entre 1 y 12.");

            if (!Regex.IsMatch(curso.Nombre, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$"))
                return (false, "El nombre solo puede contener letras y espacios.");

            return (true, "OK");
        }
    }
}
