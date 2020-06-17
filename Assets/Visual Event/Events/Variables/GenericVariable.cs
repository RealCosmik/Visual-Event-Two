using UnityEngine;
namespace VisualDelegates.Events
{
    public abstract class GenericVariable<vartype> : GenericEvent<vartype>, IVisualVariable
    {
        [SerializeField] protected vartype initialValue;
        [SerializeField] protected vartype currentValue;
        public vartype Value => currentValue;
        public abstract void ModifyBy(vartype modifier);
        public sealed override void Invoke(vartype arg1, Object sender)
        {
            currentValue = arg1;
            base.Invoke(arg1, sender);
        }
        private void OnEnable() => currentValue = initialValue;
    }
}