namespace Facturacion.API.Models.Dto.Cliente.Sucursal
{
    public sealed class SucursalDetalleDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = default!;
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

        // Para concurrencia (RowVersion timestamp -> base64)
        public string RowVersion { get; set; } = default!;

        public List<SucursalDetalleRazonDto> RazonesSociales { get; set; } = new();
        public List<SucursalSerieDto> Series { get; set; } = new();
    }

    public sealed class SucursalSerieDto
    {
        public string Concepto { get; set; } = "";
        public char TipoCfdi { get; set; }          // 'I','E','P',...
        public string Serie { get; set; } = default!;
        public int FolioActual { get; set; }
        public bool Activo { get; set; }
    }

    public sealed class SucursalDetalleRazonDto
    {
        public Guid Id { get; set; }
        public string Rfc { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public bool EsDefault { get; set; }
        public bool Activo { get; set; }
    }
}
