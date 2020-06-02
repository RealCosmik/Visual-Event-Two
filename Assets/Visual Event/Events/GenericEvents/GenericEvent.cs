using UnityEngine;
namespace VisualDelegates.Events
{
    public abstract class GenericEvent<T> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal T argument1;
        public void Invoke(T arg1)
        {
            Debug.Log("invoke");
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    (AllResponses[i][j].response as VisualDelegate<T>).Invoke(arg1);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
                argument1 = default;
        }
        private protected override void EditorInvoke() => Invoke(argument1);
    }
}
