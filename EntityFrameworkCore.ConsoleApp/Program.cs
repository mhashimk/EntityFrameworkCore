using EntityFrameworkCore.Data;
using EntityFrameworkCore.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ConsoleApp
{
    class Program
    {
        private static FootballLeagueDbContext context = new FootballLeagueDbContext();
        static async Task Main(string[] args)
        {
            //var league = new League { Name = "Serie A" };
            //await context.Leagues.AddAsync(league);
            //await context.SaveChangesAsync();

            //await AddTeamsWithLeague(league);
            //await context.SaveChangesAsync();


            //await AddNewLeagueWithTeams();
            //await AddNewCoach();
            //await QueryFilters();
            //await AddNewLeagueWithTeams();
            //await QueryView();
            // AsNoTracking()
            await TeamsHistoryTemporalQueries();
            Console.WriteLine("Press Any Key To End ...");
            Console.ReadKey();

        }

        static async Task QueryFilters()
        {
            var leagues = await context.Leagues.Where(q => q.Name == "Serie A").ToListAsync();
            foreach (var league in leagues)
            {
                Console.WriteLine($"{league.Id} - {league.Name}");
            }

            leagues = await context.Leagues.Where(q => EF.Functions.Like(q.Name, "%A%")).ToListAsync();
            foreach (var league in leagues)
            {
                Console.WriteLine($"{league.Id} - {league.Name}");
            }

        }

        static async Task AdditionalExecutionMethods()
        {
            var leagues = context.Leagues;

            var count = await leagues.CountAsync();
            var min = await leagues.MinAsync();

            var league = await leagues.FindAsync(1);
        }

        static async Task AddTeamsWithLeague(League league)
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "Juventus",
                    LeagueId= league.Id
                },
                new Team
                {
                    Name = "AC Milan",
                    LeagueId= league.Id
                },
                new Team
                {
                    Name = "AS Roma",
                    LeagueId = league.Id
                },

            };

            await context.AddRangeAsync(teams);
        }


        static async Task UpdateRecord()
        {

            // retreive record

            var league = await context.Leagues.FindAsync(13);
            league.Name = "Scottish Leagu";

            await context.SaveChangesAsync("Test Team Management User");



        }

        static async Task AddNewLeague()
        {
           var transaction = context.Database.BeginTransaction();

            try
            {   
                var league = new League { Name = "Audit Testing League" };
                await context.Leagues.AddAsync(league);
                await context.SaveChangesAsync("Tesst Audit Create User");

                await transaction.CreateSavepointAsync("SavedLeague");

                await AddTeamsWithLeague(league);
                await context.SaveChangesAsync();

                transaction.Commit();
            }
            catch(Exception ex)
            {
                await transaction.RollbackToSavepointAsync("SavedLeague");
                Console.WriteLine(ex.Message);
            }
        }


        static async Task AddNewLeagueWithTeams()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    Name = "Rivoli United"
                },
                new Team
                {
                    Name = "Waterhouse FC"
                }
            };

            var league = new League { Name = "CIFAB", Teams = teams };
            await context.AddAsync(league);
            await context.SaveChangesAsync();

        }


        private static async Task AddNewCoach()
        {
            var coach1 = new Coach { Name = "Jose Matt" };
            await context.AddAsync(coach1);

            await context.SaveChangesAsync("Test Team Management User");


        }

        static async Task QueryRelatedRecords()
        {

            // get many related records - Leagues -> Teams

            //var leagues = await context.Leagues.Include(q => q.Teams).ToListAsync();

            //// get one related record - Team -> Coach

            //var team = await context.Teams.Include(q => q.Coach).FirstOrDefaultAsync(q => q.Id == 9);

            // get 'grand children' related record - Team -> Matches -> Home/Away Team

            var teamsWithMatchesAndOpponents = await context.Teams.Include(q => q.AwayMatches)
                                                                  .ThenInclude(q => q.HomeTeam).ThenInclude(q => q.Coach)
                                                                  .Include(q => q.HomeMatches)
                                                                  .ThenInclude(q => q.AwayTeam).ThenInclude(q => q.Coach)
                                                                  .FirstOrDefaultAsync(q => q.Id == 10);



        }

        static async Task SelectOneProperty()
        {
            var teams = await context.Teams.Select(q => q.Name).ToListAsync();
        }

        static async Task AnonymouseProjection()
        {
            var teams = await context.Teams.Include(q => q.Coach)
                                           .Select(q => new { TeamName = q.Name, CoachName = q.Coach.Name })
                                           .ToListAsync();

            foreach (var item in teams)
            {
                Console.WriteLine($"Team: {item.TeamName} | Coach : {item.CoachName}");
            }
        }

        static async Task filtringWithRelatedData()
        {

            var leagues = await context.Leagues.Where(q => q.Teams.Any(x => x.Name.Contains("Bay"))).ToListAsync();
        }

        async static Task QueryView()
        {
            var details = await context.TeamsCoachesLeagues.ToListAsync();
        }

        async static Task RawSQLQuery()
        {
            var name = "AS Roma";
            var teams1 = await context.Teams.FromSqlRaw("select * from Teams").Include(q=>q.Coach).ToListAsync();  // must return all types 

            var teams2 = await context.Teams.FromSqlInterpolated($"select * from Teams where name == {name}").ToListAsync();
        }

        async static Task ExecStoredProcedure()
        {
            var teamId = 3;
            var result = await context.Coaches.FromSqlRaw("EXECT dbo.sp_GetTeamCoach {0}", teamId).ToListAsync();
        }

        async static Task ExecuteNonQueryCommand()
        {
            var teamId = 9;

            var affectedRows = await context.Database.ExecuteSqlRawAsync("exec sp_DeleteTeamById{0}", teamId);
        }


        static async Task TeamsHistoryTemporalQueries()
        {
            var teamsHistory = await context.Teams.TemporalAll().ToListAsync();

            foreach ( var item in teamsHistory)
            {
                Console.WriteLine($"Team : {item.Name}");
            }
        }

    }
}