using UnityEngine;
using System.Collections.Generic;
namespace VisualDelegates
{
    public abstract class DelegatePair<Delegatetype,ScriptableType> : MonoBehaviour where ScriptableType : ScriptableObject where Delegatetype:VisualDelegateBase
    {
        [SerializeReference]
        protected List<Delegatetype> visualDelegates;
        [SerializeField]
        protected List<ScriptableType> Scriptable_objects;
    }
}
