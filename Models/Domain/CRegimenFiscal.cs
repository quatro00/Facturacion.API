using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CRegimenFiscal
{
    public Guid Id { get; set; }

    public string RegimenFiscal { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool Fisica { get; set; }

    public bool Moral { get; set; }

    public DateTime InicioVigencia { get; set; }

    public DateTime? TerminoVigencia { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacion { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }
}
