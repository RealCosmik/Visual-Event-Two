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
                    UpdateEditorCallList(value);
            }
            remove
            {
                m_oninvoke -= value;
            }
        }
        private List<Func<IEnumerator>> YieldedDelegates;
        /// <summary>Gets array of Types that define this instance; this is used by the inspector to manage drop-downs</summary>
        public override Type[] types => null;

        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action; }
        private protected override void InitializeYieldList(int initialYieldIndex)
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<IEnumerator>>(m_calls.Count - initialYieldIndex);

        }
        public override void ReInitialize()
        {
            YieldedDelegates?.Clear();
            base.ReInitialize();
        }
        /// <summary>Handles the <see cref="RawCall"/> that was added and registers its delegate to the Publisher's matching event(s)</summary>
        /// <param name="tCall">RawCall that was added</param>
        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (!hasyield)
            {
                var temp_action = tCall.delegateInstance as Action;
                m_oninvoke += temp_action;
            }
            else
            {
                var yielded_deldegate = tCall.delegateInstance as Func<IEnumerator>;
                YieldedDelegates.Add(yielded_deldegate);
            }
        }


        /// <summary>Handles the <see cref="RawCall"/> that was removed and removes its delegate from the Publisher's matching event(s)</summary>
        /// <param name="tCall">RawCall that was removed</param>
        /// <param name="tIndex">Index of the RawCall that was removed</param>
        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action;
        }
        internal override Delegate CreateTypeSafeAction(Action action) => action;
        internal override Delegate CreateYieldableCall(Action action) => CreateYieldableDyanimcDelegate(action);
        internal Delegate CreateYieldableDyanimcDelegate(Action action)
        {
            Func<IEnumerator> yieldable_delegate = () =>
            {
                action();
                return BreakYield();
            };
            return yieldable_delegate;
        }
      
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="m_oninvoke"/> event</summary>
        public void Invoke()
        {
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
            int delegate_count = YieldedDelegates.Count;
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
                    UpdateEditorCallList(value);
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


        public override Type[] types => new Type[] { typeof(A) };

        public override void ReInitialize()
        {
            YieldedDelegates?.Clear();
            base.ReInitialize();
        }

        private protected override void InitializeYieldList(int initialYieldIndex)
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, IEnumerator>>(m_calls.Count - initialYieldIndex);
        }

        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (!hasyield)
            {
                var temp_action = tCall.delegateInstance as Action<A>;
                m_oninvoke += temp_action;
            }
            else
            {
                var yielded_delegate = tCall.delegateInstance as Func<A, IEnumerator>;
                YieldedDelegates.Add(yielded_delegate);
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A>;
        }
        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A> safe_action = (val1) => action.Invoke();
            return safe_action;
        }
        internal override Delegate CreateTypeSafeCoroutine(Func<IEnumerator> routine)
        {
            Func<A, IEnumerator> safe_routine = val1 => CreateDelegeateCoroutine(routine);
            return safe_routine;
        }
        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, IEnumerator> yieldableCall = val =>
             {
                 action();
                 return BreakYield();
             };
            return yieldableCall;
        }
      
        internal Delegate CreateYieldableDynamicDelegate(Action<A> action)
        {
            Func<A, IEnumerator> yieldable_delegate = val =>
             {
                 action(val);
                 return BreakYield();
             };
            return yieldable_delegate;
        }

        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1)
        {
            m_oninvoke?.Invoke(val1);
            if (hasyield)
                ExecuteYieldedDelegates(val1);

        }
        private void ExecuteYieldedDelegates(A val1)
        {
            Yield_target.StartCoroutine(RunYieldDelegates(val1));
        }
        private IEnumerator RunYieldDelegates(A val1)
        {
            int delegate_count = YieldedDelegates.Count;
            for (int i = 0; i < delegate_count; i++)
            {
                if (i < YieldedDelegates.Count)
                {
                    if (Yield_target == null)
                        yield break;
                    else yield return Yield_target.StartCoroutine(YieldedDelegates[i](val1));
                }
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
                    UpdateEditorCallList(value);
            }
            remove
            {
                m_oninvoke -= value;
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
        public override Type[] types => new Type[] { typeof(A), typeof(B) };

        public override void ReInitialize()
        {
            YieldedDelegates.Clear();
            base.ReInitialize();
        }

        private protected override void InitializeYieldList(int initialYieldIndex)
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, IEnumerator>>(m_calls.Count - initialYieldIndex);
        }

        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (!hasyield)
            {
                var temp_action = tCall.delegateInstance as Action<A, B>;
                m_oninvoke += temp_action;
            }
            else
            {
                var yielded_delegate = tCall.delegateInstance as Func<A, B, IEnumerator>;
                YieldedDelegates.Add(yielded_delegate);
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B>;
        }
        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B> safe_action = (val1, val2) => action.Invoke();
            return safe_action;
        }
        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, IEnumerator> yieldable_delegate = (val1, val2) =>
            {
                action();
                return BreakYield();
            };
            return yieldable_delegate;
        }

        internal Delegate CreateYieldableDynamicDelegate(Action<A, B> action)
        {
            Func<A, B, IEnumerator> yieldable_delegate = (val1, val2) =>
            {
                action(val1, val2);
                return BreakYield();
            };
            return yieldable_delegate;
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2)
        {
            m_oninvoke?.Invoke(val1, val2);
            if (hasyield)
                ExecuteYieldedDelegates(val1, val2);
        }
        private void ExecuteYieldedDelegates(A val1,B val2)
        {
            Yield_target.StartCoroutine(RunYieldDelegates(val1, val2));
        }
        private IEnumerator RunYieldDelegates(A val1,B val2)
        {
            int delegate_count = YieldedDelegates.Count;
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
    public class VisualDelegate<A, B, C> : VisualDelegate<A,B>
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
                    UpdateEditorCallList(value);
            }
            remove
            {
                m_oninvoke -= value;
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

        //=======================
        // Types
        //=======================
        public override Type[] types
        {
            get
            {
                return new Type[] { typeof(A), typeof(B), typeof(C) };
            }
        }

        public override void ReInitialize()
        {
            YieldedDelegates.Clear();
            base.ReInitialize();
        }

        private protected override void InitializeYieldList(int initialYieldIndex)
        {
            YieldedDelegates = YieldedDelegates ?? new List<Func<A, B, C, IEnumerator>>(m_calls.Count - initialYieldIndex);
        }

        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (!hasyield)
            {
                var temp_action = tCall.delegateInstance as Action<A, B, C>;
                m_oninvoke += temp_action;
            }
            else
            {
                var yielded_delegate = tCall.delegateInstance as Func<A, B, C, IEnumerator>;
                YieldedDelegates.Add(yielded_delegate);
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C>;
        }
        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B, C> safe_action = (val1, val2, val3) => action.Invoke();
            return safe_action;
        }

        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, C, IEnumerator> yieldable_delegate = (val1, val2, val3) =>
            {
                action();
                return BreakYield();
            };
            return yieldable_delegate;
        }


        internal Delegate CreateYieldableDelegate(Action<A, B, C> action)
        {
            Func<A, B, C, IEnumerator> yieldable_delegate = (val1, val2, val3) =>
              {
                  action(val1, val2, val3);
                  return BreakYield();
              };
            return yieldable_delegate;
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3)
        {
            m_oninvoke?.Invoke(val1, val2, val3);
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>4-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D> : VisualDelegate<A,B,C>
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
                    UpdateEditorCallList(value);
            }
            remove
            {
                m_oninvoke -= value;
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D>; }
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
        public override Type[] types
        {
            get
            {
                return new Type[] { typeof(A), typeof(B), typeof(C), typeof(D) };
            }
        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (tCall.delegateInstance is Action<A, B, C, D> tempDelegate)
                m_oninvoke += tempDelegate;
            else
            {
                var instance = tCall.delegateInstance as Action;
                var Corrected_delegate = new Action<A, B, C, D>((val1, val2, val3, val4) => instance.Invoke());
                tCall.delegateInstance = Corrected_delegate;
                m_oninvoke += Corrected_delegate;
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D>;
        }

        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B, C, D> safe_action = (val1, val2, val3, val4) => action.Invoke();
            return safe_action;
        }

        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, IEnumerator> yieldable_delegate = (val1, val2, val3, val4) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldable_delegate;
        }

        internal Delegate CreateYieldableDelegate(Action<A, B, C, D> action)
        {
            Func<A, B, C, D, IEnumerator> yieldable_delegate = (val1, val2, val3, val4) =>
              {
                  action(val1, val2, val3, val4);
                  return BreakYield();
              };
            return yieldable_delegate;
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4)
        {
            m_oninvoke?.Invoke(val1, val2, val3, val4);
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>5-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E> : VisualDelegate<A>
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
                    UpdateEditorCallList(value);
            }
            remove
            {
                m_oninvoke -= value;
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D, E>; }

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
        public override Type[] types
        {
            get
            {
                return new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E) };
            }
        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (tCall.delegateInstance is Action<A, B, C, D, E> tempDelegate)
                m_oninvoke += tempDelegate;
            else
            {
                var instance = tCall.delegateInstance as Action;
                var Corrected_delegate = new Action<A, B, C, D, E>((val1, val2, val3, val4, val5) => instance.Invoke());
                tCall.delegateInstance = Corrected_delegate;
                m_oninvoke += Corrected_delegate;
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E>;
        }

        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B, C, D, E> safe_action = (val1, val2, val3, val4, val5) => action.Invoke();
            return safe_action;
        }

        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, E, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldable_delegate;
        }

        internal Delegate CreateYieldableDelegate(Action<A, B, C, D, E> action)
        {
            Func<A, B, C, D, E, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5) =>
              {
                  action(val1, val2, val3, val4, val5);
                  return BreakYield();
              };
            return yieldable_delegate;
        }
        //=======================
        // Publish
        //=======================
        /// <summary>Invokes the <see cref="VisualDelegate.m_oninvoke"/> and <see cref="m_oninvoke"/> events</summary>
        public void Invoke(A val1, B val2, C val3, D val4, E val5)
        {
            m_oninvoke?.Invoke(val1, val2, val3, val4, val5);
        }
    }

    //##########################
    // Class Declaration
    //##########################
    /// <summary>6-Parameter Publisher</summary>
    public class VisualDelegate<A, B, C, D, E, F> : VisualDelegate<A>
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
                    UpdateEditorCallList(value);
            }
            remove
            {
                m_oninvoke -= value;
            }
        }
        protected override Delegate oninvoke { get => m_oninvoke; set => m_oninvoke = value as Action<A, B, C, D, E, F>; }

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
        public override Type[] types
        {
            get
            {
                return new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F) };
            }
        }
        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (tCall.delegateInstance is Action<A, B, C, D, E, F> tempDelegate)
                m_oninvoke += tempDelegate;
            else
            {
                var instance = tCall.delegateInstance as Action;
                var Corrected_delegate = new Action<A, B, C, D, E, F>((val1, val2, val3, val4, val5, val6) => instance.Invoke());
                tCall.delegateInstance = Corrected_delegate;
                m_oninvoke += Corrected_delegate;
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F>;
        }

        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B, C, D, E, F> safe_action = (val1, val2, val3, val4, val5, val6) => action.Invoke();
            return safe_action;
        }

        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, E, F, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6) =>
              {
                  action();
                  return BreakYield();
              };
            return yieldable_delegate;
        }

        internal Delegate CreateYieldableDelegate(Action<A, B, C, D, E, F> action)
        {
            Func<A, B, C, D, E, F, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6) =>
             {
                 action(val1, val2, val3, val4, val5, val6);
                 return BreakYield();
             };
            return yieldable_delegate;
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
    public class VisualDelegate<A, B, C, D, E, F, G> : VisualDelegate<A>
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
                    UpdateEditorCallList(value);
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
        // Types
        //=======================
        public override Type[] types
        {
            get
            {
                return new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F), typeof(G) };
            }
        }

        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawCall tCall)
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

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F, G>;
        }

        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B, C, D, E, F, G> safe_action = (val1, val2, val3, val4, val5, val6, val7) => action.Invoke();
            return safe_action;
        }

        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, E, F, G, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6, val7) =>
            {
                action();
                return BreakYield();
            };
            return yieldable_delegate;
        }

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
                    UpdateEditorCallList(value);
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
        // Types
        //=======================
        public override Type[] types
        {
            get
            {
                return new Type[] { typeof(A), typeof(B), typeof(C), typeof(D), typeof(E), typeof(F), typeof(G), typeof(H) };
            }
        }

        //=======================
        // Call
        //=======================
        protected override void AppendCallToInvocation(RawCall tCall)
        {
            if (tCall.delegateInstance is Action<A, B, C, D, E, F, G, H> tempDelegate)
                m_oninvoke += tempDelegate;
            else
            {
                var instance = tCall.delegateInstance as Action;
                var Corrected_delegate = new Action<A, B, C, D, E, F, G, H>((val1, val2, val3, val4, val5, val6, val7, val8) => instance.Invoke());
                tCall.delegateInstance = Corrected_delegate;
                m_oninvoke += Corrected_delegate;
            }
        }

        protected override void RemoveCallFromInvocation(RawCall tCall)
        {
            m_oninvoke -= tCall.delegateInstance as Action<A, B, C, D, E, F, G, H>;
        }


        internal override Delegate CreateTypeSafeAction(Action action)
        {
            Action<A, B, C, D, E, F, G, H> safe_action = (val1, val2, val3, val4, val5, val6, val7, Val8) => action.Invoke();
            return safe_action;
        }

        internal override Delegate CreateYieldableCall(Action action)
        {
            Func<A, B, C, D, E, F, G, H, IEnumerator> yieldable_delegate = (val1, val2, val3, val4, val5, val6, val7, val8) =>
             {
                 action();
                 return BreakYield();
             };
            return yieldable_delegate;
        }

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
