using VisualEvents;
public class IntVar : GenericValueVariable<int, int>
{
    protected override void OnDataUpdated(int arg) => currentValue = arg;
}
