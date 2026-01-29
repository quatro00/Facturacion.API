using System;
using System.Collections.Generic;
using Facturacion.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.API.Data;

public partial class FacturacionContext : DbContext
{
    public FacturacionContext()
    {
    }

    public FacturacionContext(DbContextOptions<FacturacionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetSystem> AspNetSystems { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<CClaveUnidad> CClaveUnidads { get; set; }

    public virtual DbSet<CCodigoPostal> CCodigoPostals { get; set; }

    public virtual DbSet<CConcepto> CConceptos { get; set; }

    public virtual DbSet<CExportacion> CExportacions { get; set; }

    public virtual DbSet<CFormaPago> CFormaPagos { get; set; }

    public virtual DbSet<CMetodoPago> CMetodoPagos { get; set; }

    public virtual DbSet<CMonedum> CMoneda { get; set; }

    public virtual DbSet<CRegimenFiscal> CRegimenFiscals { get; set; }

    public virtual DbSet<CUsoCfdi> CUsoCfdis { get; set; }

    public virtual DbSet<Cfdi> Cfdis { get; set; }

    public virtual DbSet<CfdiConcepto> CfdiConceptos { get; set; }

    public virtual DbSet<CfdiConceptoImpuesto> CfdiConceptoImpuestos { get; set; }

    public virtual DbSet<CfdiEstatusHistorial> CfdiEstatusHistorials { get; set; }

    public virtual DbSet<CfdiRaw> CfdiRaws { get; set; }

    public virtual DbSet<CfdiStatus> CfdiStatuses { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ClienteConfiguracion> ClienteConfiguracions { get; set; }

    public virtual DbSet<ClienteContacto> ClienteContactos { get; set; }

    public virtual DbSet<ClientePac> ClientePacs { get; set; }

    public virtual DbSet<Cuentum> Cuenta { get; set; }

    public virtual DbSet<Organizacion> Organizacions { get; set; }

    public virtual DbSet<RazonSocial> RazonSocials { get; set; }

    public virtual DbSet<Sistema> Sistemas { get; set; }

    public virtual DbSet<SistemaRol> SistemaRols { get; set; }

    public virtual DbSet<SistemaRolUsuario> SistemaRolUsuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-JM00DK5;Initial Catalog=Facturacion;Persist Security Info=True;User ID=sa;Password=sql2;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);

            entity.HasOne(d => d.Sistema).WithMany(p => p.AspNetRoles)
                .HasForeignKey(d => d.SistemaId)
                .HasConstraintName("FK_AspNetRoles_AspNetSystem");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.Property(e => e.RoleId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetSystem>(entity =>
        {
            entity.ToTable("AspNetSystem");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(500);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Apellidos).HasMaxLength(250);
            entity.Property(e => e.Avatar).HasMaxLength(450);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasOne(d => d.Organizacion).WithMany(p => p.AspNetUsers)
                .HasForeignKey(d => d.OrganizacionId)
                .HasConstraintName("FK_AspNetUsers_Organizacion");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CClaveUnidad>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("cClaveUnidad");

            entity.HasIndex(e => e.CClaveUnidad1, "IX_CClaveUnidad_CClaveUnidad");

            entity.Property(e => e.CClaveUnidad1)
                .HasMaxLength(50)
                .HasColumnName("cClaveUnidad");
            entity.Property(e => e.Descripcion).HasMaxLength(550);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Nota).HasMaxLength(250);
        });

        modelBuilder.Entity<CCodigoPostal>(entity =>
        {
            entity.ToTable("cCodigoPostal");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CCp)
                .HasMaxLength(150)
                .HasColumnName("c_CP");
            entity.Property(e => e.CCveCiudad)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("c_cve_ciudad");
            entity.Property(e => e.CEstado)
                .HasMaxLength(150)
                .HasColumnName("c_estado");
            entity.Property(e => e.CMnpio)
                .HasMaxLength(150)
                .HasColumnName("c_mnpio");
            entity.Property(e => e.COficina)
                .HasMaxLength(150)
                .HasColumnName("c_oficina");
            entity.Property(e => e.CTipoAsenta)
                .HasMaxLength(150)
                .HasColumnName("c_tipo_asenta");
            entity.Property(e => e.DAsenta)
                .HasMaxLength(150)
                .HasColumnName("d_asenta");
            entity.Property(e => e.DCiudad)
                .HasMaxLength(150)
                .HasColumnName("d_ciudad");
            entity.Property(e => e.DCodigo)
                .HasMaxLength(10)
                .HasColumnName("d_codigo");
            entity.Property(e => e.DCp)
                .HasMaxLength(150)
                .HasColumnName("d_CP");
            entity.Property(e => e.DEstado)
                .HasMaxLength(150)
                .HasColumnName("d_estado");
            entity.Property(e => e.DMnpio)
                .HasMaxLength(150)
                .HasColumnName("D_mnpio");
            entity.Property(e => e.DTipoAsenta)
                .HasMaxLength(150)
                .HasColumnName("d_tipo_asenta");
            entity.Property(e => e.DZona)
                .HasMaxLength(150)
                .HasColumnName("d_zona");
            entity.Property(e => e.IdAsentaCpcons)
                .HasMaxLength(150)
                .HasColumnName("id_asenta_cpcons");
        });

