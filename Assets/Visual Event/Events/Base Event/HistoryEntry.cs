namespace VisualDelegates.Events
{
    public class HistoryEntry
    {
        public int SenderID;
        public object[] entryData;
        public string entryTrace;
        public bool haserror;
        public HistoryEntry(int id, object[] entry,string trace,bool error)
        {
            SenderID = id;
            entryData = entry;
            entryTrace = trace;
            haserror = error;
        }
    }
}
