namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public class NotaCreditoParcialMontoRequest
    {
        public Guid CfdiOrigenId { get; set; }
        public decimal Monto { get; set; } // TOTAL (con impuestos) a acreditar
    }
}
