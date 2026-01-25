using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CCodigoPostal
{
    public Guid Id { get; set; }

    public string? DCodigo { get; set; }

    public string? DAsenta { get; set; }

    public string? DTipoAsenta { get; set; }

    public string? DMnpio { get; set; }

    public string? DEstado { get; set; }

    public string? DCiudad { get; set; }

    public string? DCp { get; set; }

    public string? CEstado { get; set; }

    public string? COficina { get; set; }

    public string? CCp { get; set; }

    public string? CTipoAsenta { get; set; }

    public string? CMnpio { get; set; }

    public string? IdAsentaCpcons { get; set; }

    public string? DZona { get; set; }

    public string? CCveCiudad { get; set; }
}
