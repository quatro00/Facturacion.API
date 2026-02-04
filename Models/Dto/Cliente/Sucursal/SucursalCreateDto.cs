namespace Facturacion.API.Models.Dto.Cliente.Sucursal
{
    public sealed class SucursalCreateDto
    {
        public string Codigo { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public bool Activo { get; set; } = true;

        public string? Telefono { get; set; }
        public string? Email { get; set; }

        public SucursalDireccionDto? Direccion { get; set; }

        public List<SucursalRazonCreateDto> RazonesSociales { get; set; } = new();

        public List<SucursalSerieUpsertDto> Series { get; set; } = new();
    }

    public sealed class SucursalDireccionDto
    {
        public string? CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? Colonia { get; set; }
        public string? Calle { get; set; }
        public string? NoInterior { get; set; }
        public string? NoExterior { get; set; }
    }

    public sealed class SucursalRazonCreateDto
    {
        public Guid RazonSocialId { get; set; }
        public bool Activo { get; set; } = true;
        public bool EsDefault { get; set; }
    }

    public sealed class SucursalSerieUpsertDto
    {
        public string Concepto { get; set; } = default!;
        public string TipoCfdi { get; set; } = default!;
        public string Serie { get; set; } = default!;
        public int? FolioInicial { get; set; }
        public bool Activo { get; set; }
    }
}
