using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CfdiEstatusHistorial
{
    public Guid Id { get; set; }

    public Guid CfdiId { get; set; }

    public string Estatus { get; set; } = null!;

    public string? Motivo { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CuentaId { get; set; }

    public virtual Cfdi Cfdi { get; set; } = null!;
}
