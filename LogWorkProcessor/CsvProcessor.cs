using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;

namespace LogWorkProcessor
{
    class CsvProcessor
    {
        public string Process(string fileurl)
        {
            List<LogWorkEntry> entries;
            using (var csvReader = new CsvReader(new StreamReader(fileurl), false))
            {
                entries = ExtractLogWorkEntries(csvReader);
            }

            var durations = from e in entries
                            group e by e.User into user
                            select new { User = user.Key, TotalDuration = user.Sum(a => a.Duration) };

            var resultFile = fileurl.Replace(".csv", "-total_count.csv");
            using (var csvWriter = new CsvWriter(new StreamWriter(resultFile)))
            {
                csvWriter.WriteField("User");
                csvWriter.WriteField("TotalCount");
                csvWriter.NextRecord();

                foreach (var duration in durations)
                {
                    Console.WriteLine($"User={duration.User} TotalCount={duration.TotalDuration}");

                    csvWriter.WriteField<string>(duration.User);
                    csvWriter.WriteField<int>(duration.TotalDuration);
                    csvWriter.NextRecord();
                }
            }

            return resultFile;
        }

        static List<LogWorkEntry> ExtractLogWorkEntries(CsvReader csvReader)
        {
            if (!csvReader.Read())
            {
                throw new InvalidOperationException("csv file doesn't data");
            }

            List<int> indices = ExtractIndexForLogWorkColumn(csvReader);
            //Console.WriteLine($"Indices count for log work columns={indices.Count}");

            return ExtractLogWorkEntries(csvReader, indices);
        }

        static List<LogWorkEntry> ExtractLogWorkEntries(CsvReader csvReader, List<int> indices)
        {
            var logworkEntries = new List<LogWorkEntry>();
            var regex = new Regex(@";(.*);(.*);(\d*)");

            while (csvReader.Read())
            {
                foreach (int idx in indices)
                {
                    string value = csvReader.GetField(idx);
                    if (!String.IsNullOrEmpty(value.Trim()))
                    {
                        //Console.WriteLine($"Log Work raw: '{value}'");
                        //;20/Aug/18 12:42 PM;andre.alexandre;1800
                        //;(.*);(.*);(\d*)
                        var match = regex.Match(value);

                        //TODO use factory (delegate responsabilities)
                        var entry = new LogWorkEntry(
                            match.Groups[1].Value,
                            match.Groups[2].Value,
                            int.Parse(match.Groups[3].Value)
                        );

                        //Console.WriteLine($"Log Work: '{entry}'");
                        logworkEntries.Add(entry);
                    }
                }
            }

            return logworkEntries;
        }

        static List<int> ExtractIndexForLogWorkColumn(CsvReader csvReader)
        {
            var indices = new List<int>();
            for (var index = 0; true; ++index)
            {
                try
                {
                    string value = csvReader.GetField(index);

                    if (value.ToLower().Contains("log work"))
                    {
                        indices.Add(index);
                    }
                }
                catch (CsvHelper.MissingFieldException)
                {
                    break;
                }
            }

            return indices;
        }
    }

    class LogWorkEntry
    {
        public string DateTime { get; }
        public string User { get; }
        public int Duration { get; }

        public LogWorkEntry(string dateTime, string user, int duration)
        {
            DateTime = dateTime;
            User = user;
            Duration = duration;
        }

        public override string ToString() => $"[Date={DateTime} User={User} Duration={Duration}]";

        public override bool Equals(object obj) => obj is LogWorkEntry entry &&
                    DateTime == entry.DateTime &&
                    User == entry.User &&
                    Duration == entry.Duration;

        public override int GetHashCode()
        {
            var hashCode = -34653793;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DateTime);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(User);
            hashCode = hashCode * -1521134295 + Duration.GetHashCode();
            return hashCode;
        }
    }
}
