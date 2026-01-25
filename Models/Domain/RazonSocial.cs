using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class RazonSocial
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public string Rfc { get; set; } = null!;

    public string RazonSocial1 { get; set; } = null!;

    public Guid RegimenFiscalId { get; set; }

    public string CelularNotificaciones { get; set; } = null!;

    public int FolioInicio { get; set; }

    public string CodigoPostal { get; set; } = null!;

    public string Municipio { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string Colonia { get; set; } = null!;

    public string Calle { get; set; } = null!;

    public string? NoInterior { get; set; }

    public string? NoExterior { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public byte[]? CerBase64 { get; set; }

    public byte[]? KeyBase64 { get; set; }

    public byte[]? KeyPassword { get; set; }

    public string? FechaModificacion { get; set; }

    public string? UsuarioModificacionId { get; set; }

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual CRegimenFiscal RegimenFiscal { get; set; } = null!;
}
