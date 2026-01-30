namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class EmitirCfdiRequest
    {
        public Guid RazonSocialId { get; set; }
        public Guid ClienteId { get; set; }//si
        public string? Serie { get; set; }//si
        public string? Folio { get; set; }//si
        public DateTime Fecha { get; set; }
        public string? CfdiUse { get; set; }       // UsoCFDI //si
        public string ExpeditionPlace { get; set; } = null!; // CP de expedición (obligatorio en CFDI 4.0)
        public string CfdiType { get; set; } = "I";           // I/E/P/T
        public string Currency { get; set; } = "MXN";//si
        public string Exportation { get; set; } = "01";//si
        // Puedes permitir override, pero normalmente viene de ClienteConfiguracion
        public string? PaymentForm { get; set; }   // FormaPago //si
        public string? PaymentMethod { get; set; } // MetodoPago //si
        public List<EmitirCfdiItem> Items { get; set; } = new();
    }

    public class EmitirCfdiItem
    {
        public string ProductCode { get; set; } = null!; // ClaveProdServ //si
        public string UnitCode { get; set; } = null!;    // ClaveUnidad //si
        public string Description { get; set; } = null!; //si
        public decimal Quantity { get; set; } //si
        public decimal UnitPrice { get; set; } //si
        public string TaxObject { get; set; } = "02"; // "01".."08"
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
