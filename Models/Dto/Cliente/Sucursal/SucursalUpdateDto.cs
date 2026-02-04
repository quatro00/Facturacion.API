namespace Facturacion.API.Models.Dto.Cliente.Sucursal
{
    public sealed class SucursalUpdateDto
    {
        public string Nombre { get; set; } = default!;
        public bool Activo { get; set; }

        public string? Telefono { get; set; }
        public string? Email { get; set; }

        public string? CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? Colonia { get; set; }
        public string? Calle { get; set; }
        public string? NoInterior { get; set; }
        public string? NoExterior { get; set; }

        public string RowVersion { get; set; } = default!;

        public List<SucursalRazonUpdateDto> RazonesSociales { get; set; } = new();
        public List<SucursalSerieUpsertDto> Series { get; set; } = new();
    }

    public sealed class SucursalRazonUpdateDto
    {
        public Guid RazonSocialId { get; set; }
        public bool Activo { get; set; } = true;
        public bool EsDefault { get; set; }
    }


}
