namespace VisualDelegates.Events
{
    public class HistoryEntry
    {
        public int SenderID;
        public object[] entryData;
        public HistoryEntry(int id, object[] entry)
        {
            SenderID = id;
            entryData = entry;
        }
    }
}
