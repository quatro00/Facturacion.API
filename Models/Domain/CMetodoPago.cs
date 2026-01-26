using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CMetodoPago
{
    public string MetodoPagoId { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime InicioVigencia { get; set; }

    public DateTime? TerminoVigencia { get; set; }

    public virtual ICollection<ClienteConfiguracion> ClienteConfiguracions { get; set; } = new List<ClienteConfiguracion>();
}
