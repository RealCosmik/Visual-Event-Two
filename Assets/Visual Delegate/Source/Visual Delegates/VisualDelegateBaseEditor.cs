using System;
using System.Linq;
using UnityEngine;
namespace VisualDelegates
{
    // all methods in this class are invoked either by refleciton or in editor mode only never in builds
    public abstract partial class VisualDelegateBase
    {
        private protected event Action m_internalcalls;
        /// <summary>
        /// Method used to add runtime delegates to editor list for debugging purposes
        /// </summary>
        /// <param name="runtimeDelegate"></param>
        private protected void AddRuntimetoEditor(Delegate runtimeDelegate)
        {
            if (Application.isEditor)
            {
                //this block of code only exist so that the list view in editor reflects the invocation order of the delgate itself
                //we cannot rely on editor proprty drawers to achieve this function because that assumes that the user has this delegate selected
                //in editor
                var runtiemcall = new RawRuntimeCall(runtimeDelegate);
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
        private protected void RemoveRuntimeFromEditor(Delegate runttimeDelegate)
        {
            if (Application.isEditor)
            {
                var removed_call = m_calls.FirstOrDefault(rc => rc.delegateInstance == runttimeDelegate);
                if (removed_call != null)
                    m_calls.RemoveAt(m_calls.IndexOf(removed_call));
            }
        }
        private protected void InvokeInternalCall()
        {
            if (Application.isEditor)
                m_internalcalls?.Invoke();
        }
    }
}