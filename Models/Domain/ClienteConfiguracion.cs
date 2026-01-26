using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class ClienteConfiguracion
{
    public Guid Id { get; set; }

    public Guid ClienteId { get; set; }

    public string? MetodoPago { get; set; }

    public string? FormaPago { get; set; }

    public string Moneda { get; set; } = null!;

    public string Exportacion { get; set; } = null!;

    public string? UsoCfdiDefault { get; set; }

    public bool? Activo { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string? UsuarioModificacionId { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual CExportacion ExportacionNavigation { get; set; } = null!;

    public virtual CFormaPago? FormaPagoNavigation { get; set; }

    public virtual CMetodoPago? MetodoPagoNavigation { get; set; }

    public virtual CMonedum MonedaNavigation { get; set; } = null!;

    public virtual CUsoCfdi? UsoCfdiDefaultNavigation { get; set; }
}
