using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Models
{
    public class FixtureData
    {
        public int FixtureId { get; set; }
        public string Name { get; set; } = "";
        public List<PlayerInfo> Players { get; set; } = new();
    }
}
