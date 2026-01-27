using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CfdiConcepto
{
    public Guid Id { get; set; }

    public Guid CfdiId { get; set; }

    public string ClaveProdServ { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public string ObjetoImp { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CuentaId { get; set; }

    public virtual Cfdi Cfdi { get; set; } = null!;

    public virtual ICollection<CfdiConceptoImpuesto> CfdiConceptoImpuestos { get; set; } = new List<CfdiConceptoImpuesto>();
}
