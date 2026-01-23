using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.API.Data
{
    public class AuthDbContext : IdentityDbContext<IdentityUser, ApplicationRole, string>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityUser>().ToTable("AspNetUsers", "dbo");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles", "dbo");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "dbo");
            /*
            builder.Entity<IdentityUserClaim>().ToTable("AspNetUserClaims", "portal_lamarina");
            
            builder.Entity<IdentityUserLogin>().ToTable("AspNetUserLogins", "portal_lamarina");
            builder.Entity<IdentityRoleClaim>().ToTable("AspNetRoleClaims", "portal_lamarina");
            builder.Entity<IdentityUserToken>().ToTable("AspNetUserTokens", "portal_lamarina");
            */

        }
    }

    public class ApplicationRole : IdentityRole
    {
        public int SistemaId { get; set; }
    }
}
