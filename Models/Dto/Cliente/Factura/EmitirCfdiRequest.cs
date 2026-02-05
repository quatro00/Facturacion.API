namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class EmitirCfdiRequest
    {
        public Guid RazonSocialId { get; set; }
        public Guid ClienteId { get; set; }

        // ✅ NUEVO: de qué sucursal se expide (y de ahí tomamos CP + series)
        public Guid SucursalId { get; set; }

        // ✅ NUEVO: Concepto de serie (coincide con SucursalSerie.Concepto)
        // Valores permitidos: I_MERCANCIAS | I_SERVICIOS | I_ANTICIPO
        public string TipoFactura { get; set; } = "I_MERCANCIAS";

        // Nota: Serie/Folio ya NO deberían venir del front, el sistema los calcula;
        // los dejamos opcionales por compatibilidad/depuración.
        public string? Serie { get; set; }
        public string? Folio { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string? CfdiUse { get; set; }                     // UsoCFDI
        public string? ExpeditionPlace { get; set; }             // CP expedición (se setea desde sucursal)
        public string CfdiType { get; set; } = "I";              // siempre "I"
        public string Currency { get; set; } = "MXN";
        public string Exportation { get; set; } = "01";

        public string? PaymentForm { get; set; }                 // FormaPago
        public string? PaymentMethod { get; set; }               // MetodoPago

        public List<EmitirCfdiItem> Items { get; set; } = new();
    }

    public class EmitirCfdiItem
    {
        public string ProductCode { get; set; } = null!;
        public string UnitCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string TaxObject { get; set; } = "02";
        public List<EmitirCfdiTax>? Taxes { get; set; } = new();
    }

    public class EmitirCfdiTax
    {
        // Ej: "IVA", "ISR", "IEPS", "IVA RET", etc. (según tu modelo Facturama)
        public string Name { get; set; } = "IVA";
        public decimal Rate { get; set; }      // 0.16, 0, etc
        public decimal Base { get; set; }      // base gravable
        public decimal Total { get; set; }     // importe del impuesto
        public bool IsRetention { get; set; } = false;
    }
}
