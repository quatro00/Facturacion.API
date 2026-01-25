namespace Facturacion.API.Models.Dto.Cliente.Catalogos
{
    public class ClienteGetRegimenFiscal
    {
        public Guid Id { get; set; }

        public string RegimenFiscal { get; set; } = null!;

        public string Descripcion { get; set; } = null!;

        public bool Fisica { get; set; }

        public bool Moral { get; set; }

        public DateTime InicioVigencia { get; set; }

        public DateTime? TerminoVigencia { get; set; }

        public bool Activo { get; set; }
    }
}
