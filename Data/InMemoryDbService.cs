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
    public class InMemoryDbService
    {
        private readonly InMemoryDb _context;

        public InMemoryDbService(InMemoryDb context)
        {
            if (!context.WODs.Any())
            {
                // Seed in-memory db from csv
                try
                {
                    string weightliftingPath = Path.GetFullPath("Seed\\Weightlifting.csv");
                    string metconPath = Path.GetFullPath("Seed\\CFProgramming.csv");

                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine(weightliftingPath);
                    Console.WriteLine(metconPath);
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
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

                        // Only keep distinct lifts
                        List<dynamic> distinceLifts = lifts
                          .GroupBy(l => l.Component)
                          .Select(l => l.First())
                          .ToList();

                        // Add lifts to db
                        for (int i = 1; i < distinceLifts.Count; i++)
                        {
                            context.Lifts.Add(new Lift()
                            {
                                Id = i,
                                Name = distinceLifts[i].Component
                            });
                        }

                        // Remove bad data of entire metcons of 2 min of DU's
                        metcons.RemoveAll(m => m.Name == "2min Double-Unders" || m.Name == "Max Double-Unders");

                        for (int i = 1; i < metcons.Count; i++)
                        {
                            // Get a list of all the pregame components for the day of this metcon
                            var pregameItems = pregames.Where(p => p.Date == metcons[i].Date).Select(p => p.Component).ToList();

                            context.Pregames.Add(new Pregame()
                            {
                                Id = i,
                                // If pregame records have the same date, aggregate their components together separated by line breaks
                                Description = pregameItems.Count > 0 ? pregameItems.Aggregate((current, next) => current + "\n" + next).ToString() : ""
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
                catch (Exception exception)
                {
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine(exception.Message.ToString());
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    Console.WriteLine("===================================================");
                    throw;
                    throw;
                }
                
            }
            _context = context;
        }

        public async Task<WOD> GetWOD(int id)
        {
            return await _context.WODs.FindAsync(id);
        }

        public async Task<WOD> GetCurrentWOD()
        {
            return await _context.WODs.OrderByDescending(w => w.Id).FirstOrDefaultAsync(w => !w.Completed);
        }

        public async Task<List<WOD>> GetAllWODs()
        {
            return await _context.WODs.OrderByDescending(w => w.Id).ToListAsync();
        }

        public async Task<Pregame> GetPregame(int id)
        {
            return await _context.Pregames.FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Metcon> GetMetcon(int id)
        {
            return await _context.Metcons.FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}
