namespace VisualEvents
{
    public sealed class IntVariable : GenericValueVariable<int, int>
    {
        protected override void OnDataUpdated(int arg) => currentValue = arg;
    }
}
