using Microsoft.EntityFrameworkCore;
using StaffTaskList.Core.Entities;
using StaffTaskList.Core.ICurrentUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StaffTaskList.Data
{
    public class DatabaseContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Core.Entities.Task> Tasks { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<TaskDeparture> TaskDepartures { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // çalışan dll içinden configuration class ları bul

            modelBuilder.Entity<Core.Entities.Task>()
                .HasOne(x => x.Employee)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskDeparture>()
                .HasOne(x => x.Task)
                .WithMany(x => x.TaskDepartures)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            AddAuditInfo();
            var result = await base.SaveChangesAsync(cancellationToken);

            if (auditEntries.Any())
            {
                await AuditLogs.AddRangeAsync(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private List<AuditLog> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var username = _currentUserService.Username ?? "system";
            var entriesCommon = ChangeTracker
            .Entries<CommonEntity>()
            .Where(e => e.State == EntityState.Added);

            foreach (var entry in entriesCommon)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Created = username;
                    entry.Entity.CreatedDate = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Created = username;
                    entry.Entity.CreatedDate = DateTime.Now;
                }
            }

            var auditEntries = new List<AuditLog>();
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var audit = new AuditLog
                {
                    TableName = entry.Metadata.GetTableName(),
                    ChangedAt = DateTime.UtcNow,
                    UserName = _currentUserService.Username ?? "system", // burada o anki kullanıcıyı inject edebilirsin
                    Action = entry.State.ToString()
                };

                // Primary key
                var keyValues = new Dictionary<string, object>();
                foreach (var property in entry.Properties.Where(p => p.Metadata.IsPrimaryKey()))
                {
                    keyValues[property.Metadata.Name] = property.CurrentValue;
                }
                audit.KeyValues = System.Text.Json.JsonSerializer.Serialize(keyValues);

                // Added
                if (entry.State == EntityState.Added)
                {
                    var newValues = new Dictionary<string, object>();
                    foreach (var property in entry.Properties)
                    {
                        newValues[property.Metadata.Name] = property.CurrentValue;
                    }
                    audit.NewValues = System.Text.Json.JsonSerializer.Serialize(newValues);
                }
                // Deleted
                else if (entry.State == EntityState.Deleted)
                {
                    var oldValues = new Dictionary<string, object>();
                    foreach (var property in entry.Properties)
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                    }
                    audit.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues);
                }
                // Modified
                else if (entry.State == EntityState.Modified)
                {
                    var oldValues = new Dictionary<string, object>();
                    var newValues = new Dictionary<string, object>();
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            oldValues[property.Metadata.Name] = property.OriginalValue;
                            newValues[property.Metadata.Name] = property.CurrentValue;
                        }
                    }
                    audit.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues);
                    audit.NewValues = System.Text.Json.JsonSerializer.Serialize(newValues);
                }

                auditEntries.Add(audit);
            }

            return auditEntries;
        }

        private void AddAuditInfo()
        {
            var username = _currentUserService.Username ?? "system";

            foreach (var entry in ChangeTracker.Entries<CommonEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Created = username;
                    entry.Entity.CreatedDate = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Created = username;
                    entry.Entity.CreatedDate = DateTime.Now;
                }
            }
        }
    }
}
