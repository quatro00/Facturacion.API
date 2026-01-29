namespace Facturacion.API.Models.Perfil
{
    public class RazonSocialDto
    {
        public Guid Id { get; set; }
        public Guid CuentaId { get; set; }

        public string Rfc { get; set; } = default!;
        public string RazonSocial { get; set; } = default!;
        public string? NombreComercial { get; set; }

        public Guid RegimenFiscalId { get; set; }

        public string? EmailEmisor { get; set; }
        public string? TelefonoEmisor { get; set; }
        public string CelularNotificaciones { get; set; } = default!;

        public int FolioInicio { get; set; }

        public string SerieIngresoDefault { get; set; } = default!;
        public string? SerieEgresoDefault { get; set; }
        public string? CpLugarExpedicionDefault { get; set; }

        public string CodigoPostal { get; set; } = default!;
        public string Estado { get; set; } = default!;
        public string Municipio { get; set; } = default!;
        public string Colonia { get; set; } = default!;
        public string Calle { get; set; } = default!;
        public string? NoExterior { get; set; }
        public string? NoInterior { get; set; }

        public bool Activo { get; set; }
        public bool EsDefault { get; set; }

        public string? FacturamaIssuerId { get; set; }

        public bool TieneSellos { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
