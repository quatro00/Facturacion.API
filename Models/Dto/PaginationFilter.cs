namespace Facturacion.API.Models.Dto
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderBy { get; set; }
        public bool Descending { get; set; } = false;
    }
}
