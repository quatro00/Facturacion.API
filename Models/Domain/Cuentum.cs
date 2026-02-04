using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class Cuentum
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<Cfdi> Cfdis { get; set; } = new List<Cfdi>();

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<RazonSocial> RazonSocials { get; set; } = new List<RazonSocial>();

    public virtual ICollection<SucursalRazonSocial> SucursalRazonSocials { get; set; } = new List<SucursalRazonSocial>();

    public virtual ICollection<SucursalSerie> SucursalSeries { get; set; } = new List<SucursalSerie>();

    public virtual ICollection<Sucursal> Sucursals { get; set; } = new List<Sucursal>();

    public virtual AspNetUser User { get; set; } = null!;
}
