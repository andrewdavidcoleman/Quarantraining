using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace Quarantraining.Data
{
    public class InMemoryDb : DbContext
    {
        public InMemoryDb()
        {
        }

        public InMemoryDb(DbContextOptions<InMemoryDb> options) : base(options) { }

        public DbSet<WOD> WODs { get; set; }
        public DbSet<Pregame> Pregames { get; set; }
        public DbSet<Metcon> Metcons { get; set; }
        public DbSet<Lift> Lifts { get; set; }
    }

    public class WOD
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int PregameId { get; set; }
        public int MetconId { get; set; }
        public bool Completed { get; set; } = false;
        public string Notes { get; set; }
    }

    public class Pregame
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class Metcon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Lift
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class LiftPerformance
    {
        public int Id { get; set; }
        public int LiftId { get; set; }
        public DateTime Date { get; set; }
    }

    public class MetconPerformance
    {
        public int Id { get; set; }
        public int MetconId { get; set; }
        public DateTime Date { get; set; }
    }
}

