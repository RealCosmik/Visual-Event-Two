using UnityEngine;
namespace VisualDelegates.Cinema
{
    [System.Serializable]
    sealed class SignalResponse : VisualDelegate
    {
        [SerializeField] bool validResponse;
        public bool IsValidResponse => validResponse;
    }
}