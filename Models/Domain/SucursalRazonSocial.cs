using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class SucursalRazonSocial
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public Guid SucursalId { get; set; }

    public Guid RazonSocialId { get; set; }

    public bool Activo { get; set; }

    public bool EsDefault { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual RazonSocial RazonSocial { get; set; } = null!;

    public virtual Sucursal Sucursal { get; set; } = null!;
}
