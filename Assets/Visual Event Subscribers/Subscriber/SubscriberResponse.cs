using UnityEngine;
using VisualDelegates;
namespace VisualEvents.VisualSubscribers
{
    [System.Serializable]
    public class SubscriberResponse : EventResponse
    {
        [UnityEngine.SerializeReference] VisualDelegateBase response = null;
        public VisualDelegateBase CurrentResponse => response;


        [SerializeField] string responseNote = string.Empty;
        /// <summary>
        /// Strictly used for editor do not use for runtime logic
        /// </summary>
        [System.NonSerialized] internal int responseIndex;

        internal SubscriberResponse(VisualDelegateBase newresponse, int newpriority) : base(newpriority)
        {
            response = newresponse;
        }

        /// <summary>
        /// Releases the <see cref="VisualDelegate"/> attached to this response
        /// </summary>
        public sealed override void Release() => response.Release();

        protected sealed override void Invoke() => (CurrentResponse as VisualDelegate).Invoke();

        protected override void Invoke<Arg1>(Arg1 arg1) => (response as VisualDelegate<Arg1>).Invoke(arg1);

        protected override void Invoke<Arg1, Arg2>(Arg1 arg1, Arg2 arg2) => (response as VisualDelegate<Arg1, Arg2>).Invoke(arg1, arg2);

        protected override void Invoke<Arg1, Arg2, Arg3>(Arg1 arg1, Arg2 arg2, Arg3 arg3) => (response as VisualDelegate<Arg1, Arg2, Arg3>).Invoke(arg1, arg2, arg3);

        protected override void Invoke<Arg1, Arg2, Arg3, Arg4>(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4) =>
            (response as VisualDelegate<Arg1, Arg2, Arg3, Arg4>).Invoke(arg1, arg2, arg3, arg4);
    }
}
