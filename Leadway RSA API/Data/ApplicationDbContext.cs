using Leadway_RSA_API.Models;
using Leadway_RSA_API.Services;
using Microsoft.EntityFrameworkCore;

namespace Leadway_RSA_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Defining the DbSet for each entities
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<PersonalDetails> PersonalDetails { get; set; }
        public DbSet<Identification> Identifications { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Executor> Executors { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<BeneficiaryAssetAllocation> BeneficiaryAssetAllocations { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        // ADDED: DbSet for the RefreshToken entity
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // ADDED: DbSet for the RegistrationKey entity
        public DbSet<RegistrationKey> RegistrationKeys { get; set; }

        // Using Fluent API to configure relationships, constraints, and default values
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Applicant Configurationz ---
            modelBuilder.Entity<Applicant>()
                .HasIndex(a => a.EmailAddress) // Ensure email is unique.
                .IsUnique();

            modelBuilder.Entity<Applicant>()
                .Property(a => a.CreatedDate)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'"); // set default value for creation date

            modelBuilder.Entity<Applicant>()
                .Property(a => a.LastModifiedDate)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
                .ValueGeneratedOnAddOrUpdate(); // Database generates/updates this value

            // -- PersonalDetails Configurations ---
            modelBuilder.Entity<PersonalDetails>()
                .HasOne(pd => pd.Applicant)
                .WithOne(a => a.PersonalDetails)
                .HasForeignKey<PersonalDetails>(pd => pd.ApplicantId);

            // --- Identification Configurations ---
            modelBuilder.Entity<Identification>()
                .HasOne(i => i.Applicant)
                .WithMany(a => a.Identifications)
                .HasForeignKey(a => a.ApplicantId);

            // --- Beneficiary Configurations ---
            modelBuilder.Entity<Beneficiary>()
                .HasOne(b => b.Applicant)
                .WithMany(a => a.Beneficiaries)
                .HasForeignKey(b => b.ApplicantId);

            // --- Guardian Configurations ---
            modelBuilder.Entity<Guardian>()
                .HasOne(g => g.Applicant)
                .WithMany(a => a.Guardians)
                .HasForeignKey(g => g.ApplicantId);

            // --- Asset Configurations ---
            modelBuilder.Entity<Asset>()
                .HasOne(a => a.Applicant)
                .WithMany(app => app.Assets)
                .HasForeignKey(a => a.ApplicantId);

            // --- PaymentTransaction Configurations ---
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Applicant)
                .WithMany(a => a.PaymentTransactions)
                .HasForeignKey(pt => pt.ApplicantId);

            // --- BeneficiaryAssetAllocation Configurations (Junction Table) ---
            // Ensure unique combination of AssetId and BeneficiaryId for a single allocation
            modelBuilder.Entity<BeneficiaryAssetAllocation>()
                .HasIndex(baa => new { baa.AssetId, baa.BeneficiaryId })
                .IsUnique();

            // Define the relationships for the junction table
            modelBuilder.Entity<BeneficiaryAssetAllocation>()
                .HasOne(baa => baa.Asset)
                .WithMany(a => a.AssetAllocations)
                .HasForeignKey(baa => baa.AssetId);

            modelBuilder.Entity<BeneficiaryAssetAllocation>()
                .HasOne(baa => baa.Beneficiary)
                .WithMany(b => b.AssetAllocations)
                .HasForeignKey(baa => baa.BeneficiaryId);

            // Also link the junction table directly to Applicant for easy querying (optional but good practice)
            modelBuilder.Entity<BeneficiaryAssetAllocation>()
                .HasOne(baa => baa.Applicant)
                .WithMany(a => a.AssetAllocations)
                .HasForeignKey(baa => baa.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of Applicant if allocations exist

            // ADDED: Configuration for the RegistrationKey entity
            modelBuilder.Entity<RegistrationKey>()
                .HasKey(k => k.Id);

            modelBuilder.Entity<RegistrationKey>()
                .HasIndex(k => k.Key) // Ensure the key itself is unique.
                .IsUnique();

            modelBuilder.Entity<RegistrationKey>()
                .HasOne(k => k.Applicant) // A registration key belongs to one applicant.
                .WithOne(a => a.RegistrationKey) // An applicant has one registration key at a time.
                .HasForeignKey<RegistrationKey>(k => k.ApplicantId);
        }

        // Optional: Override SaveChanges to automatically update LastModifiedDate
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateAuditFields();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditable && // Now we check for the interface
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                // Cast the entity to the IAuditable interface
                var auditableEntity = (IAuditable)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    auditableEntity.CreatedDate = DateTime.UtcNow;
                }

                auditableEntity.LastModifiedDate = DateTime.UtcNow;
            }
        }
    }
}
