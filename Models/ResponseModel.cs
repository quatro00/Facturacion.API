namespace Facturacion.API.Models
{
    public class ResponseModel
    {
        public dynamic result { get; set; }
        public bool response { get; set; }
        public string message { get; set; }
        public string data { get; set; }

        public ResponseModel()
        {
            this.response = false;
            this.message = "Ocurrio un error inesperado";
        }

        public void SetResponse(bool r, string m = "")
        {
            this.response = r;
            this.message = m;

            if (m == "")
            {
                switch (r)
                {
                    case true:
                        this.message = "Datos guardados con éxito.";
                        break;
                    case false:
                        this.message = "Ocurrio un error inesperado";
                        break;
                }
            }
        }
    }
}
