namespace VRDB.Models
{
    public class WorkerArgs
    {
        public string FileName { get; set; }
        public int CompareLevel { get; set; }
        public double MaxProgressValue { get; set; }
        public ImportStatus ImportResult { get; set; }
    }
}
