namespace VisualEvents
{
    public sealed class FloatVariable : GenericValueVariable<float,float>
    {
        protected override void OnDataUpdated(float arg) => currentValue = arg;
    }
}
