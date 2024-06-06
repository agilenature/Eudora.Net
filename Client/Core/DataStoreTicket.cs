namespace Eudora.Net.Core
{
    public class DataStoreTicket
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string FullPath { get; set; } = string.Empty;
    }
}
