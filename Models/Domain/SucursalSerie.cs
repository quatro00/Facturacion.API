using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class SucursalSerie
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public Guid SucursalId { get; set; }

    public string TipoCfdi { get; set; } = null!;

    public string Serie { get; set; } = null!;

    public int FolioActual { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacionId { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public string Concepto { get; set; } = null!;

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual Sucursal Sucursal { get; set; } = null!;
}
