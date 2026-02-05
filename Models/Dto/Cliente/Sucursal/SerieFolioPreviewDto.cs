namespace Facturacion.API.Models.Dto.Cliente.Sucursal
{
    public class SerieFolioPreviewDto
    {
        public string Serie { get; set; } = null!;
        public int Folio { get; set; }
        public string? ExpeditionPlace { get; set; } // CP (opcional)
    }
}
