namespace VisualEvents
{
    public sealed class BoolVariable : GenericValueVariable<bool,bool>
    {
        protected override void OnDataUpdated(bool arg) => currentValue = arg;
        public static implicit operator bool(BoolVariable boolvar)=>boolvar.currentValue;
    }
}
