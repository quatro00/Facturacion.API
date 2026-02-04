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

    public string? RazonSocialEmisor { get; set; }

    public string? RazonSocialReceptor { get; set; }

    public string Pac { get; set; } = null!;

    public string Estatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string FacturamaId { get; set; } = null!;

    public string? MotivoCancelacion { get; set; }

    public Guid? UuidSustitucion { get; set; }

    public DateTime? FechaSolicitudCancel { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public string? EstatusCancelacionSat { get; set; }

    public string? AcuseCancelacionXml { get; set; }

    public int CfdiStatusId { get; set; }

    public string? EmisorRegimenFiscal { get; set; }

    public string? UsoCfdi { get; set; }

    public string? ReceptorRegimenFiscal { get; set; }

    public string? ReceptorTaxZipCode { get; set; }

    public string? Exportacion { get; set; }

    public Guid? CfdiOrigenId { get; set; }

    public string? TipoRelacionSat { get; set; }

    public Guid RazonSocialId { get; set; }

    public Guid? SucursalId { get; set; }

    public virtual ICollection<CfdiConcepto> CfdiConceptos { get; set; } = new List<CfdiConcepto>();

    public virtual ICollection<CfdiEstatusHistorial> CfdiEstatusHistorials { get; set; } = new List<CfdiEstatusHistorial>();

    public virtual Cfdi? CfdiOrigen { get; set; }

    public virtual ICollection<CfdiRaw> CfdiRaws { get; set; } = new List<CfdiRaw>();

    public virtual CfdiStatus CfdiStatus { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual ICollection<Cfdi> InverseCfdiOrigen { get; set; } = new List<Cfdi>();

    public virtual RazonSocial RazonSocial { get; set; } = null!;

    public virtual Sucursal? Sucursal { get; set; }
}
