using System;
using System.Collections.Generic;

namespace Facturacion.API.Models.Domain;

public partial class CFormaPago
{
    public string CFormaPago1 { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string Bancarizado { get; set; } = null!;

    public string NúmeroDeOperación { get; set; } = null!;

    public string RfcDelEmisorDeLaCuentaOrdenante { get; set; } = null!;

    public string CuentaOrdenante { get; set; } = null!;

    public string PatrónParaCuentaOrdenante { get; set; } = null!;

    public string RfcDelEmisorCuentaDeBeneficiario { get; set; } = null!;

    public string CuentaDeBenenficiario { get; set; } = null!;

    public string PatrónParaCuentaBeneficiaria { get; set; } = null!;

    public string TipoCadenaPago { get; set; } = null!;

    public string NombreDelBancoEmisorDeLaCuentaOrdenanteEnCasoDeExtranjero { get; set; } = null!;

    public DateTime FechaInicioDeVigencia { get; set; }

    public string? FechaFinDeVigencia { get; set; }

    public virtual ICollection<ClienteConfiguracion> ClienteConfiguracions { get; set; } = new List<ClienteConfiguracion>();
}
