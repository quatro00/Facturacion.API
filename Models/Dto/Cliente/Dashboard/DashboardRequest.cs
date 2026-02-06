namespace Facturacion.API.Models.Dto.Cliente.Dashboard
{
    public sealed class DashboardRequest
    {
        // Periodo
        public DateTime? Desde { get; set; }   // si viene null => default últimos 30 días
        public DateTime? Hasta { get; set; }

        // Filtros (opcionales)
        public Guid? SucursalId { get; set; }
        public Guid? RazonSocialId { get; set; }

        // Moneda (opcional): "MXN", "USD", "ALL"
        public string? Moneda { get; set; }

        // Cantidades (opcional)
        public int TakeRecientes { get; set; } = 10;
        public int TakeTopClientes { get; set; } = 5;

        // Para barras: meses hacia atrás
        public int MesesHistorico { get; set; } = 12;
    }
}
