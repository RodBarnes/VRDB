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
                msg = $"Loaded: {Utility.FormatWithComma(DatabaseManager.RowCount)} rows from {ImportFilename}\n" +
                    $"Date: {ImportDateTime.ToShortDateString()} ({TimeString()})\n";

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

        public string TimeString()
        {
            string msg;

            if (TimeSpanTicks > 0)
            {
                var span = TimeSpan.FromTicks(TimeSpanTicks);
                msg = string.Format("{0:D2}.{1:D3}s", span.Seconds, span.Milliseconds);
                if (span.Minutes > 0)
                {
                    msg = $"{string.Format("{0:D2}m", span.Minutes)} {msg}";
                }
                if (span.Hours > 0)
                {
                    msg = $"{string.Format("{0:D2}h", span.Hours)} {msg}";
                }
                //msg = string.Format("{0:D2}h {1:D2}m {2:D2}.{3:D3}s",
                //                span.Hours,
                //                span.Minutes,
                //                span.Seconds,
                //                span.Milliseconds);
            }
            else
            {
                msg = "0";
            }

            return msg;
        }
    }
}
