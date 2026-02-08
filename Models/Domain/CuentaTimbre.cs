using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CuentaTimbre
{
    public Guid CuentaId { get; set; }

    public int Disponibles { get; set; }

    public int Consumidos { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;
}
