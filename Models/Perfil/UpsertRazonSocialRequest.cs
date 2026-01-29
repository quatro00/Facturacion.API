namespace Facturacion.API.Models.Perfil
{
    public class UpsertRazonSocialRequest
    {
        public Guid? Id { get; set; }

        public bool Activa { get; set; } = true;

        public string Rfc { get; set; } = default!;
        public string RazonSocial { get; set; } = default!;
        public Guid RegimenFiscalId { get; set; }

        public string? NombreComercial { get; set; }
        public string? EmailEmisor { get; set; }
        public string? TelefonoEmisor { get; set; }

        public int FolioInicio { get; set; }

        public string SerieIngresoDefault { get; set; } = default!;
        public string? SerieEgresoDefault { get; set; }

        public string? CpLugarExpedicionDefault { get; set; }

        public string? CelularNotificaciones { get; set; }

        public string CodigoPostal { get; set; } = default!;
        public string Estado { get; set; } = default!;
        public string Municipio { get; set; } = default!;
        public string Colonia { get; set; } = default!;
        public string Calle { get; set; } = default!;
        public string NoExterior { get; set; } = default!;
        public string? NoInterior { get; set; }
    }
}
