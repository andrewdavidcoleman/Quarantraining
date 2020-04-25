using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Quarantraining.Data
{
    public class QuarantrainingDb : DbContext
    {
        public QuarantrainingDb(DbContextOptions<QuarantrainingDb> options) : base(options) { }

        public DbSet<WOD> WODs { get; set; }
        public DbSet<Pregame> Pregames { get; set; }
        public DbSet<Metcon> Metcons { get; set; }
        public DbSet<Lift> Lifts { get; set; }
    }

    public class WOD
    {
        [Key]
        public int WODId { get; set; }
        public DateTime Date { get; set; }
        public int PregameId { get; set; }
        public int MetconId { get; set; }
        public bool Completed { get; set; } = false;
        public string Notes { get; set; }
    }

    public class Pregame
    {
        public int PregameId { get; set; }
        public string Description { get; set; }
    }

    public class Metcon
    {
        public int MetconId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Lift
    {
        public int LiftId { get; set; }
        public string Name { get; set; }
    }

    public class LiftPerformance
    {
        public int LiftPerformanceId { get; set; }
        public int LiftId { get; set; }
        public DateTime Date { get; set; }
    }

    public class MetconPerformance
    {
        public int MetconPerformanceId { get; set; }
        public int MetconId { get; set; }
        public DateTime Date { get; set; }
    }
}

