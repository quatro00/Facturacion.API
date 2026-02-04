namespace Facturacion.API.Models.Dto.Cliente.Sucursal
{
    public class ToggleActivoRequest_Sucursal
    {
        public bool Activo { get; set; }
    }
    public sealed class ToggleActivoResponse_Sucursal
    {
        public Guid Id { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
