using System;
using UnityEngine;
namespace VisualDelegates
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Manages delegate event registration and invocation</summary>
    [Serializable]
    public class VisualDelegate : VisualDelegateBase
    {
        private Action m_onInvoke;
        /// <summary>Event for 0-Parameter delegates and calls</summary>
        public event Action OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }
        protected override Delegate m_delegate => m_onInvoke;
        /// <summary>Handles the <see cref="RawCall"/> that was added and registers its delegate to the Publisher's matching event(s)</summary>
        /// <param name="tCall">RawCall that was added</param>
        protected override void AppendCallToEvent(RawDelegate call)
        {

            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                Action leaksafe= () =>
                 {
                     if (call.isDelegateLeaking())
                     {
                         removeCall(call);
                     }
                     else call_delegate();
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }

        /// <summary>Handles the <see cref="RawCall"/> that was removed and removes its delegate from the Publisher's matching event(s)</summary>
        /// <param name="tCall">RawCall that was removed</param>
        /// <param name="tIndex">Index of the RawCall that was removed</param>
        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action;
            tCall.Release();
        }
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="m_onInvoke"/> event</summary>
        public void Invoke()
        {
            if (!isinitialized)
                throw new Exception("Delegate not initliazed!");
            InvokeInternalCall();
            m_onInvoke?.Invoke();
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>1-Parameter Publisher</summary>
    [Serializable]
    public class VisualDelegate<A> : VisualDelegateBase
    { 
        private Action<A> m_onInvoke;
        /// <summary>Event for 0-Parameter delegates and calls</summary>
        public event Action<A> OnInvoke
        { 
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }
        protected sealed override void AppendCallToEvent(RawDelegate call)
        {

            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A> leaksafe = _ =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else call_delegate();
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A> dynamic_call)
            {
                Action<A> leaksafe = val =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else dynamic_call(val);
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }


        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A>;
            tCall.Release();
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1)
        {
            if (!isinitialized)
                throw new Exception("Delegate not initliazed!");
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }


    //##########################
    // Class Declaration
    //##########################
    /// <summary>2-Parameter Publisher</summary>
    public class VisualDelegate<A, B> : VisualDelegateBase
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 2-Parameter delegates</summary>
        private Action<A, B> m_onInvoke;
        public event Action<A, B> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }

        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B> leaksafe = (val1, val2) =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else call_delegate();
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B> dynamic_call)
            {
                Action<A, B> leaksafe = (val1, val2) =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else dynamic_call(val1, val2);
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }


        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B>;
            tCall.Release();
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2)
        {
            if (!isinitialized)
                throw new Exception("Delegate not initliazed!");
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }

    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>3-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C> : VisualDelegateBase
    {

        //=======================
        // Variables
        //=======================
        /// <summary>Event for 3-Parameter delegates</summary>
        private Action<A, B, C> m_onInvoke;
        public event Action<A, B, C> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }
        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B, C> leaksafe = (val1, val2, val3) =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else call_delegate();
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B, C> dynamic_call)
            {
                Action<A, B, C> leaksafe = (val1, val2, val3) =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else dynamic_call(val1, val2, val3);
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }

        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B, C>;
            tCall.Release();
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3)
        {
            if (!isinitialized)
                throw new Exception("Delegate not initliazed!");
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2, val3);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>4-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D> : VisualDelegateBase
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 4-Parameter delegates</summary>
        private Action<A, B, C, D> m_onInvoke;
        public event Action<A, B, C, D> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B, C, D> leaksafe = (val1, val2, val3, val4) =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else call_delegate();
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B, C, D> dynamic_call)
            {
                Action<A, B, C, D> leaksafe = (val1, val2, val3, val4) =>
                 {
                     if (call.isDelegateLeaking())
                         removeCall(call);
                     else dynamic_call(val1, val2, val3, val4);
                 };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }

        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B, C, D>;
            tCall.Release();
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4)
        {
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2, val3, val4);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>5-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E> : VisualDelegateBase
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 5-Parameter delegates</summary>
        private Action<A, B, C, D, E> m_onInvoke;
        public event Action<A, B, C, D, E> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }

        //=======================
        // Call
        //=======================
        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B, C, D,E> leaksafe = (val1, val2, val3, val4,val5) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else call_delegate();
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B, C, D,E> dynamic_call)
            {
                Action<A, B, C, D,E> leaksafe = (val1, val2, val3, val4,val5) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else dynamic_call(val1, val2, val3, val4,val5);
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }
        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B, C, D, E>;
            tCall.Release();
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5)
        {
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2, val3, val4, val5);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>6-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E, F> : VisualDelegateBase
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 6-Parameter delegates</summary>
        private Action<A, B, C, D, E, F> m_onInvoke;
        public event Action<A, B, C, D, E, F> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }

        //=======================
        // Call
        //=======================
        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B, C, D, E,F> leaksafe = (val1, val2, val3, val4, val5,val6) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else call_delegate();
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B, C, D, E,F> dynamic_call)
            {
                Action<A, B, C, D, E,F> leaksafe = (val1, val2, val3, val4, val5,val6) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else dynamic_call(val1, val2, val3, val4, val5,val6);
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }


        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F>;
            tCall.Release();
        }


        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5, F val6)
        {
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2, val3, val4, val5, val6);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>7-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E, F, G> : VisualDelegateBase
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 7-Parameter delegates</summary>
        private Action<A, B, C, D, E, F, G> m_onInvoke;
        public event Action<A, B, C, D, E, F, G> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }

        //=======================
        // Call
        //=======================
        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B, C, D, E, F,G> leaksafe = (val1, val2, val3, val4, val5, val6,val7) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else call_delegate();
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B, C, D, E, F,G> dynamic_call)
            {
                Action<A, B, C, D, E, F,G> leaksafe = (val1, val2, val3, val4, val5, val6,val7) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else dynamic_call(val1, val2, val3, val4, val5, val6,val7);
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }

        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F, G>;
            tCall.Release();
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5, F val6, G val7)
        {
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2, val3, val4, val5, val6, val7);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>8-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E, F, G, H> : VisualDelegateBase
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 8-Parameter delegates</summary>
        private Action<A, B, C, D, E, F, G, H> m_onInvoke;
        public event Action<A, B, C, D, E, F, G, H> OnInvoke
        {
            add
            {
                m_onInvoke += value;
                if (Application.isEditor)
                    AddRuntimetoEditor(value);
            }
            remove
            {
                m_onInvoke -= value;
                if (Application.isEditor)
                    RemoveRuntimeFromEditor(value);
            }
        }
        //  protected override Delegate oninvoke { get => m_onInvoke; set => m_onInvoke = value as Action<A, B, C, D, E, F, G, H>; }


        protected override void AppendCallToEvent(RawDelegate call)
        {
            var delegatehandle = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (delegatehandle is Action call_delegate)
            {
                Action<A, B, C, D, E, F,G,H> leaksafe = (val1, val2, val3, val4, val5, val6,val7,val8) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else call_delegate();
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (delegatehandle is Action<A, B, C, D, E, F,G,H> dynamic_call)
            {
                Action<A, B, C, D, E, F,G,H> leaksafe = (val1, val2, val3, val4, val5, val6,val7,val8) =>
                {
                    if (call.isDelegateLeaking())
                        removeCall(call);
                    else dynamic_call(val1, val2, val3, val4, val5, val6,val7,val8);
                };
                call.delegateInstance = leaksafe;
                m_onInvoke += leaksafe;
            }
            else Debug.LogWarning("no case found");
        }

        protected override void RemoveCallFromEvent(RawDelegate tCall)
        {
            m_onInvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F, G, H>;
            tCall.Release();
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_onInvoke"/> and <see cref="m_onInvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5, F val6, G val7, H val8)
        {
            InvokeInternalCall();
            m_onInvoke?.Invoke(val1, val2, val3, val4, val5, val6, val7, val8);
        }
        public override void Release()
        {
            m_onInvoke = null;
            base.Release();
        }
    }
}
