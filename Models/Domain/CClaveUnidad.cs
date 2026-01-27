using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CClaveUnidad
{
    public string CClaveUnidad1 { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Nota { get; set; }
}
