using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class Cfdi
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public Guid ClienteId { get; set; }

    public Guid Uuid { get; set; }

    public string? Serie { get; set; }

    public string? Folio { get; set; }

    public string TipoCfdi { get; set; } = null!;

    public DateTime FechaTimbrado { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public string Moneda { get; set; } = null!;

    public string? FormaPago { get; set; }

    public string? MetodoPago { get; set; }

    public string LugarExpedicion { get; set; } = null!;

    public string RfcEmisor { get; set; } = null!;

    public string RfcReceptor { get; set; } = null!;

    public string Pac { get; set; } = null!;

    public string Estatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? FacturamaId { get; set; }

    public virtual ICollection<CfdiConcepto> CfdiConceptos { get; set; } = new List<CfdiConcepto>();

    public virtual ICollection<CfdiEstatusHistorial> CfdiEstatusHistorials { get; set; } = new List<CfdiEstatusHistorial>();

    public virtual ICollection<CfdiRaw> CfdiRaws { get; set; } = new List<CfdiRaw>();

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Cuentum Cuenta { get; set; } = null!;
}
