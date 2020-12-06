using System;
namespace VisualEvents
{
    public class RuntimeResponse : EventResponse
    {
        public Delegate response { get; private set; }
        internal RuntimeResponse(Delegate newresponse,int priority,UnityEngine.Object subscriber) :base(priority)
        {
            subscriberID = subscriber.GetInstanceID();
            response = newresponse;
        }
        public sealed override void Release() => response = null;

        protected internal override void Invoke() => (response as Action).Invoke();

        protected internal override void Invoke<Arg1>(Arg1 arg1) => (response as Action<Arg1>).Invoke(arg1);

        protected internal override void Invoke<Arg1, Arg2>(Arg1 arg1, Arg2 arg2) => (response as Action<Arg1,Arg2>).Invoke(arg1,arg2);

        protected internal override void Invoke<Arg1, Arg2, Arg3>(Arg1 arg1, Arg2 arg2, Arg3 arg3) => (response as Action<Arg1, Arg2,Arg3>).Invoke(arg1, arg2,arg3);

        protected internal override void Invoke<Arg1, Arg2, Arg3, Arg4>(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4) => 
            (response as Action<Arg1, Arg2, Arg3,Arg4>).Invoke(arg1, arg2, arg3,arg4);
    }
}
