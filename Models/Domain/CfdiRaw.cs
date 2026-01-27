using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CfdiRaw
{
    public Guid Id { get; set; }

    public Guid CfdiId { get; set; }

    public string RequestJson { get; set; } = null!;

    public string ResponseJson { get; set; } = null!;

    public string? XmlPath { get; set; }

    public string? PdfPath { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CuentaId { get; set; }

    public virtual Cfdi Cfdi { get; set; } = null!;
}
