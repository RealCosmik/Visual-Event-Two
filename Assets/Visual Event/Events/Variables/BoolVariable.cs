namespace VisualDelegates.Events
{
    public class BoolVariable : GenericVariable<bool>
    {
        public override void ModifyBy(bool modifier) => Invoke(currentValue = modifier, null);
    }
}
