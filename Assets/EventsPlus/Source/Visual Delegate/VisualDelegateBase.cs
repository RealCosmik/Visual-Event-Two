﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace VisualEvent
{
    public abstract class VisualDelegateBase
    {
        [SerializeField] protected bool isinvoking;
        [NonSerialized]
        public int currentIndex;
        /// <summary>List of raw <see cref="RawCall"/> objects that this Publisher invokes using predefined arguments</summary>
        [SerializeReference]
        public List<RawDelegate> m_calls;
        [SerializeField] protected MonoBehaviour Yield_target;
        public bool hasyield;
        /// <summary>
        ///  flag for if this delgate has had its called generated
        /// </summary>
        protected bool isinitialized;
        private int pre_initcalls;
        protected abstract void AppendCallToInvocation(RawDelegate raw_call);
        protected abstract void RemoveCallFromInvocation(RawDelegate raw_call);
        private protected abstract void InitializeYieldList();

        protected abstract Delegate oninvoke { get; set; }

        /// <summary>
        /// Deseralizes all editor delegates and appends to oninvoke event
        /// </summary>
        public void initialize()
        {
            if (hasyield)
                InitializeYieldList();
            int tempListLength = m_calls.Count;
            for (int i = 0; i < tempListLength; i++)
            {
                if (m_calls[i] != null && m_calls[i].delegateInstance != null)
                {
                    AppendCallToInvocation(m_calls[i]);
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
        protected internal bool removeCall(RawDelegate tCall)
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
                RawDelegate tempCall = m_calls[tIndex];

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
        /// <summary>
        /// Utility method that returns a yield break
        /// </summary>
        /// <returns></returns>
        protected IEnumerator BreakYield()
        {
            yield break;
        }

        /// <summary>
        /// Method used to add runtime delegates to editor list for debugging purposes
        /// </summary>
        /// <param name="runtimeDelegate"></param>
        protected void AddRuntimeDelegateEditor(Delegate runtimeDelegate)
        {
            if (Application.isEditor)
            {
                //this block of code only exist so that the list view in editor reflects the invocation order of the delgate itself
                //we cannot rely on editor proprty drawers to achieve this function because that assumes that the user has this delegate selected
                //in editor
                var runtiemcall = new RawRuntimeDelegate(runtimeDelegate);
                runtiemcall.delegateInstance = runtimeDelegate;
                // we only care about adding to the list 
                if (!isinitialized)
                {
                    /// in the case that this delegate has had calls added to in the editor
                    /// if the user decides to add runtime methods before calling <see cref="initialize"/>
                    /// we have to make sure to add the runtime delegates to the front of the list 
                    m_calls.Insert(pre_initcalls, runtiemcall); // starts at 0
                    pre_initcalls++;
                }
                else m_calls.Add(runtiemcall);
            }
        }
        /// <summary>
        /// Used to remove dynamic calls from editor list for debugging purposes
        /// </summary>
        /// <param name="runttimeDelegate"></param>
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
