namespace Avatar_Mod_Administración.Entities
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class LookupsResponse
    {
        public IEnumerable<LookupItem> Carreras { get; set; } = new List<LookupItem>();
        public IEnumerable<LookupItem> Niveles { get; set; } = new List<LookupItem>();
    }

}