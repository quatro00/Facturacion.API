using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CMonedum
{
    public string CMoneda { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public byte Decimales { get; set; }

    public string? PorcentajeVariación { get; set; }

    public DateTime InicioVigencia { get; set; }

    public string? TerminoVigencia { get; set; }

    public virtual ICollection<ClienteConfiguracion> ClienteConfiguracions { get; set; } = new List<ClienteConfiguracion>();
}
