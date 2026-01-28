using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CfdiStatus
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool EsFinal { get; set; }

    public int Orden { get; set; }

    public virtual ICollection<Cfdi> Cfdis { get; set; } = new List<Cfdi>();
}
