using UnityEngine;
namespace VisualDelegates.Events
{
    public abstract class GenericVariable<vartype> : GenericEvent<vartype>, IVisualVariable
    {
        [SerializeField] protected vartype initialValue;
        [SerializeField] protected vartype CurrentValue;
        public vartype Value => CurrentValue;
        public abstract void ModifyBy(vartype modifier);
        public sealed override void Invoke(vartype arg1, Object sender)
        {
            CurrentValue = arg1;
            base.Invoke(arg1, sender);
        }
        private void OnEnable() => CurrentValue = initialValue;
    }
}