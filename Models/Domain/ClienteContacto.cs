using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class ClienteContacto
{
    public Guid Id { get; set; }

    public Guid ClienteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Puesto { get; set; }

    public bool Activo { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public string? UsuarioMofificacionId { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}
