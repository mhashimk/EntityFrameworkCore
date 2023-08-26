using EntityFrameworkCore.Domain;
using EntityFrameworkCore.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Data
{
    public abstract class AuditableFootballLeagueDbContext : DbContext
    {

        public DbSet<Audit> Audits { get; set; }
        public async Task<int> SaveChangesAsync(string username)
        {
            var auditEntries = OnBeforeSaveChanges(username);
            var saveResult = await base.SaveChangesAsync();
            if (auditEntries != null || auditEntries.Count > 0)
            {
                await OnAfterSaveChanges(auditEntries);
            }
            return saveResult;
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }

                }
                Audits.Add(auditEntry.ToAudit());
            }
            return SaveChangesAsync();
        }

            private List<AuditEntry> OnBeforeSaveChanges(string username)
            {
                var entries = ChangeTracker.Entries().Where(q => q.State == EntityState.Added || q.State == EntityState.Modified || q.State== EntityState.Deleted);

                var auditEntries = new List<AuditEntry>();

                foreach (var entry in entries)
                {
                    var auditableObject = (BaseDomainObjects)entry.Entity;
                    auditableObject.ModifiedDate = DateTime.Now;
                    auditableObject.ModifiedBy = username;

                    if (entry.State == EntityState.Added)
                    {
                        auditableObject.CreateDate = DateTime.Now;
                        auditableObject.CreatedBy = username;
                    }

                    var auditEntry = new AuditEntry(entry);

                    auditEntry.TableName = entry.Metadata.GetTableName();
                    auditEntry.Action = entry.State.ToString();



                    foreach (var property in entry.Properties)
                    {
                        if (property.IsTemporary)
                        {
                            auditEntry.TemporaryProperties.Add(property);
                            continue;
                        }

                        string propertyName = property.Metadata.Name;
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[propertyName] = property.CurrentValue;
                            continue;
                        }

                        switch (entry.State)
                        {
                            case EntityState.Added:
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                break;

                            case EntityState.Deleted:
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                break;

                            case EntityState.Modified:
                                if (property.IsModified)
                                {
                                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                                }
                                break;

                        }
                        auditEntries.Add(auditEntry);
                    }


                }

                foreach (var pendidAuditEntry in auditEntries.Where(q => q.HasTemporaryProperties == false))
                {
                    Audits.Add(pendidAuditEntry.ToAudit());


                }
                return auditEntries.Where(q => q.HasTemporaryProperties).ToList();


            }

     }
}

