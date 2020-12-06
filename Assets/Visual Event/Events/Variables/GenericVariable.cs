using UnityEngine;
namespace VisualEvents
{
    public abstract class GenericValueVariable<VarType, ArgumentType> : GenericEvent<ArgumentType>, IVisualVariable where VarType : struct
    {
        [SerializeField] protected VarType initialValue;
        [SerializeField] protected VarType currentValue;
        public VarType Value => currentValue;
        protected abstract void OnDataUpdated(ArgumentType arg);
        protected virtual void InitializeVariable() => currentValue = initialValue;
        public sealed override void Invoke(ArgumentType arg1, Object sender)
        {
            OnDataUpdated(arg1);
            base.Invoke(arg1, sender);
        }
        protected void OnEnable() => InitializeVariable();
    }

    public abstract class GenericReferenceVariable<VarType, ArgumentType> : GenericEvent<ArgumentType>, IVisualVariable where VarType : class
    {
        [SerializeField] protected VarType initialValue;
        [SerializeField] protected VarType currentValue;
        public VarType Value => currentValue;
        protected abstract void OnDataUpdated(ArgumentType arg);
        protected abstract void InitializeVariable();
        public sealed override void Invoke(ArgumentType arg1, Object sender)
        {
            OnDataUpdated(arg1);
            base.Invoke(arg1, sender);
        }
        protected void OnEnable() => InitializeVariable();
    }

}