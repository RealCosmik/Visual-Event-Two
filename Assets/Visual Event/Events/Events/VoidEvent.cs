using UnityEngine;
namespace VisualDelegates.Events
{
    [CreateAssetMenu(fileName = "newevent", menuName = "cusevent/base")]
    public class VoidEvent : BaseEvent
    {
        public void invoke()
        {
            int priority = AllResponses.Count;
            for (int i = 0; i < priority; i++)
            {
                int response_count = AllResponses[i].Count;
                for (int j = 0; j < response_count; j++)
                {
                    (AllResponses[i][j].response as VisualDelegate).Invoke();
                }
            }
        }
    }
}
