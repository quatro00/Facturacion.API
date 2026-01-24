namespace Facturacion.API.Models.Dto.Cliente
{
    public class RegistroClienteRequestDto
    {
        public string name {  get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string confirmEmail { get; set; }
        public string password { get; set; }
        public bool terminosCondiciones { get; set; }
    }
}
