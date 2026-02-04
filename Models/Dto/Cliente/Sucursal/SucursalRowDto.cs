namespace Facturacion.API.Models.Dto.Cliente.Sucursal
{
    public sealed class PagedResult<T>
    {
        public int Total { get; set; }
        public List<T> Items { get; set; } = new();
    }

    public sealed class SucursalRowDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public bool Activo { get; set; }

        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? Cp { get; set; }

        public List<SucursalRazonDto> RazonesSociales { get; set; } = new();
    }

    public sealed class SucursalRazonDto
    {
        public Guid Id { get; set; }
        public string Rfc { get; set; } = default!;
        public string RazonSocial { get; set; } = default!;
        public bool EsDefault { get; set; }
    }

    public sealed class SucursalesQueryDto
    {
        public string? Q { get; set; }
        public string Status { get; set; } = "ALL"; // ALL | ACTIVE | INACTIVE
        public Guid? RazonSocialId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string Sort { get; set; } = "codigo"; // codigo | nombre | activo | fechacreacion
        public string Dir { get; set; } = "asc";     // asc | desc
    }
}
