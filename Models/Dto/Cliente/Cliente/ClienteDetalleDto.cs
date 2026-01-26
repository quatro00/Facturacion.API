namespace Facturacion.API.Models.Dto.Cliente.Cliente
{
    public class ClienteDetalleDto
    {
        public Guid id { get; set; }
        public string rfc { get; set; }
        public string razonSocial { get; set; }
        public Guid regimenFiscalId { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string pais {  get; set; }
        public string codigoPostal { get; set; }
        public string colonia { get; set; }
        public string estado { get; set; }
        public string calle {  get; set; }
        public string noInterior { get; set; }
        public string noExterior { get; set; }
        public string metodoPago { get; set; }
        public string formaPago { get; set; }
        public string moneda { get; set; }
        public string exportacion { get; set; }
        public string usoCfdi { get; set; }
    }
}
