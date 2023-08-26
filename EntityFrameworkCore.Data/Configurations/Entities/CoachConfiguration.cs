using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Data.Configurations.Entities
{
    internal class CoachConfiguration : IEntityTypeConfiguration<Coach>
    {
        public void Configure(EntityTypeBuilder<Coach> builder)
        {

            //builder.Property(p => p.Name).HasMaxLength(50);
            builder.HasIndex(h => new { h.Name, h.TeamId }).IsUnique();
            builder.HasData(
                new Coach
                {
                    Id = 20,
                    Name = "Trevoir Williams"
                },
                new Coach
                {
                    Id = 21,
                    Name = "Adam"
                },
                new Coach
                {
                    Id = 22,
                    Name = "Matt"
                });
        }
    }
}
