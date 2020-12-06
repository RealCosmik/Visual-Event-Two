using System;
using UnityEngine;
namespace VisualEvents
{
    public abstract class GenericEvent<Arg1> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        public virtual void Invoke(Arg1 arg1, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));
             
            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {
                            eventresponse.Invoke(arg1);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: arg1);
        }
        public RuntimeResponse Subscribe(Action<Arg1> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            eventresponse.SetPriority(priority);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear()
        {
            argument1 = default;
        }
        protected override void EditorInvoke() => Invoke(argument1, this);
    }

    public abstract class GenericEvent<Arg1, Arg2> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        public void Invoke(Arg1 arg1, Arg2 arg2, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));

            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {
                            eventresponse.Invoke(arg1, arg2);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: new object[] { arg1, arg2 });

        }
        public RuntimeResponse Subscribe(Action<Arg1, Arg2> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            eventresponse.SetPriority(in priority);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, this);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));

            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {
                            eventresponse.Invoke(arg1, arg2, arg3);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: new object[] { arg1, arg2, arg3 });

        }
        public RuntimeResponse Subscribe(Action<Arg1, Arg2, Arg3> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority,subscriber);
            eventresponse.SetPriority(in priority);
            Subscribe(eventresponse);
            return eventresponse;
        }
        protected sealed override void Clear()
        {
            argument1 = default;
            argument2 = default;
            argument3 = default;
        }
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, this);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));

            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {
                            eventresponse.Invoke(arg1, arg2, arg3, arg4);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: new object[] { arg1, arg2, arg3, arg4 });
        }
        public RuntimeResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            eventresponse.SetPriority(in priority);
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
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, this);
    }

    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));

            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: new object[] { arg1, arg2, arg3, arg4, arg5 });
        }
        public RuntimeResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5> response, int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            eventresponse.SetPriority(in priority);
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
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, this);
    }

    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        [SerializeField] internal Arg6 argument6;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));

            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        }
        public RuntimeResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            eventresponse.SetPriority(in priority);
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
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, argument6, this);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        [SerializeField] internal Arg6 argument6;
        [SerializeField] internal Arg7 argument7;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, UnityEngine.Object sender)
        {
            bool haserror = false;
            isinvoke = true;
            if (ReferenceEquals(sender, null))
                throw new System.ArgumentNullException(nameof(sender));

            var priorites = m_EventResponses.Count;
            for (int i = 0; i < priorites; i++)
            {
                var responsecount = m_EventResponses[i].Count;
                for (int j = 0; j < responsecount; j++)
                {
                    if (m_EventResponses[i][j].isActive)
                    {
                        var eventresponse = m_EventResponses[i][j];
                        try
                        {

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in {name}\n {ex}", sender);
                            haserror = true;
                        }
                    }
                }
            }
            if (debugHistory)
                UpdateEventHistory(sender: sender, error: in haserror, args: new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        }

        public RuntimeResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority, subscriber);
            eventresponse.SetPriority(in priority);
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
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, argument6, argument7, this);
    }
    public abstract class GenericEvent<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8> : BaseEvent
    {
        [SerializeField] internal Arg1 argument1;
        [SerializeField] internal Arg2 argument2;
        [SerializeField] internal Arg3 argument3;
        [SerializeField] internal Arg4 argument4;
        [SerializeField] internal Arg5 argument5;
        [SerializeField] internal Arg6 argument6;
        [SerializeField] internal Arg7 argument7;
        [SerializeField] internal Arg8 argument8;
        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4, Arg5 arg5, Arg6 arg6, Arg7 arg7, Arg8 arg8, UnityEngine.Object sender)
        {
            isinvoke = true;
            try
            {
                if (ReferenceEquals(sender, null))
                    throw new System.NullReferenceException();
                var priorites = m_EventResponses.Count;
                for (int i = 0; i < priorites; i++)
                {
                    var responsecount = m_EventResponses[i].Count;
                    for (int j = 0; j < responsecount; j++)
                    {

                    }
                }
                UpdateEventHistory(sender, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, this);
                UpdateEventHistory(sender, true, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
        }
        public RuntimeResponse Subscribe(Action<Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, Arg8> response, in int priority, UnityEngine.Object subscriber)
        {
            if (response == null)
                throw new System.ArgumentNullException(nameof(response));
            var eventresponse = new RuntimeResponse(response, priority,subscriber);
            eventresponse.SetPriority(in priority);
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
        protected sealed override void EditorInvoke() => Invoke(argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, this);
    }
}
