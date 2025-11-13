namespace Avatar_Mod_Administración.Entities
{
    public class NotaDto
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

        public int ID_Curso { get; set; }
    }
}
