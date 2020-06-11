using UnityEngine;
namespace VisualDelegates.Events
{
    public class VoidEvent : BaseEvent
    {
        public void Invoke(Object sender)
        {
            try
            {
                int priority = m_EventResponses.Count;
                for (int i = 0; i < priority; i++)
                {
                    int response_count = m_EventResponses[i].Count;
                    for (int j = 0; j < response_count; j++)
                    {
                        (m_EventResponses[i][j].CurrentResponse as VisualDelegate).Invoke();
                    }
                }
                UpdateEventHistory(sender, false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true);
            }

        }
        protected override void Clear() { }
        protected override void EditorInvoke() => Invoke(null);
    }
}
