using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTask
{
    [Table("players")]
    public class FootballPlayer
    {
        [Key, Column("id")]
        public long Id { get; set; }
        [Required, Column("name")]
        public string Name { get; set; } = "";
        [Required, Column("age")]
        public int Age { get; set; }
        public ISet<FootbalContract> Contracts { get; set; } = new HashSet<FootbalContract>();
    }

    [Table("teams")]
    public class FootballTeam
    {
        [Key, Column("id")]
        public long Id { get; set; }
        [Required, Column("name")]
        public string Name { get; set; } = "";
        public ISet<FootbalContract> Contracts { get; set; } = new HashSet<FootbalContract>();
    }

    [Table("contracts")]
    public class FootbalContract
    {
        [Key, Column("id")]
        public long Id { get; set; }
        [Required, Column("player_id")]
        public long PlayerId { get; set; }
        public FootballPlayer Player { get; set; } = new();
        [Required, Column("team_id")]
        public long TeamId { get; set; }
        public FootballTeam Team { get; set; } = new();
        [Required, Column("salary")]
        public decimal Salary { get; set; }
    }
}
