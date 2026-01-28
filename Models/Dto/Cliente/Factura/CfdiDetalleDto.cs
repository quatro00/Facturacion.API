namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public sealed class CfdiConceptoDto
    {
        public Guid Id { get; set; }
        public string? ProductCode { get; set; }
        public string? UnitCode { get; set; }
        public decimal Cantidad { get; set; }
        public string? Unidad { get; set; }
        public string Descripcion { get; set; } = default!;
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Importe { get; set; }
    }

    public sealed class CfdiHistorialDto
    {
        public Guid Id { get; set; }
        public string Estatus { get; set; } = default!;
        public string? Motivo { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class CfdiDetalleDto
    {
        public Guid Id { get; set; }
        public string FacturamaId { get; set; } = default!;
        public Guid Uuid { get; set; }

        public string? Serie { get; set; }
        public string? Folio { get; set; }

        public string TipoCfdi { get; set; } = default!;
        public string Moneda { get; set; } = default!;
        public DateTime FechaTimbrado { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }

        public string? FormaPago { get; set; }
        public string? MetodoPago { get; set; }
        public string LugarExpedicion { get; set; } = default!;

        public string RfcEmisor { get; set; } = default!;
        public string? RazonSocialEmisor { get; set; }

        public string RfcReceptor { get; set; } = default!;
        public string? RazonSocialReceptor { get; set; }

        public string Estatus { get; set; } = default!;
        public string? MotivoCancelacion { get; set; }
        public Guid? UuidSustitucion { get; set; }

        public List<CfdiConceptoDto> Conceptos { get; set; } = new();
        public List<CfdiHistorialDto> Historial { get; set; } = new();
    }
}
