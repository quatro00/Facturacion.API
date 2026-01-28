namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class GetFacturasQuery
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string? Status { get; set; }      // Activo, Cancelado
        public string? Type { get; set; }        // I, E, P, T
        public string? Currency { get; set; }    // MXN, USD, etc
        public string? Search { get; set; }      // UUID, RFC, Serie/Folio

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
