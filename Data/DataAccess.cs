using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quarantraining.Data
{
    public class DataAccess
    {
        private readonly QuarantrainingDb _context;

        public DataAccess(QuarantrainingDb context)
        {
            _context = context;

            if (!_context.WODs.Any())
            {
                // Seed in-memory db from csv
                try
                {
                    string weightliftingPath = Path.GetFullPath("wwwroot/Seed/Weightlifting.csv");
                    string metconPath = Path.GetFullPath("wwwroot/Seed/CFProgramming.csv");
                    using (var pregameReader = new StreamReader(weightliftingPath))
                    using (var pregameCsv = new CsvReader(pregameReader, new CultureInfo("en-US")))
                    using (var metconReader = new StreamReader(metconPath))
                    using (var metconCsv = new CsvReader(metconReader, new CultureInfo("en-US")))
                    {
                        var lifts = pregameCsv.GetRecords<dynamic>().ToList();
                        var metcons = metconCsv.GetRecords<dynamic>().ToList();
                        var pregames = new List<dynamic>(lifts);

                        // If a metcon is listed as max DU's, move it to the pregame for that date, and remove from metcons
                        var doubleUnderMetcons = metcons.Where(m => m.Name == "Max Double-Unders" || m.Name == "2min Double-Unders");
                        foreach (var doubleUnderMetcon in doubleUnderMetcons)
                        {
                            // Get the first pregame that has the same date as this doubleUnderMetcon
                            // Add "2 min DU's..." to the beginning of the pregame
                            var pregameToAddDUTo = pregames.FirstOrDefault(p => p.Date == doubleUnderMetcon.Date);
                            if (pregameToAddDUTo != null)
                            {
                                pregames.FirstOrDefault(p => p.Date == doubleUnderMetcon.Date).Component = pregameToAddDUTo.Component.Insert(0, "2min Double-Unders:\nGet as many double-unders as possible in two minutes, record your largest unbroken set.\n\n");
                            }
                        }

                        // Remove bad data of entire metcons of 2 min of DU's
                        metcons.RemoveAll(m => m.Name == "2min Double-Unders" || m.Name == "Max Double-Unders");

                        for (int i = 1; i < metcons.Count; i++)
                        {
                            // Get a list of all the pregame components for the day of this metcon
                            var pregameItems = pregames.Where(p => p.Date == metcons[i].Date).Select(p => p.Component).ToList();

                            _context.Pregames.Add(new Pregame()
                            {
                                PregameId = i,
                                // If pregame records have the same date, aggregate their components together separated by line breaks
                                Description = pregameItems.Count > 0 ? pregameItems.Aggregate((current, next) => current + "\n" + next).ToString() : ""
                            });

                            _context.Metcons.Add(new Metcon()
                            {
                                MetconId = i,
                                Name = metcons[i].Name,
                                Description = metcons[i].Description
                            });

                            _context.WODs.Add(new WOD()
                            {
                                WODId = i,
                                PregameId = i,
                                MetconId = i,
                                Date = Convert.ToDateTime(metcons[i].Date.ToString()),
                                Completed = false
                            });
                        }

                        _context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                
            }
        }

        public WOD GetWOD(int id)
        {
            return _context.WODs.Find(id);
        }

        public WOD GetCurrentWOD()
        {
            return _context.WODs.OrderByDescending(w => w.WODId).FirstOrDefault(w => !w.Completed);
        }

        public List<WOD> GetAllWODs()
        {
            return _context.WODs.OrderByDescending(w => w.WODId).ToList();
        }

        public void UpdateWOD(WOD wod)
        {
            _context.WODs.Update(wod);
            _context.SaveChanges();
        }

        public Pregame GetPregame(int id)
        {
            var pregame = _context.Pregames.FirstOrDefault(w => w.PregameId == id);
            return pregame;
        }

        public Metcon GetMetcon(int id)
        {
            var metcon = _context.Metcons.FirstOrDefault(w => w.MetconId == id);
            return metcon;
        }
    }
}
