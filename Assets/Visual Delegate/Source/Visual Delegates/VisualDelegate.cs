using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VisualEvent
{
    //##########################
    // Class Declaration
    //##########################
    /// <summary>Manages delegate event registration and invocation</summary>
    [Serializable]
    public sealed class VisualDelegate : VisualDelegateBase
    {
        /// <summary>Event for 0-Parameter delegates and calls</summary>
        private event Action m_oninvoke;
        public event Action OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        private List<Func<IEnumerator>> YieldedDelegates;
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action; }
        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<IEnumerator>>(m_calls.Count);
        }
        public override void ReInitialize()
        {
            YieldedDelegates?.Clear();
            base.ReInitialize();
        }
        /// <summary>Handles the <see cref="RawCall"/> that was added and registers its delegate to the Publisher's matching event(s)</summary>
        /// <param name="tCall">RawCall that was added</param>
        protected override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke +=() =>
                    {
                        if (call.target == null)
                            removeCall(call);
                        else call_delegate();
                    };
                else
                    YieldedDelegates.Add(CreateYieldableDyanimcDelegate(call_delegate, call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add(corutineCall);
            }
            else Debug.LogWarning("no case found");
        }

        /// <summary>Handles the <see cref="RawCall"/> that was removed and removes its delegate from the Publisher's matching event(s)</summary>
        /// <param name="tCall">RawCall that was removed</param>
        /// <param name="tIndex">Index of the RawCall that was removed</param>
        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action;
        }
        // internal override Delegate CreateTypeSafeActioncall(Action action) => action;
        //  internal override Delegate CreateYieldableCall(Action action) => CreateYieldableDyanimcDelegate(action);
        internal Func<IEnumerator> CreateYieldableDyanimcDelegate(Action action,RawDelegate call)
        {
            Func<IEnumerator> yieldable_delegate = () =>
            {
                if (call.target == null)
                    removeCall(call);
                else action();
                return BreakYield();
            };
            return yieldable_delegate;
        }

        // Publish
        //=======================
        /// <summary>Invokes the <see cref="m_oninvoke"/> event</summary>
        public void Invoke()
        {
            if (!isinitialized)
                initialize();
            m_oninvoke?.Invoke();
            if (hasyield)
                ExecuteYieldedDelegates();
        }

        private void ExecuteYieldedDelegates()
        {
            Yield_target.StartCoroutine(RunYieldDelegates());
        }
        private IEnumerator RunYieldDelegates()
        {
            int delegate_count = YieldedDelegates?.Count ?? 0;
            for (int i = 0; i < delegate_count; i++)
            {
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i]());
                }
            }
        }

        ~VisualDelegate()
        {
            // Clear memory
            m_calls = null;
            m_oninvoke = null;
        }

    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>1-Parameter Publisher</summary>
    public class VisualDelegate<A> : VisualDelegateBase
    {

        //=======================
        // Variables
        //=======================
        /// <summary>Event for 1-Parameter delegates</summary>
        private event Action<A> m_oninvoke;
        public event Action<A> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        private List<Func<A, IEnumerator>> YieldedDelegates;

        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A>; }

        public override void ReInitialize()
        {
            YieldedDelegates?.Clear();
            base.ReInitialize();
        }

        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, IEnumerator>>(m_calls.Count);
        }

        protected sealed override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke += _ =>
                    {
                        if (call.isDelegateLeaking())
                            removeCall(call);
                        else call_delegate();
                    };
                else
                    YieldedDelegates.Add(CreateYieldableCall(call_delegate, call));
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (raw_delegate_instance is Action<A> dynamic_call)
            {
                if (!hasyield)
                    m_oninvoke += dynamic_call;
                else YieldedDelegates.Add(CreateYieldableDynamicDelegate(dynamic_call, call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add(_ => corutineCall());
            }
            // corutine thats dynamic 
            else if (raw_delegate_instance is Func<A, IEnumerator> DynamicRoutineCall)
            {
                YieldedDelegates.Add(val =>
                {
                    if (call.target == null)
                    {
                        removeCall(call);
                        return BreakYield();
                    }
                    else return DynamicRoutineCall(val);
                });
            }
            else Debug.LogWarning("no case found");
        }

        internal Func<A, IEnumerator> CreateYieldableCall(Action action, RawDelegate call)
        {
            Func<A, IEnumerator> yieldableCall = _ =>
             {
                 if (call.target == null)
                     removeCall(call);
                 else action();
                 return BreakYield();
             };
            return yieldableCall;
        }

        internal Func<A, IEnumerator> CreateYieldableDynamicDelegate(Action<A> action,RawDelegate call)
        {
            Func<A, IEnumerator> yieldable_delegate = val =>
             {
                 if (call.target == null)
                     removeCall(call);
                 else action(val);
                 return BreakYield();
             };
            return yieldable_delegate;
        }
        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A>;
            tCall.delegateInstance = null;
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1)
        {
            isinvoking = true;
            if (hasyield)
                Yield_target?.StartCoroutine(ExecuteYieldedDelegates(val1));
            else
            {
                m_oninvoke?.Invoke(val1);
                if (!Application.isEditor)
                    isinvoking = false;
            }

        }
        private IEnumerator ExecuteYieldedDelegates(A val1)
        {
            int delegate_count = YieldedDelegates.Count;
            for (int i = 0; i < delegate_count; i++)
            {
                currentIndex = i;
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i](val1));
                }
                if (!Application.isEditor)
                    isinvoking = false;
            }
        }


        ~VisualDelegate()
        {
            m_oninvoke = null;
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
        private event Action<A, B> m_oninvoke;
        public event Action<A, B> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B>; }
        private List<Func<A, B, IEnumerator>> YieldedDelegates;

        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }

        //=======================
        // Types
        //=======================

        public override void ReInitialize()
        {
            YieldedDelegates.Clear();
            base.ReInitialize();
        }

        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, IEnumerator>>(m_calls.Count);
        }

        protected override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke += (val1, val2) => call_delegate();
                else
                    YieldedDelegates.Add(CreateYieldableCall(call_delegate));
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (raw_delegate_instance is Action<A, B> dynamic_call)
            {
                if (!hasyield)
                    m_oninvoke += dynamic_call;
                else YieldedDelegates.Add(CreateYieldableDynamicDelegate(dynamic_call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add((val1, val2) => corutineCall());
            }
            // corutine thats dynamic 
            else if (raw_delegate_instance is Func<A, B, IEnumerator> DynamicRoutineCall)
            {
                YieldedDelegates.Add(DynamicRoutineCall);
            }
            else Debug.LogWarning("no case found");
        }

        private Func<A, B, IEnumerator> CreateYieldableCall(Action action)
        {
            Func<A, B, IEnumerator> yieldableCall = (_, __) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldableCall;
        }

        private Func<A, B, IEnumerator> CreateYieldableDynamicDelegate(Action<A, B> action)
        {
            Func<A, B, IEnumerator> yieldable_delegate = (val, val2) =>
             {
                 action(val, val2);
                 return BreakYield();
             };
            return yieldable_delegate;
        }
        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B>;
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2)
        {
            if (hasyield)
                Yield_target.StartCoroutine(RunYieldDelegates(val1, val2));
            else m_oninvoke?.Invoke(val1, val2);
        }
        private IEnumerator RunYieldDelegates(A val1, B val2)
        {
            int delegate_count = YieldedDelegates?.Count ?? 0;
            for (int i = 0; i < delegate_count; i++)
            {
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i](val1, val2));
                }
            }
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
        private event Action<A, B, C> m_oninvoke;
        public event Action<A, B, C> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C>; }
        private List<Func<A, B, C, IEnumerator>> YieldedDelegates;

        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }

        public override void ReInitialize()
        {
            YieldedDelegates.Clear();
            base.ReInitialize();
        }

        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, C, IEnumerator>>(m_calls.Count);
        }

        protected override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke += (val1, val2, val3) => call_delegate();
                else
                    YieldedDelegates.Add(CreateYieldableCall(call_delegate));
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (raw_delegate_instance is Action<A, B, C> dynamic_call)
            {
                if (!hasyield)
                    m_oninvoke += dynamic_call;
                else YieldedDelegates.Add(CreateYieldableDynamicDelegate(dynamic_call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add((val1, val2, val3) => corutineCall());
            }
            // corutine thats dynamic 
            else if (raw_delegate_instance is Func<A, B, C, IEnumerator> DynamicRoutineCall)
            {
                YieldedDelegates.Add(DynamicRoutineCall);
            }
            else Debug.LogWarning("no case found");
        }

        private Func<A, B, C, IEnumerator> CreateYieldableCall(Action action)
        {
            Func<A, B, C, IEnumerator> yieldableCall = (val, val2, val3) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldableCall;
        }

        private Func<A, B, C, IEnumerator> CreateYieldableDynamicDelegate(Action<A, B, C> action)
        {
            Func<A, B, C, IEnumerator> yieldable_delegate = (val, val2, val3) =>
             {
                 action(val, val2, val3);
                 return BreakYield();
             };
            return yieldable_delegate;
        }

        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C>;
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3)
        {
            if (!hasyield)
                m_oninvoke?.Invoke(val1, val2, val3);
            else
                Yield_target.StartCoroutine(RunYieldDelegates(val1, val2, val3));
        }
        private IEnumerator RunYieldDelegates(A val1, B val2, C val3)
        {
            int delegate_count = YieldedDelegates?.Count ?? 0;
            for (int i = 0; i < delegate_count; i++)
            {
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i](val1, val2, val3));
                }
            }
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
        private event Action<A, B, C, D> m_oninvoke;
        public event Action<A, B, C, D> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        private List<Func<A, B, C, D, IEnumerator>> YieldedDelegates;
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D>; }
        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }
        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, C, D, IEnumerator>>(m_calls.Count);

        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke += (val1, val2, val3, val4) => call_delegate();
                else
                    YieldedDelegates.Add(CreateYieldableCall(call_delegate));
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (raw_delegate_instance is Action<A, B, C, D> dynamic_call)
            {
                if (!hasyield)
                    m_oninvoke += dynamic_call;
                else YieldedDelegates.Add(CreateYieldableDynamicDelegate(dynamic_call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add((val1, val2, val3, val4) => corutineCall());
            }
            // corutine thats dynamic 
            else if (raw_delegate_instance is Func<A, B, C, D, IEnumerator> DynamicRoutineCall)
            {
                YieldedDelegates.Add(DynamicRoutineCall);
            }
            else Debug.LogWarning("no case found");
        }

        private Func<A, B, C, D, IEnumerator> CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, IEnumerator> yieldableCall = (val, val2, val3, val4) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldableCall;
        }

        private Func<A, B, C, D, IEnumerator> CreateYieldableDynamicDelegate(Action<A, B, C, D> action)
        {
            Func<A, B, C, D, IEnumerator> yieldable_delegate = (val, val2, val3, val4) =>
             {
                 action(val, val2, val3, val4);
                 return BreakYield();
             };
            return yieldable_delegate;
        }


        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D>;
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4)
        {
            if (!hasyield)
                m_oninvoke?.Invoke(val1, val2, val3, val4);
            else Yield_target.StartCoroutine(RunYieldDelegates(val1, val2, val3, val4));
        }
        private IEnumerator RunYieldDelegates(A val1, B val2, C val3, D val4)
        {
            int delegate_count = YieldedDelegates?.Count ?? 0;
            for (int i = 0; i < delegate_count; i++)
            {
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i](val1, val2, val3, val4));
                }
            }
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
        private event Action<A, B, C, D, E> m_oninvoke;
        public event Action<A, B, C, D, E> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        private List<Func<A, B, C, D, E, IEnumerator>> YieldedDelegates;
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D, E>; }

        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }
        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, C, D, E, IEnumerator>>(m_calls.Count);
        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke += (val1, val2, val3, val4, val5) => call_delegate();
                else
                    YieldedDelegates.Add(CreateYieldableCall(call_delegate));
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (raw_delegate_instance is Action<A, B, C, D, E> dynamic_call)
            {
                if (!hasyield)
                    m_oninvoke += dynamic_call;
                else YieldedDelegates.Add(CreateYieldableDynamicDelegate(dynamic_call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add((val1, val2, val3, val4, val5) => corutineCall());
            }
            // corutine thats dynamic 
            else if (raw_delegate_instance is Func<A, B, C, D, E, IEnumerator> DynamicRoutineCall)
            {
                YieldedDelegates.Add(DynamicRoutineCall);
            }
            else Debug.LogWarning("no case found");
        }

        private Func<A, B, C, D, E, IEnumerator> CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, E, IEnumerator> yieldableCall = (val, val2, val3, val4, val5) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldableCall;
        }

        private Func<A, B, C, D, E, IEnumerator> CreateYieldableDynamicDelegate(Action<A, B, C, D, E> action)
        {
            Func<A, B, C, D, E, IEnumerator> yieldable_delegate = (val, val2, val3, val4, val5) =>
             {
                 action(val, val2, val3, val4, val5);
                 return BreakYield();
             };
            return yieldable_delegate;
        }

        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E>;
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5)
        {
            if (!hasyield)
                m_oninvoke?.Invoke(val1, val2, val3, val4, val5);
            else Yield_target.StartCoroutine(RunYieldDelegates(val1, val2, val3, val4, val5));
        }
        private IEnumerator RunYieldDelegates(A val1, B val2, C val3, D val4, E val5)
        {
            int delegate_count = YieldedDelegates?.Count ?? 0;
            for (int i = 0; i < delegate_count; i++)
            {
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i](val1, val2, val3, val4, val5));
                }
            }
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
        private event Action<A, B, C, D, E, F> m_oninvoke;
        public event Action<A, B, C, D, E, F> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
                if (Application.isEditor)
                    RemoveEditorCallList(value);
            }
        }
        private List<Func<A, B, C, D, E, F, IEnumerator>> YieldedDelegates;
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D, E, F>; }

        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }

        private protected override void InitializeYieldList()
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, C, D, E, F, IEnumerator>>(m_calls.Count);
        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawDelegate call)
        {
            var raw_delegate_instance = call.delegateInstance;
            // here we know that the delegate is either void method or a method with pre-defined args
            if (raw_delegate_instance is Action call_delegate)
            {
                if (!hasyield)
                    m_oninvoke += (val1, val2, val3, val4, val5, val6) => call_delegate();
                else
                    YieldedDelegates.Add(CreateYieldableCall(call_delegate));
            }
            // this will only have happen if the call is labeled as dynamic because it matches the param of this delegate
            else if (raw_delegate_instance is Action<A, B, C, D, E, F> dynamic_call)
            {
                if (!hasyield)
                    m_oninvoke += dynamic_call;
                else YieldedDelegates.Add(CreateYieldableDynamicDelegate(dynamic_call));
            }
            // if the call is an  void coroutine or a corutine with pre-defined arguments
            else if (raw_delegate_instance is Func<IEnumerator> corutineCall)
            {
                YieldedDelegates.Add((val1, val2, val3, val4, val5, val6) => corutineCall());
            }
            // corutine thats dynamic 
            else if (raw_delegate_instance is Func<A, B, C, D, E, F, IEnumerator> DynamicRoutineCall)
            {
                YieldedDelegates.Add(DynamicRoutineCall);
            }
            else Debug.LogWarning("no case found");
        }

        private Func<A, B, C, D, E, F, IEnumerator> CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, E, F, IEnumerator> yieldableCall = (val, val2, val3, val4, val5, val6) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldableCall;
        }

        private Func<A, B, C, D, E, F, IEnumerator> CreateYieldableDynamicDelegate(Action<A, B, C, D, E, F> action)
        {
            Func<A, B, C, D, E, F, IEnumerator> yieldable_delegate = (val, val2, val3, val4, val5, val6) =>
             {
                 action(val, val2, val3, val4, val5, val6);
                 return BreakYield();
             };
            return yieldable_delegate;
        }


        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F>;
        }


        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5, F val6)
        {
            m_oninvoke?.Invoke(val1, val2, val3, val4, val5, val6);
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>7-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E, F, G> : VisualDelegate<A, B, C>
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 7-Parameter delegates</summary>
        private event Action<A, B, C, D, E, F, G> m_oninvoke;
        public event Action<A, B, C, D, E, F, G> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D, E, F, G>; }

        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }

        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawDelegate tCall)
        {
            if (tCall.delegateInstance is Action<A, B, C, D, E, F, G> tempDelegate)
                m_oninvoke += tempDelegate;
            else
            {
                var instance = tCall.delegateInstance as Action;
                var Corrected_delegate = new Action<A, B, C, D, E, F, G>((val1, val2, val3, val4, val5, val6, val7) => instance.Invoke());
                tCall.delegateInstance = Corrected_delegate;
                m_oninvoke += Corrected_delegate;
            }
        }

        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F, G>;
        }

        //internal override Delegate CreateTypeSafeActioncall(Action action)
        //{
        //    Action<A, B, C, D, E, F, G> safe_action = (val1, val2, val3, val4, val5, val6, val7) => action.Invoke();
        //    return safe_action;
        //}

        //internal override Delegate CreateYieldableCall(Action action)
        //{
        //    Func<A, B, C, D, E, F, G, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6, val7) =>
        //    {
        //        action();
        //        return BreakYield();
        //    };
        //    return yieldable_delegate;
        //}

        internal Delegate CreateYieldableDelegate(Action<A, B, C, D, E, F, G> action)
        {
            Func<A, B, C, D, E, F, G, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6, val7) =>
             {
                 action(val1, val2, val3, val4, val5, val6, val7);
                 return BreakYield();
             };
            return yieldable_delegate;
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5, F val6, G val7)
        {
            m_oninvoke?.Invoke(val1, val2, val3, val4, val5, val6, val7);
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>8-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E, F, G, H> : VisualDelegate<A>
    {
        //=======================
        // Variables
        //=======================
        /// <summary>Event for 8-Parameter delegates</summary>
        private event Action<A, B, C, D, E, F, G, H> m_oninvoke;
        public event Action<A, B, C, D, E, F, G, H> OnInvoke
        {
            add
            {
                m_oninvoke += value;
                if (Application.isEditor)
                    AddRuntimeDelegateEditor(value);
            }
            remove
            {
                m_oninvoke -= value;
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D, E, F, G, H>; }

        //=======================
        // Destructor
        //=======================
        ~VisualDelegate()
        {
            m_oninvoke = null;
        }


        //=======================
        // Call
        //=======================
        //protected override void AppendCallToInvocation(RawDelegate tCall)
        //{
        //    if (tCall.delegateInstance is Action<A, B, C, D, E, F, G, H> tempDelegate)
        //        m_oninvoke += tempDelegate;
        //    else
        //    {
        //        var instance = tCall.delegateInstance as Action;
        //        var Corrected_delegate = new Action<A, B, C, D, E, F, G, H>((val1, val2, val3, val4, val5, val6, val7, val8) => instance.Invoke());
        //        tCall.delegateInstance = Corrected_delegate;
        //        m_oninvoke += Corrected_delegate;
        //    }
        //}

        protected override void RemoveCallFromInvocation(RawDelegate tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F, G, H>;
        }


        //internal override Delegate CreateTypeSafeActioncall(Action action)
        //{
        //    Action<A, B, C, D, E, F, G, H> safe_action = (val1, val2, val3, val4, val5, val6, val7, Val8) => action.Invoke();
        //    return safe_action;
        //}

        //internal override Delegate CreateYieldableCall(Action action)
        //{
        //    Func<A, B, C, D, E, F, G, H, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6, val7, val8) =>
        //     {
        //         action();
        //         return BreakYield();
        //     };
        //    return yieldable_delegate;
        //}

        internal Delegate CreateYieldableDelegate(Action<A, B, C, D, E, F, G> action)
        {
            Func<A, B, C, D, E, F, G, H, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6, val7, val8) =>
             {
                 action(val1, val2, val3, val4, val5, val6, val7);
                 return BreakYield();
             };
            return yieldable_delegate;
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5, F val6, G val7, H val8)
        {
            m_oninvoke?.Invoke(val1, val2, val3, val4, val5, val6, val7, val8);
        }
    }
}
