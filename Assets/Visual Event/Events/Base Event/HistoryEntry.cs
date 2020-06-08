namespace VisualDelegates.Events
{
    public class HistoryEntry
    {
        public int SenderID;
        public object[] entryData;
        public string entryTrace;
        public HistoryEntry(int id, object[] entry,string trace)
        {
            SenderID = id;
            entryData = entry;
            entryTrace = trace;
        }
    }
}
