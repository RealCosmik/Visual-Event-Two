namespace VisualEvents
{
    public class HistoryEntry
    {
        public int SenderID,frame;
        public object[] entryData;
        public string entryTrace;
        public bool haserror;
        public HistoryEntry(int id,int frame, object[] entry,string trace,bool error)
        {
            this.frame = frame;
            SenderID = id;
            entryData = entry;
            entryTrace = trace;
            haserror = error;
        }
    }
}
