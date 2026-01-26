using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CExportacion
{
    public string CExportacion1 { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateTime InicioVigencia { get; set; }

    public string? TerminoVigencia { get; set; }

    public virtual ICollection<ClienteConfiguracion> ClienteConfiguracions { get; set; } = new List<ClienteConfiguracion>();
}
