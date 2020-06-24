namespace VisualDelegates.Events
{
    public class FloatVariable :GenericVariable<float>
    {
        public override void ModifyBy(float modifier)
        {
            Invoke(currentValue += modifier, null);
        }
    }
}
