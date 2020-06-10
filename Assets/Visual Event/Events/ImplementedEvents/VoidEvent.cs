using UnityEngine;
namespace VisualDelegates.Events
{
    [CreateAssetMenu(fileName = "newevent", menuName = "cusevent/base")]
    public class VoidEvent : BaseEvent
    {
        public void Invoke()
        {
            int priority = m_EventResponses.Count;
            for (int i = 0; i < priority; i++)
            {
                int response_count = m_EventResponses[i].Count;
                for (int j = 0; j < response_count; j++)
                {
                    (m_EventResponses[i][j].response as VisualDelegate).Invoke();
                }
            }
        }
        private protected override void Clear() { }
        private protected override void EditorInvoke() => Invoke();
    }
}
