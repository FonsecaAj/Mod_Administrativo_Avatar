namespace Avatar_Mod_Administración.Entities
{
    public class BusinessLogicResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? ResponseObject { get; set; }
    }
}
