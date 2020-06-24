namespace VisualDelegates.Events
{
    public class IntVariable :GenericVariable<int>
    {
        public override void ModifyBy(int modifier) => Invoke(currentValue += modifier, null);
    }
}
