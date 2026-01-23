using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class SistemaRol
{
    public Guid SistemaId { get; set; }

    public string Rol { get; set; } = null!;

    public virtual Sistema Sistema { get; set; } = null!;

    public virtual ICollection<SistemaRolUsuario> SistemaRolUsuarios { get; set; } = new List<SistemaRolUsuario>();
}
