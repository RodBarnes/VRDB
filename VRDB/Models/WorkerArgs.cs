namespace VRDB.Models
{
    public class WorkerArgs
    {
        public string FileName { get; set; }
        public int Level { get; set; }
        public double MaxProgressValue { get; set; }
        public ImportStatus ImportResult { get; set; }
    }
}
