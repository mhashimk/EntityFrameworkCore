using EntityFrameworkCore.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Domain
{
    public class Team:BaseDomainObjects
    {
        
        public string Name { get; set; }
        public int LeagueId { get; set; }
        public virtual League League { get; set; }

        public virtual Coach Coach { get; set; }

        public virtual List<Match> HomeMatches { get; set; }
        public virtual List<Match> AwayMatches { get; set; }
    }
}
