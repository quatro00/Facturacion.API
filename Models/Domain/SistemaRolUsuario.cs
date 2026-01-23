using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class SistemaRolUsuario
{
    public Guid Id { get; set; }

    public Guid SistemaId { get; set; }

    public string Rol { get; set; } = null!;

    public string UsuarioId { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual SistemaRol SistemaRol { get; set; } = null!;

    public virtual AspNetUser Usuario { get; set; } = null!;
}
