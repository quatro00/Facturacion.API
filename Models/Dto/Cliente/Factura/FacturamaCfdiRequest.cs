namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class FacturamaCfdiRequest
    {
        public string? Date { get; set; } // Facturama acepta string
        public string? Serie { get; set; }
        public string? Folio { get; set; }
        public string CfdiType { get; set; } = "I";

        public string Currency { get; set; } = "MXN";
        public string ExpeditionPlace { get; set; } = null!;
        public string Exportation { get; set; } = "01";

        public string? PaymentForm { get; set; }
        public string? PaymentMethod { get; set; }

        public FacturamaIssuer Issuer { get; set; } = null!;
        public FacturamaReceiver Receiver { get; set; } = null!;

        public List<FacturamaItem> Items { get; set; } = new();
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
        public string Unit { get; set; }
        public string IdentificationNumber { get; set; }
        public decimal Subtotal { get; set; }
        public string TaxObject { get; set; } = "02";
        public List<FacturamaTax> Taxes { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class FacturamaTax
    {
        public string Name { get; set; } = "IVA";
        public decimal Rate { get; set; } = 0.16m;
        public decimal Base { get; set; }
        public decimal Total { get; set; }
        public bool IsRetention { get; set; } = false;
    }
}
