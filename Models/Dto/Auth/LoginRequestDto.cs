namespace Facturacion.API.Models.Dto.Auth
{
    public class LoginRequestDto
    {
        public string email { get; set; }
        public string password { get; set; }
        public bool? remember { get; set; }
    }
}
