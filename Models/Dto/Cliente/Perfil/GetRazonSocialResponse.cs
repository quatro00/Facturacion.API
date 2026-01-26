namespace Facturacion.API.Models.Dto.Cliente.Perfil
{
    public class GetRazonSocialResponse
    {
        public Guid id { get; set; }
        public string rfc { get; set; }
        public string razonSocial { get; set; }
        public Guid regimenFiscalId { get; set; }
        public string celularNotificaciones { get; set; }
        public int folioInicio { get; set; }
        public string codigoPostal { get; set; }
        public string municipio { get; set; }
        public string estado { get; set; }
        public string colonia { get; set; }
        public string calle { get; set; }
        public string noInterior { get; set; }
        public string noExterior { get; set; }
    }
}
