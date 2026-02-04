using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class Sucursal
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? CodigoPostal { get; set; }

    public string? Municipio { get; set; }

    public string? Estado { get; set; }

    public string? Colonia { get; set; }

    public string? Calle { get; set; }

    public string? NoInterior { get; set; }

    public string? NoExterior { get; set; }

    public bool Activo { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public string? UsuarioEliminacionId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacionId { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Cfdi> Cfdis { get; set; } = new List<Cfdi>();

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual ICollection<SucursalRazonSocial> SucursalRazonSocials { get; set; } = new List<SucursalRazonSocial>();

    public virtual ICollection<SucursalSerie> SucursalSeries { get; set; } = new List<SucursalSerie>();
}
