using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Models
{
    public class PlayerInfo
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public int JerseyNumber { get; set; }
        public int TeamId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int? BirthYear { get; set; }
        public string Country { get; set; } = "";

        public override string ToString()
            => $"  #{JerseyNumber,-3} {PlayerName,-25} " +
               $"Rodjen: {BirthYear?.ToString() ?? "N/A",-6} " +
               $"Zemlja: {Country}";
    }
}
