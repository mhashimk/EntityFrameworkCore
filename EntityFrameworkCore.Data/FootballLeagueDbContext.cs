using EntityFrameworkCore.Data.Configurations.Entities;
using EntityFrameworkCore.Domain;
using EntityFrameworkCore.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Match = EntityFrameworkCore.Domain.Match;

namespace EntityFrameworkCore.Data
{
    public class FootballLeagueDbContext:AuditableFootballLeagueDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=FootballLeague_EfCore",
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                }).LogTo(Console
                .Write,new[] { DbLoggerCategory.Database.Command.Name },LogLevel.Information). EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            modelBuilder.Entity<TeamsCoachesLeaguesView>().HasNoKey().ToView("TeamsCoachesLeagues");
            modelBuilder.ApplyConfiguration(new LeagueConfiguration());
            modelBuilder.ApplyConfiguration(new TeamConfiguration());
            modelBuilder.ApplyConfiguration(new CoachConfiguration());


            // set all FK relationships should be restrict

            var foreignKeys = modelBuilder.Model.GetEntityTypes()
                                                .SelectMany(x => x.GetForeignKeys())
                                                .Where(x => !x.IsOwnership && x.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in foreignKeys)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Indicate which has a History Table
            modelBuilder.Entity<Team>()
                        .ToTable("Teams", b => b.IsTemporal());

            
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Pre convention model configuration goes here
            //configurationBuilder.Properties<string>().AreUnicode(false).HaveMaxLength(50);
        }




        public DbSet<Team> Teams { get; set; }
        public DbSet<League> Leagues { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<Coach> Coaches { get; set; }

        public DbSet<TeamsCoachesLeaguesView> TeamsCoachesLeagues { get; set; }
    }
}
