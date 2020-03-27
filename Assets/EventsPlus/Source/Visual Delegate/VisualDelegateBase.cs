using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace VisualEvent
{
    public abstract class VisualDelegateBase
    {

        /// <summary>List of raw <see cref="RawCall"/> objects that this Publisher invokes using predefined arguments</summary>
        [SerializeField]
        protected List<RawCall> m_calls;
        protected MonoBehaviour Yield_target;
        public bool hasyield { get; private set; }
        /// <summary>
        ///  flag for if this delgate has had its called generated
        /// </summary>
        [SerializeField]
        protected bool isinitialized;
        private int pre_initcalls;

        protected abstract void AppendCallToInvocation(RawCall raw_call);
        protected abstract void RemoveCallFromInvocation(RawCall raw_call);
        private protected abstract void InitializeYieldList(int initialYieldIndex);
        internal abstract Delegate CreateTypeSafeAction(Action action);
        internal virtual  Delegate CreateTypeSafeCoroutine(Func<IEnumerator> routine)
        {
            return null;
        }
        internal abstract Delegate CreateYieldableCall(Action action);
        public abstract Type[] types { get; }

        protected abstract Delegate oninvoke { get; set; }

        public void initialize()
        {
            if (m_calls != null)
            { 
                int tempListLength = m_calls.Count;
                for (int i = 0; i < tempListLength; ++i)
                {
                    var currentdelegate = m_calls[i];
                    Debug.Log(currentdelegate.isYieldable);
                    if (currentdelegate.isYieldable && !hasyield)
                    {
                        hasyield = true;
                        Yield_target = currentdelegate.target as MonoBehaviour;
                        InitializeYieldList(i);
                    }
                    if (Application.isEditor)
                    {
                        EditorIntialize(currentdelegate);
                    }
                    else
                    {
                        currentdelegate.initialize(this);
                        AppendCallToInvocation(currentdelegate);
                    }
                }
            }
            isinitialized = true;
        }
        public virtual void ReInitialize()
        {
            oninvoke = null;
            hasyield = false;
            initialize();
        }


        /// <summary>Attempts to remove a <see cref="RawCall"/> from the Publisher's internal array and event(s)</summary>
        /// <param name="tCall">RawCall to remove</param>
        /// <returns>True if successful</returns>
        protected internal bool removeCall(RawCall tCall)
        {
            return tCall != null && m_calls != null && removeCall(m_calls.IndexOf(tCall));
        }

        /// <summary>Attempts to remove a <see cref="RawCall"/> from the Publisher's internal array</summary>
        /// <param name="tIndex">Index of <see cref="m_calls"/> array to remove</param>
        /// <returns>True if successful</returns>
        public bool removeCall(int tIndex)
        {
            if (tIndex >= 0 && m_calls != null && tIndex < m_calls.Count)
            {
                RawCall tempCall = m_calls[tIndex];

                m_calls.RemoveAt(tIndex);
                if (m_calls.Count == 0)
                {
                    m_calls = null;
                }

                RemoveCallFromInvocation(tempCall);

                return true;
            }

            return false;
        }

        protected IEnumerator BreakYield()
        {
            yield break;
        }
        protected IEnumerator CreateDelegeateCoroutine(Func<IEnumerator> routine)
        {
            yield return routine();
        }
        private void EditorIntialize(RawCall raw_delegate)
        {
            //this block of code is simply editor sugar. this only exist because in editor time because
            ///<see cref="RawDelegate.delegateInstance"/> is not seralizable it will become null on changes to <see cref="m_calls"/>
            /// so we add this block of code here so that we can regenerate runtime delegates if user changes list and delgate instnace becomes null
            if (raw_delegate.m_runtime)
            {
                //this continue skip here is important because if we add runtime delgates before this method is called
                //we run the risk of adding the runtime delegate twice. So in this case we skip
                if (isinitialized)
                {
                    raw_delegate.initialize();
                    AppendCallToInvocation(raw_delegate);
                }
            }
            else
            {
                //if its not a runtime delegate then we need a reference to the visual delegate
                raw_delegate.initialize(this);
                AppendCallToInvocation(raw_delegate); //regardless if the delegate was created in editor or runtime we add it the invocation the same
            }
            if (!Application.isEditor)
                throw new Exception("should not execute this method in builds!");
        }

        protected void UpdateEditorCallList(Delegate runtimeDelegate)
        {
            if (Application.isEditor)
            {
                //this block of code only exist so that the list view in editor reflects the invocation order of the delgate itself
                //we cannot rely on editor proprty drawers to achieve this function because that assumes that the user has this delegate selected
                //in editor
                var new_call = new RawCall(true);
                new_call.delegateInstance = runtimeDelegate;
                // we only care about adding to the list 
                if (!isinitialized)
                {
                    /// in the case that this delegate has had calls added to in the editor
                    /// if the user decides to add runtime methods before calling <see cref="initialize"/>
                    /// we have to make sure to add the runtime delegates to the front of the list 
                    m_calls.Insert(pre_initcalls, new_call); // starts at 0
                    pre_initcalls++;
                }
                else m_calls.Add(new_call);
            }
        }
        protected void RemoveEditorCallList(Delegate runttimeDelegate)
        {
            if (Application.isEditor)
            {
                var removed_call = m_calls.FirstOrDefault(rc => rc.delegateInstance == runttimeDelegate);
                if (removed_call != null)
                    m_calls.RemoveAt(m_calls.IndexOf(removed_call));
            }
        }
    }

}
