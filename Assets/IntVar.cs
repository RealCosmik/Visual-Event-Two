using VisualDelegates;
using VisualDelegates.Events;
public class IntVar : GenericVariable<int>
{
    public override void ModifyBy(int modifier) => Invoke(currentValue += modifier, null);
}
