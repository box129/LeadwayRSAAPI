using Microsoft.EntityFrameworkCore;
using Leadway_RSA_API.Models;

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
        public DbSet<Identification> Identifications { get; set; }

        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Executor> Executors { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<BeneficiaryAssetAllocation> BeneficiaryAssetAllocations { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

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

            // --- Identification Configurations --
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
            // 1. Get all entries that are currently being tracked by the DbContext
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Applicant && ( // Filter to only process 'Applicant' entities
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            // 2. Loop through each relevant entity
            foreach (var entityEntry in entries)
            {
                // 3. If the entity is being added for the first time
                if (entityEntry.State == EntityState.Added)
                {
                    // Set CreatedDate to current UTC time
                    ((Applicant)entityEntry.Entity).CreatedDate = DateTime.UtcNow;
                }

                // 4. In both Added and Modified cases, update LastModifiedDate
                ((Applicant)entityEntry.Entity).LastModifiedDate = DateTime.UtcNow;
            }

        }



    }
}
