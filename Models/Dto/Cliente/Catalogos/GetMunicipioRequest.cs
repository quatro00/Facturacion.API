namespace Facturacion.API.Models.Dto.Cliente.Catalogos
{
    public class GetMunicipioRequest
    {
        public string codigoPostal { get; set; }
    }

    public class GetMunicipioResponse
    {
        public string municipio { get; set; }
        public string estado { get; set; }
        public List<string> colonia { get; set; }
    }
}
