namespace Facturacion.API.Models.Dto
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];
        public int Total { get; set; }
    }
}
