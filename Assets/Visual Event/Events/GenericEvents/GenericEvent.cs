using UnityEngine;
namespace VisualDelegates.Events
{
    public abstract class GenericEvent<arg1> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal arg1 argument1;
        public void Invoke(arg1 arg1)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if(AllResponses[i][j].IsActive)
                    (AllResponses[i][j].response as VisualDelegate<arg1>).Invoke(arg1);
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

    public abstract class GenericEvent<Arg1, Arg2> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        public void Invoke(Arg1 arg1, Arg2 arg2)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                        (AllResponses[i][j].response as VisualDelegate<Arg1, Arg2>).Invoke(arg1, arg2);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
            {
                argument1 = default;
                argument2 = default;
            }
        }
        private protected override void EditorInvoke() => Invoke(argument1, argument2);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                        (AllResponses[i][j].response as VisualDelegate<Arg1, Arg2, Arg3>).Invoke(arg1, arg2, arg3);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
            {
                argument1 = default;
                argument2 = default;
                argument3 = default;
            }
        }
        private protected override void EditorInvoke() => Invoke(argument1, argument2, argument3);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                        (AllResponses[i][j].response as VisualDelegate<Arg1, Arg2, Arg3, Arg4>).Invoke(arg1, arg2, arg3, arg4);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
            {
                argument1 = default;
                argument2 = default;
                argument3 = default;
                argument4 = default;
            }
        }
        private protected override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4);
    }

    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4,Arg5> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4,Arg5 arg5)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                        (AllResponses[i][j].response as VisualDelegate<Arg1, Arg2, Arg3, Arg4,Arg5>).Invoke(arg1, arg2, arg3, arg4,arg5);
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (!Application.isEditor)
            {
                argument1 = default;
                argument2 = default;
                argument3 = default;
                argument4 = default;
                argument5 = default;
            }
        }
        private protected override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5);
    }

}
