namespace Facturacion.API.Models.Dto.Cliente.Cliente
{
    public class ActualizarClienteDto
    {
        // Datos fiscales
        public string Rfc { get; set; }
        public string RazonSocial { get; set; }
        public Guid RegimenFiscal { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Pais { get; set; }
        public string CodigoPostal { get; set; }

        // Dirección (aunque no la mandes al CFDI, sí la guardas)
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Colonia { get; set; }
        public string Calle { get; set; }
        public string NoExterior { get; set; }
        public string NoInterior { get; set; }

        // Configuración (defaults)
        public string? MetodoPago { get; set; }
        public string? FormaPago { get; set; }
        public string? Moneda { get; set; }
        public string? Exportacion { get; set; }
        public string? UsoCfdi { get; set; }
    }
}
