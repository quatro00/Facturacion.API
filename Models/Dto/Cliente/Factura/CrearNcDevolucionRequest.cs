namespace Facturacion.API.Models.Dto.Cliente.Factura
{
    public sealed class CrearNcDevolucionRequest
    {
        public Guid CfdiId { get; set; }
        public List<CrearNcDevolucionConceptoRequest> Conceptos { get; set; } = new();
    }

    public sealed class CrearNcDevolucionConceptoRequest
    {
        public Guid CfdiConceptoId { get; set; }
        public decimal Cantidad { get; set; }
    }
}
