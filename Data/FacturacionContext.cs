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

    public virtual DbSet<CCodigoPostal> CCodigoPostals { get; set; }

    public virtual DbSet<CRegimenFiscal> CRegimenFiscals { get; set; }

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

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Calle).HasMaxLength(50);
            entity.Property(e => e.CelularNotificaciones).HasMaxLength(50);
            entity.Property(e => e.CodigoPostal).HasMaxLength(50);
            entity.Property(e => e.Colonia).HasMaxLength(50);
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasMaxLength(50);
            entity.Property(e => e.Municipio).HasMaxLength(50);
            entity.Property(e => e.NoExterior).HasMaxLength(50);
            entity.Property(e => e.NoInterior).HasMaxLength(50);
            entity.Property(e => e.RazonSocial1)
                .HasMaxLength(250)
                .HasColumnName("RazonSocial");
            entity.Property(e => e.Rfc).HasMaxLength(50);
            entity.Property(e => e.UsuarioCreacionId).HasMaxLength(50);
            entity.Property(e => e.UsuarioModificacionId).HasMaxLength(50);

            entity.HasOne(d => d.Cuenta).WithMany(p => p.RazonSocials)
                .HasForeignKey(d => d.CuentaId)
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
