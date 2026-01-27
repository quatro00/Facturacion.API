using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class Cliente
{
    public Guid Id { get; set; }

    public Guid CuentaId { get; set; }

    public string Rfc { get; set; } = null!;

    public string RazonSocial { get; set; } = null!;

    public Guid RegimenFiscalId { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string Calle { get; set; } = null!;

    public string NumeroExterior { get; set; } = null!;

    public string? NumeroInterior { get; set; }

    public string Colonia { get; set; } = null!;

    public string Municipio { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string Pais { get; set; } = null!;

    public string CodigoPostal { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string UsuarioCreacionId { get; set; } = null!;

    public DateTime? FechaModificacion { get; set; }

    public string? UsuarioModificacion { get; set; }

    public virtual ICollection<Cfdi> Cfdis { get; set; } = new List<Cfdi>();

    public virtual ICollection<ClienteConfiguracion> ClienteConfiguracions { get; set; } = new List<ClienteConfiguracion>();

    public virtual ICollection<ClienteContacto> ClienteContactos { get; set; } = new List<ClienteContacto>();

    public virtual ICollection<ClientePac> ClientePacs { get; set; } = new List<ClientePac>();

    public virtual Cuentum Cuenta { get; set; } = null!;

    public virtual CRegimenFiscal RegimenFiscal { get; set; } = null!;
}
