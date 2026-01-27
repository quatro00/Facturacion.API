using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CConcepto
{
    public string CClaveProdServ { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string IncluirIvaTrasladado { get; set; } = null!;

    public string IncluirIepsTrasladado { get; set; } = null!;

    public string? Complemento { get; set; }

    public DateTime? VigenciaInicio { get; set; }

    public string? VigenciaTermino { get; set; }

    public bool EstimuloFranjaFronteriza { get; set; }
}
