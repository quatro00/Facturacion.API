using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CfdiConceptoImpuesto
{
    public Guid Id { get; set; }

    public Guid CfdiConceptoId { get; set; }

    public string TipoImpuesto { get; set; } = null!;

    public decimal Tasa { get; set; }

    public decimal Base { get; set; }

    public decimal Importe { get; set; }

    public bool EsRetencion { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CuentaId { get; set; }

    public string? ImpuestoClave { get; set; }

    public string? TipoFactor { get; set; }

    public virtual CfdiConcepto CfdiConcepto { get; set; } = null!;
}
