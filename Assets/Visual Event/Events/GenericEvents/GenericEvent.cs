using System;
using UnityEngine;
namespace VisualDelegates.Events
{
    public abstract class GenericEvent<arg1> : BaseEvent
    {
        [SerializeField] internal arg1 argument1;
        public virtual void Invoke(arg1 arg1, UnityEngine.Object sender)
        {
            isinvoke = true;
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                        (AllResponses[i][j].response as VisualDelegate<arg1>).Invoke(arg1);
                }
            }
            UpdateEventHistory(sender, arg1);
        }
        public void Subscribe(int priority,Action<arg1> response)
        {
            var newdelegate = new VisualDelegate<arg1>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse
            {
                response = newdelegate,
                priority = priority,
                senderID = -1
            };
            Subscribe(eventresponse);
        }
        private protected sealed override void Clear() => argument1 = default;
        private protected sealed override void EditorInvoke() => Invoke(argument1, null);
    }

    public abstract class GenericEvent<Arg1, Arg2> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, UnityEngine.Object sender)
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
            UpdateEventHistory(sender, arg1, arg2);
        }

        private protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
        }
        private protected sealed override void EditorInvoke() => Invoke(argument1, argument2, null);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, UnityEngine.Object sender)
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

        private protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
        }
        private protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, null);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, UnityEngine.Object sender)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                    {
                        (AllResponses[i][j].response as VisualDelegate<Arg1, Arg2, Arg3, Arg4>).Invoke(arg1, arg2, arg3, arg4);
                    }
                }
            }
        }

        private protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
        }
        private protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, null);
    }

    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, UnityEngine.Object sender)
        {
            var priorites = AllResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = AllResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (AllResponses[i][j].IsActive)
                        (AllResponses[i][j].response as VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5>).Invoke(arg1, arg2, arg3, arg4, arg5);
                }
            }
        }

        private protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
            argument5 = default;
        }
        private protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, null);
    }

}
