using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CuentaTimbreMovimiento
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public string Accion { get; set; } = null!;

    public int Timbres { get; set; }

    public Guid? CfdiId { get; set; }

    public string? FacturamaId { get; set; }

    public string? Uuid { get; set; }

    public string? Referencia { get; set; }

    public string? Notas { get; set; }

    public string? MetaJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
}
