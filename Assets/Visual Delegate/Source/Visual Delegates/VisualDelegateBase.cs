using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VisualEvent
{
    public abstract partial class VisualDelegateBase
    {
        /// <summary>List of raw <see cref="RawCall"/> objects that this Publisher invokes using predefined arguments</summary>
        [SerializeReference]
        public List<RawDelegate> m_calls;
        protected bool isinitialized;
        private int pre_initcalls;
        protected abstract void AppendCallToEvent(RawDelegate raw_call);
        protected abstract void RemoveCallFromEvent(RawDelegate raw_call);
        /// <summary>
        /// Deseralizes all editor delegates and appends to oninvoke event
        /// </summary>
        public void initialize()
        {
            int tempListLength = m_calls.Count;
            for (int i = 0; i < tempListLength; i++)
            {
                if (m_calls[i] != null && m_calls[i].delegateInstance != null && (!(m_calls[i] is RawRuntimeCall)))
                {
                    AppendCallToEvent(m_calls[i]);
                }
                if (Application.isEditor && isinitialized && m_calls[i] is RawRuntimeCall)
                {
                    AppendCallToEvent(m_calls[i]);
                }
            }
            isinitialized = true;
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
                    m_calls = null;

                RemoveCallFromEvent(tempCall);
                return true;
            }
            return false;
        }
        public virtual void Release()
        {
            for (int i = 0; i < m_calls.Count; i++)
                m_calls[i].Release();
            m_calls.Clear();
        }
    }

}
