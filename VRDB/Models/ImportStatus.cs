using Common;
using System;
using System.Collections.Generic;

namespace VRDB.Models
{
    public class ImportStatus
    {
        public string ImportFilename { get; set; }
        public long TimeSpanTicks { get; set; }
        public DateTime ImportDateTime { get; set; }
        public List<int> ImportRejects { get; set; } = new List<int>();

        public ImportStatus() { }

        public override string ToString()
        {
            string msg;
            if (!string.IsNullOrEmpty(ImportFilename))
            {
                msg = $"Loaded: {Utility.FormatWithComma(DatabaseManager.RowCount)} rows from \"{ImportFilename}\" in {Utility.TimeString(TimeSpanTicks)}\n" +
                    $"Date: {ImportDateTime.ToShortDateString()}\n";

                if (ImportRejects.Count > 0)
                {
                    msg += $"Rejected: {ImportRejects.Count}";

                    foreach (int id in ImportRejects)
                    {
                        msg += $"\nDuplicate: {id}";
                    }
                }
            }
            else
            {
                msg = $"Status: Empty";
            }

            return msg;
        }
    }
}
