namespace Adm_Notas.Entities
{
    public class Nota
    {

        public int ID_Nota { get; set; }
        public int ID_Estudiante { get; set; }
        public int ID_Rubro { get; set; }
        public decimal Valor_Nota { get; set; }

    }

    public class NotaRequest
    {
        public int ID_Estudiante { get; set; }
        public int ID_Rubro { get; set; }
        public decimal Valor_Nota { get; set; }
    }
}
