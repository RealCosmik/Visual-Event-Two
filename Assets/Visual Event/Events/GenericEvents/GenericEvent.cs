using System;
using UnityEngine;
namespace VisualDelegates.Events
{
    public abstract class GenericEvent<Arg1> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        public virtual void Invoke(Arg1 arg1, UnityEngine.Object sender)
        {
            isinvoke = true;
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1>).Invoke(arg1);
                    }
                }
                UpdateEventHistory(sender, false, arg1);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1);
            }

        }
        public EventResponse Subscribe(Action<Arg1> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear() => argument1 = default;
        protected  override void EditorInvoke() => Invoke(argument1, null);
    }

    public abstract class GenericEvent<Arg1, Arg2> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, UnityEngine.Object sender)
        {
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2>).Invoke(arg1, arg2);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2);
            }

        }
        public EventResponse Subscribe(Action<Arg1, Arg2> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, null);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, UnityEngine.Object sender)
        {
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2, Arg3>).Invoke(arg1, arg2, arg3);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2, arg3);
            }

        }
        public EventResponse Subscribe(Action<Arg1, Arg2, Arg3> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2, Arg3>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, null);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, UnityEngine.Object sender)
        {
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2, Arg3, Arg4>).Invoke(arg1, arg2, arg3, arg4);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3, arg4);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2, arg3, arg4);
            }
        }
        public EventResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2, Arg3, Arg4>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }

        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, null);
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
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5>).Invoke(arg1, arg2, arg3, arg4, arg5);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3, arg4, arg5);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2, arg3, arg4, arg5);
            }
        }
        public EventResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }

        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
            argument5 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, null);
    }

    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        [SerializeField] internal Arg6 argument6;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, UnityEngine.Object sender)
        {
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6>).Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }
        public EventResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }

        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
            argument5 = default;
            argument6 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, argument6, null);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        [SerializeField] internal Arg6 argument6;
        [SerializeField] internal Arg7 argument7;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, UnityEngine.Object sender)
        {
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7>).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }

        public EventResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6,Arg7> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6,Arg7>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }

        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
            argument5 = default;
            argument6 = default;
            argument7 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, argument6, argument7, null);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8> : BaseEvent, ISerializationCallbackReceiver
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        [SerializeField] internal Arg6 argument6;
        [SerializeField] internal Arg7 argument7;
        [SerializeField] internal Arg8 argument8;
        public virtual void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, UnityEngine.Object sender)
        {
            try
            {
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {
                        if (m_EventResponses[i][j].isActive)
                            (m_EventResponses[i][j].CurrentResponse as VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8>).Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UpdateEventHistory(sender, true, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
        }
        public EventResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7,Arg8> response, int priority)
        {
            var newdelegate = new VisualDelegate<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7,Arg8>();
            newdelegate.OnInvoke += response;
            var eventresponse = new EventResponse(newdelegate, priority)
            {
                senderID = -1,
            };
            eventresponse.SetPrioirtiy(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }

        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
            argument4 = default;
            argument5 = default;
            argument6 = default;
            argument7 = default;
            argument8 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, null);
    }
}
