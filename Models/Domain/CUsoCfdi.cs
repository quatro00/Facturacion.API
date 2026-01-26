using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CUsoCfdi
{
    public string CUsoCfdi1 { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool Fisica { get; set; }

    public bool Moral { get; set; }

    public DateTime InicioVigencia { get; set; }

    public DateTime? TerminoVigencia { get; set; }

    public string RegimenReceptor { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<ClienteConfiguracion> ClienteConfiguracions { get; set; } = new List<ClienteConfiguracion>();
}
