namespace Facturacion.API.Models.Dto.Cliente.Cliente
{
    public class ClienteListadoDto
    {
        public Guid ClienteId { get; set; }
        public string Rfc { get; set; }
        public string RazonSocial { get; set; }
        public string UsoCfdi { get; set; }
        public string Moneda { get; set; }
    }
}
