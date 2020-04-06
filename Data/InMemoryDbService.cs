using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quarantraining.Data
{
    public class InMemoryDbService
    {
        private readonly InMemoryDb _context;

        public InMemoryDbService(InMemoryDb context)
        {
            if (!context.WODs.Any())
            {
                // Seed in-memory db from csv
                using (var pregameReader = new StreamReader("Seed\\Weightlifting.csv"))
                using (var pregameCsv = new CsvReader(pregameReader, new CultureInfo("en-US")))
                using (var metconReader = new StreamReader("Seed\\CFProgramming.csv"))
                using (var metconCsv = new CsvReader(metconReader, new CultureInfo("en-US")))
                {
                    var pregames = pregameCsv.GetRecords<dynamic>().ToList();
                    var metcons = metconCsv.GetRecords<dynamic>().ToList();


                    for (int i = 1; i < metcons.Count; i++)
                    {
                        var pregame = pregames.FirstOrDefault(p => p.Date == metcons[i].Date) ?? new { Id = i, Component = "" };

                        context.Pregames.Add(new Pregame()
                        {
                            Id = i,
                            Component = pregame.Component
                        });

                        context.Metcons.Add(new Metcon()
                        {
                            Id = i,
                            Name = metcons[i].Name,
                            Description = metcons[i].Description
                        });

                        context.WODs.Add(new WOD()
                        {
                            Id = i,
                            PregameId = i,
                            MetconId = i,
                            Date = Convert.ToDateTime(metcons[i].Date.ToString()),
                            Completed = false
                        });
                    }

                    context.SaveChanges();
                }
            }
            _context = context;
        }

        public WOD GetWOD(int id)
        {
            return _context.WODs.FirstOrDefault(w => w.Id == id);
        }

        public WOD GetCurrentWOD()
        {
            return _context.WODs.OrderByDescending(w => w.Id).FirstOrDefault(w => !w.Completed);
        }

        public List<WOD> GetAllWODs()
        {
            return _context.WODs.OrderByDescending(w => w.Id).ToList();
        }

        public Pregame GetPregame(int id)
        {
            return _context.Pregames.FirstOrDefault(w => w.Id == id);
        }

        public Metcon GetMetcon(int id)
        {
            return _context.Metcons.FirstOrDefault(w => w.Id == id);
        }
    }
}
