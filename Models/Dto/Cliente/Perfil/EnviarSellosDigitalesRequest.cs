namespace Facturacion.API.Models.Dto.Cliente.Perfil
{
    public class EnviarSellosDigitalesRequest
    {
        /// <summary>
        /// Certificado (.cer) en Base64
        /// </summary>
        public string CertificadoBase64 { get; set; }

        /// <summary>
        /// Llave privada (.key) en Base64
        /// </summary>
        public string LlavePrivadaBase64 { get; set; }

        /// <summary>
        /// Contraseña del archivo .key
        /// </summary>
        public string PasswordLlave { get; set; }
    }
}
