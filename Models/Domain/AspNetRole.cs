using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class AspNetRole
{
    public string Id { get; set; } = null!;

    public int? SistemaId { get; set; }

    public string? Name { get; set; }

    public string? NormalizedName { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    public virtual AspNetSystem? Sistema { get; set; }

    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