        modelBuilder.Entity<CConcepto>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("cConceptos");

            entity.HasIndex(e => e.CClaveProdServ, "IX_CConceptos_CClaveProdServ");

            entity.Property(e => e.CClaveProdServ)
                .HasMaxLength(50)
                .HasColumnName("cClaveProdServ");
            entity.Property(e => e.Complemento).HasMaxLength(1);
            entity.Property(e => e.Descripcion).HasMaxLength(400);
            entity.Property(e => e.IncluirIepsTrasladado).HasMaxLength(50);
            entity.Property(e => e.IncluirIvaTrasladado).HasMaxLength(50);
            entity.Property(e => e.VigenciaInicio).HasColumnType("date");
            entity.Property(e => e.VigenciaTermino).HasMaxLength(1);
        });

        modelBuilder.Entity<CExportacion>(entity =>
        {
            entity.HasKey(e => e.CExportacion1);

            entity.ToTable("cExportacion");

            entity.Property(e => e.CExportacion1)
                .HasMaxLength(2)
                .HasColumnName("cExportacion");
            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.InicioVigencia).HasColumnType("date");
            entity.Property(e => e.TerminoVigencia).HasMaxLength(1);
        });

        modelBuilder.Entity<CFormaPago>(entity =>
        {
            entity.HasKey(e => e.CFormaPago1).HasName("PK_FormaPago");

            entity.ToTable("cFormaPago");

            entity.Property(e => e.CFormaPago1)
                .HasMaxLength(2)
                .HasColumnName("c_FormaPago");
            entity.Property(e => e.Bancarizado).HasMaxLength(50);
            entity.Property(e => e.CuentaDeBenenficiario)
                .HasMaxLength(50)
                .HasColumnName("Cuenta_de_Benenficiario");
            entity.Property(e => e.CuentaOrdenante)
                .HasMaxLength(50)
                .HasColumnName("Cuenta_Ordenante");
            entity.Property(e => e.Descripcion).HasMaxLength(50);
            entity.Property(e => e.FechaFinDeVigencia)
                .HasMaxLength(1)
                .HasColumnName("Fecha_fin_de_vigencia");
            entity.Property(e => e.FechaInicioDeVigencia)
                .HasColumnType("date")
                .HasColumnName("Fecha_inicio_de_vigencia");
            entity.Property(e => e.NombreDelBancoEmisorDeLaCuentaOrdenanteEnCasoDeExtranjero)
                .HasMaxLength(100)
                .HasColumnName("Nombre_del_Banco_emisor_de_la_cuenta_ordenante_en_caso_de_extranjero");
            entity.Property(e => e.NúmeroDeOperación)
                .HasMaxLength(50)
                .HasColumnName("Número_de_operación");
            entity.Property(e => e.PatrónParaCuentaBeneficiaria)
                .HasMaxLength(100)
                .HasColumnName("Patrón_para_cuenta_Beneficiaria");
            entity.Property(e => e.PatrónParaCuentaOrdenante)
                .HasMaxLength(100)
                .HasColumnName("Patrón_para_cuenta_ordenante");
            entity.Property(e => e.RfcDelEmisorCuentaDeBeneficiario)
                .HasMaxLength(50)
                .HasColumnName("RFC_del_Emisor_Cuenta_de_Beneficiario");
            entity.Property(e => e.RfcDelEmisorDeLaCuentaOrdenante)
                .HasMaxLength(50)
                .HasColumnName("RFC_del_Emisor_de_la_cuenta_ordenante");
            entity.Property(e => e.TipoCadenaPago)
                .HasMaxLength(50)
                .HasColumnName("Tipo_Cadena_Pago");
        });

        modelBuilder.Entity<CMetodoPago>(entity =>
        {
            entity.HasKey(e => e.MetodoPagoId);

            entity.ToTable("cMetodoPago");

            entity.Property(e => e.MetodoPagoId).HasMaxLength(3);
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.InicioVigencia).HasColumnType("datetime");
            entity.Property(e => e.TerminoVigencia).HasColumnType("datetime");
        });

        modelBuilder.Entity<CMonedum>(entity =>
        {
            entity.HasKey(e => e.CMoneda);

            entity.ToTable("cMoneda");

            entity.Property(e => e.CMoneda)
                .HasMaxLength(3)
                .HasColumnName("cMoneda");
            entity.Property(e => e.Descripcion).HasMaxLength(100);
            entity.Property(e => e.InicioVigencia).HasColumnType("date");
            entity.Property(e => e.PorcentajeVariación)
                .HasMaxLength(50)
                .HasColumnName("Porcentaje_variación");
            entity.Property(e => e.TerminoVigencia).HasMaxLength(1);
        });

        modelBuilder.Entity<CRegimenFiscal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_c_RegimenFiscal");

            entity.ToTable("cRegimenFiscal");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.InicioVigencia).HasColumnType("datetime");
            entity.Property(e => e.RegimenFiscal).HasMaxLength(50);
            entity.Property(e => e.TerminoVigencia).HasColumnType("datetime");
            entity.Property(e => e.UsuarioCreacion).HasMaxLength(50);
            entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);
        });

        modelBuilder.Entity<CUsoCfdi>(entity =>
        {
            entity.HasKey(e => e.CUsoCfdi1).HasName("PK__cUsoCFDI__B6A94A1FE200928E");

            entity.ToTable("cUsoCFDI");

            entity.Property(e => e.CUsoCfdi1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("c_UsoCFDI");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.InicioVigencia).HasColumnType("datetime");
            entity.Property(e => e.RegimenReceptor).HasMaxLength(450);
            entity.Property(e => e.TerminoVigencia).HasColumnType("datetime");
        });

        modelBuilder.Entity<Cfdi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cfdi__3214EC071EC437D1");

            entity.ToTable("Cfdi");

            entity.HasIndex(e => new { e.ClienteId, e.FechaTimbrado }, "IX_Cfdi_Cliente_Fecha");

            entity.HasIndex(e => new { e.CuentaId, e.FechaTimbrado }, "IX_Cfdi_Cuenta_Fecha");

            entity.HasIndex(e => new { e.CuentaId, e.FechaTimbrado, e.Estatus }, "IX_Cfdi_Cuenta_Fecha_Estatus");

            entity.HasIndex(e => new { e.CuentaId, e.Serie, e.Folio }, "IX_Cfdi_Cuenta_SerieFolio");

            entity.HasIndex(e => new { e.Serie, e.Folio }, "IX_Cfdi_SerieFolio");

            entity.HasIndex(e => e.Uuid, "IX_Cfdi_Uuid").IsUnique();

            entity.HasIndex(e => new { e.CuentaId, e.FacturamaId }, "UX_Cfdi_Cuenta_FacturamaId").IsUnique();

            entity.HasIndex(e => new { e.CuentaId, e.Uuid }, "UX_Cfdi_Cuenta_Uuid").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CfdiStatusId).HasDefaultValueSql("((2))");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EmisorRegimenFiscal)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Estatus).HasMaxLength(30);
            entity.Property(e => e.EstatusCancelacionSat).HasMaxLength(30);
            entity.Property(e => e.Exportacion)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FacturamaId).HasMaxLength(50);
            entity.Property(e => e.Folio).HasMaxLength(20);
            entity.Property(e => e.FormaPago)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LugarExpedicion)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MotivoCancelacion)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Pac).HasMaxLength(50);
            entity.Property(e => e.RazonSocialEmisor).HasMaxLength(150);
            entity.Property(e => e.RazonSocialReceptor).HasMaxLength(150);
            entity.Property(e => e.ReceptorRegimenFiscal)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ReceptorTaxZipCode)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RfcEmisor)
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RfcReceptor)
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Serie).HasMaxLength(10);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoCfdi)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TipoRelacionSat)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UsoCfdi)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.CfdiOrigen).WithMany(p => p.InverseCfdiOrigen)
                .HasForeignKey(d => d.CfdiOrigenId)
                .HasConstraintName("FK_Cfdi_CfdiOrigen");

            entity.HasOne(d => d.CfdiStatus).WithMany(p => p.Cfdis)
                .HasForeignKey(d => d.CfdiStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cfdi_CfdiStatus");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Cfdis)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cfdi_Cliente");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.Cfdis)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cfdi_Cuenta");
        });

        modelBuilder.Entity<CfdiConcepto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CfdiConc__3214EC0799B64317");

            entity.ToTable("CfdiConcepto");

            entity.HasIndex(e => new { e.CuentaId, e.CfdiId }, "IX_CfdiConcepto_Cuenta_Cfdi");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ClaveProdServ)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ClaveUnidad)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NoIdentificacion).HasMaxLength(100);
            entity.Property(e => e.ObjetoImp)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unidad).HasMaxLength(50);
            entity.Property(e => e.ValorUnitario).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.Cfdi).WithMany(p => p.CfdiConceptos)
                .HasForeignKey(d => d.CfdiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CfdiConcepto_Cfdi");
        });

        modelBuilder.Entity<CfdiConceptoImpuesto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CfdiConc__3214EC07BD1E8B90");

            entity.ToTable("CfdiConceptoImpuesto");

            entity.HasIndex(e => new { e.CuentaId, e.CfdiConceptoId }, "IX_CfdiConceptoImp_Cuenta_Concepto");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Base).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ImpuestoClave)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Tasa).HasColumnType("decimal(10, 6)");
            entity.Property(e => e.TipoFactor)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TipoImpuesto).HasMaxLength(10);

            entity.HasOne(d => d.CfdiConcepto).WithMany(p => p.CfdiConceptoImpuestos)
                .HasForeignKey(d => d.CfdiConceptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CfdiConceptoImpuesto_Concepto");
        });

        modelBuilder.Entity<CfdiEstatusHistorial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CfdiEsta__3214EC077A433924");

            entity.ToTable("CfdiEstatusHistorial");

            entity.HasIndex(e => new { e.CuentaId, e.CfdiId }, "IX_CfdiEstatus_Cuenta_Cfdi");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Estatus).HasMaxLength(30);
            entity.Property(e => e.Motivo).HasMaxLength(250);

            entity.HasOne(d => d.Cfdi).WithMany(p => p.CfdiEstatusHistorials)
                .HasForeignKey(d => d.CfdiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CfdiEstatus_Cfdi");
        });

        modelBuilder.Entity<CfdiRaw>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CfdiRaw__3214EC0770D9CC07");

            entity.ToTable("CfdiRaw");

            entity.HasIndex(e => new { e.CuentaId, e.CfdiId }, "UX_CfdiRaw_Cuenta_Cfdi").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PdfPath).HasMaxLength(500);
            entity.Property(e => e.XmlPath).HasMaxLength(500);

            entity.HasOne(d => d.Cfdi).WithMany(p => p.CfdiRaws)
                .HasForeignKey(d => d.CfdiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CfdiRaw_Cfdi");
        });

        modelBuilder.Entity<CfdiStatus>(entity =>
        {
            entity.ToTable("CfdiStatus");

            entity.HasIndex(e => e.Clave, "UQ_CfdiStatus_Clave").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Clave).HasMaxLength(30);
            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");

            entity.HasIndex(e => new { e.CuentaId, e.RazonSocial }, "IX_Cliente_Cuenta_RazonSocial");

            entity.HasIndex(e => new { e.CuentaId, e.Rfc }, "UX_Cliente_Cuenta_Rfc").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Calle).HasMaxLength(150);
            entity.Property(e => e.CodigoPostal).HasMaxLength(50);
            entity.Property(e => e.Colonia).HasMaxLength(50);
            entity.Property(e => e.CorreosCc).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Localidad).HasMaxLength(80);
            entity.Property(e => e.Municipio).HasMaxLength(50);
            entity.Property(e => e.NombreComercial).HasMaxLength(150);
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Property(e => e.NumeroExterior).HasMaxLength(50);
            entity.Property(e => e.NumeroInterior).HasMaxLength(50);
            entity.Property(e => e.Pais)
                .HasMaxLength(50)
                .HasDefaultValueSql("(N'México')");
            entity.Property(e => e.RazonSocial).HasMaxLength(250);
            entity.Property(e => e.Referencia).HasMaxLength(150);
            entity.Property(e => e.Rfc).HasMaxLength(50);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.TipoPersona)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("('M')")
                .IsFixedLength();
            entity.Property(e => e.UsuarioCreacionId).HasMaxLength(50);
            entity.Property(e => e.UsuarioModificacion).HasMaxLength(50);

            entity.HasOne(d => d.Cuenta).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_Cuenta");

            entity.HasOne(d => d.RegimenFiscal).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.RegimenFiscalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_cRegimenFiscal");
        });

        modelBuilder.Entity<ClienteConfiguracion>(entity =>
        {
            entity.ToTable("ClienteConfiguracion");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Exportacion)
                .HasMaxLength(2)
                .HasDefaultValueSql("('01')");
            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FormaPago).HasMaxLength(2);
            entity.Property(e => e.MetodoPago).HasMaxLength(3);
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValueSql("('MXN')");
            entity.Property(e => e.UsoCfdiDefault)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("UsoCFDI_Default");
            entity.Property(e => e.UsuarioCreacionId).HasMaxLength(50);
            entity.Property(e => e.UsuarioModificacionId).HasMaxLength(50);

            entity.HasOne(d => d.Cliente).WithMany(p => p.ClienteConfiguracions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClienteConfiguracion_Cliente");

            entity.HasOne(d => d.ExportacionNavigation).WithMany(p => p.ClienteConfiguracions)
                .HasForeignKey(d => d.Exportacion)
                .HasConstraintName("FK_ClienteConfiguracion_cExportacion");

            entity.HasOne(d => d.FormaPagoNavigation).WithMany(p => p.ClienteConfiguracions)
                .HasForeignKey(d => d.FormaPago)
                .HasConstraintName("FK_ClienteConfiguracion_cFormaPago");

            entity.HasOne(d => d.MetodoPagoNavigation).WithMany(p => p.ClienteConfiguracions)
                .HasForeignKey(d => d.MetodoPago)
                .HasConstraintName("FK_ClienteConfiguracion_cMetodoPago");

            entity.HasOne(d => d.MonedaNavigation).WithMany(p => p.ClienteConfiguracions)
                .HasForeignKey(d => d.Moneda)
                .HasConstraintName("FK_ClienteConfiguracion_cMoneda");

            entity.HasOne(d => d.UsoCfdiDefaultNavigation).WithMany(p => p.ClienteConfiguracions)
                .HasForeignKey(d => d.UsoCfdiDefault)
                .HasConstraintName("FK_ClienteConfiguracion_cUsoCFDI");
        });

        modelBuilder.Entity<ClienteContacto>(entity =>
        {
            entity.ToTable("ClienteContacto");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Puesto).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(150);
            entity.Property(e => e.UsuarioCreacionId).HasMaxLength(50);
            entity.Property(e => e.UsuarioMofificacionId).HasMaxLength(50);

            entity.HasOne(d => d.Cliente).WithMany(p => p.ClienteContactos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClienteContacto_Cliente");
        });

        modelBuilder.Entity<ClientePac>(entity =>
        {
            entity.ToTable("ClientePac");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.IdExterno).HasMaxLength(150);
            entity.Property(e => e.ProveedorPac)
                .HasMaxLength(150)
                .HasColumnName("ProveedorPAC");
            entity.Property(e => e.UsuarioCreacionId).HasMaxLength(50);
            entity.Property(e => e.UsuarioMofificacionId).HasMaxLength(50);

            entity.HasOne(d => d.Cliente).WithMany(p => p.ClientePacs)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientePac_Cliente");
        });

        modelBuilder.Entity<Cuentum>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cuenta_AspNetUsers");
        });

        modelBuilder.Entity<Organizacion>(entity =>
        {
            entity.ToTable("Organizacion");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Clave).HasMaxLength(500);
            entity.Property(e => e.Direccion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(500);
            entity.Property(e => e.Responsable).HasMaxLength(500);
            entity.Property(e => e.Telefono).HasMaxLength(500);
        });

        modelBuilder.Entity<RazonSocial>(entity =>
        {
            entity.ToTable("RazonSocial");

            entity.HasIndex(e => e.CuentaId, "UX_RazonSocial_Cuenta_Default")
                .IsUnique()
                .HasFilter("([EsDefault]=(1))");

            entity.HasIndex(e => new { e.CuentaId, e.Rfc }, "UX_RazonSocial_Cuenta_Rfc").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Calle).HasMaxLength(50);
            entity.Property(e => e.CelularNotificaciones).HasMaxLength(50);
            entity.Property(e => e.CodigoPostal).HasMaxLength(50);
            entity.Property(e => e.Colonia).HasMaxLength(50);
            entity.Property(e => e.CpLugarExpedicionDefault).HasMaxLength(5);
            entity.Property(e => e.EmailEmisor).HasMaxLength(254);
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.FacturamaIssuerId).HasMaxLength(80);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Municipio).HasMaxLength(50);
            entity.Property(e => e.NoExterior).HasMaxLength(50);
            entity.Property(e => e.NoInterior).HasMaxLength(50);
            entity.Property(e => e.NombreComercial).HasMaxLength(200);
            entity.Property(e => e.RazonSocial1)
                .HasMaxLength(250)
                .HasColumnName("RazonSocial");
            entity.Property(e => e.Rfc).HasMaxLength(50);
            entity.Property(e => e.SerieEgresoDefault).HasMaxLength(10);
            entity.Property(e => e.SerieIngresoDefault)
                .HasMaxLength(10)
                .HasDefaultValueSql("('A')");
            entity.Property(e => e.TelefonoEmisor).HasMaxLength(20);
            entity.Property(e => e.UsuarioCreacionId).HasMaxLength(50);
            entity.Property(e => e.UsuarioModificacionId).HasMaxLength(50);

            entity.HasOne(d => d.Cuenta).WithOne(p => p.RazonSocial)
                .HasForeignKey<RazonSocial>(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RazonSocial_Cuenta");

            entity.HasOne(d => d.RegimenFiscal).WithMany(p => p.RazonSocials)
                .HasForeignKey(d => d.RegimenFiscalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RazonSocial_cRegimenFiscal");
        });

        modelBuilder.Entity<Sistema>(entity =>
        {
            entity.ToTable("Sistema");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.UsuarioCreacion).HasMaxLength(50);
        });

        modelBuilder.Entity<SistemaRol>(entity =>
        {
            entity.HasKey(e => new { e.SistemaId, e.Rol });

            entity.ToTable("SistemaRol");

            entity.Property(e => e.Rol).HasMaxLength(50);

            entity.HasOne(d => d.Sistema).WithMany(p => p.SistemaRols)
                .HasForeignKey(d => d.SistemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SistemaRol_Sistema");
        });

        modelBuilder.Entity<SistemaRolUsuario>(entity =>
        {
            entity.ToTable("SistemaRolUsuario");

            entity.HasIndex(e => new { e.SistemaId, e.Rol, e.UsuarioId }, "UQ_SistemaRolUsuario_Sistema_Rol_Usuario").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Rol).HasMaxLength(50);

            entity.HasOne(d => d.Usuario).WithMany(p => p.SistemaRolUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SistemaRolUsuario_AspNetUsers");

            entity.HasOne(d => d.SistemaRol).WithMany(p => p.SistemaRolUsuarios)
                .HasForeignKey(d => new { d.SistemaId, d.Rol })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SistemaRolOrganizacion_SistemaRol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
