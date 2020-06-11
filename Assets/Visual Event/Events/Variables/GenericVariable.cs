using UnityEngine;
namespace VisualDelegates.Events
{
   public class GenericVariable<vartype> : GenericEvent<vartype>, IVisualVariable
    {
        [SerializeField] vartype initialValue;
        [SerializeField] vartype CurrentValue;
        public vartype Value => CurrentValue;
        public sealed override void Invoke(vartype arg1, Object sender)
        {
            CurrentValue = arg1;
            base.Invoke(arg1, sender);
        }
        private void OnEnable()
        {
            CurrentValue = initialValue;
        }
    }
}