using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class Sistema
{
    public Guid Id { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public virtual ICollection<SistemaRol> SistemaRols { get; set; } = new List<SistemaRol>();
}
