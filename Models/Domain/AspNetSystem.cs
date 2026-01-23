using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class AspNetSystem
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<AspNetRole> AspNetRoles { get; set; } = new List<AspNetRole>();
}
