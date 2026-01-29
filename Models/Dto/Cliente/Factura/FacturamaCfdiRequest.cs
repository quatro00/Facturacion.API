namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class FacturamaCfdiRequest
    {
        // Generales
        public string? Date { get; set; }              // ISO string
        public string? Serie { get; set; }
        public string? Folio { get; set; }

        /// <summary>
        /// I = Ingreso, E = Egreso (Nota de crédito)
        /// </summary>
        public string CfdiType { get; set; } = "I";

        /// <summary>
        /// Requerido por Facturama cuando es Nota de Crédito
        /// "2" = Nota de crédito
        /// </summary>
        public string? NameId { get; set; }

        public string Currency { get; set; } = "MXN";
        public string ExpeditionPlace { get; set; } = null!;
        public string Exportation { get; set; } = "01";

        public string? PaymentForm { get; set; }
        public string? PaymentMethod { get; set; }

        // Emisor / Receptor
        public FacturamaIssuer Issuer { get; set; } = null!;
        public FacturamaReceiver Receiver { get; set; } = null!;

        // Relación SAT (solo para Egreso)
        public FacturamaRelations? Relations { get; set; }

        // Conceptos
        public List<FacturamaItem> Items { get; set; } = new();
    }

    public class FacturamaRelations
    {
        /// <summary>
        /// 01 = Nota de crédito de los documentos relacionados
        /// </summary>
        public string Type { get; set; } = "01";

        public List<FacturamaRelatedCfdi> Cfdis { get; set; } = new();
    }

    public class FacturamaRelatedCfdi
    {
        public string Uuid { get; set; } = null!;
    }

    public class FacturamaIssuer
    {
        public string Rfc { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string FiscalRegime { get; set; } = null!;
    }

    public class FacturamaReceiver
    {
        public string Rfc { get; set; } = null!;
        public string Name { get; set; } = null!;

        /// <summary>
        /// G02 para Nota de Crédito
        /// </summary>
        public string CfdiUse { get; set; } = null!;

        public string FiscalRegime { get; set; } = null!;
        public string TaxZipCode { get; set; } = null!;
    }
    public class FacturamaItem
    {
        public string ProductCode { get; set; } = null!;
        public string UnitCode { get; set; } = null!;
        public string Description { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public string? Unit { get; set; }
        public string? IdentificationNumber { get; set; }

        public decimal Subtotal { get; set; }

        /// <summary>
        /// 02 = Sí objeto de impuesto
        /// </summary>
        public string TaxObject { get; set; } = "02";

        public List<FacturamaTax> Taxes { get; set; } = new();

        public decimal Total { get; set; }
    }

    public class FacturamaTax
    {
        public string Name { get; set; } = "IVA";

        /// <summary>
        /// 0.16 = IVA 16%
        /// </summary>
        public decimal Rate { get; set; } = 0.16m;

        public decimal Base { get; set; }
        public decimal Total { get; set; }

        public bool IsRetention { get; set; } = false;
    }
}
