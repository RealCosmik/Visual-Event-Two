using VisualDelegates;
using VisualDelegates.Events;
public class IntVar : GenericVariable<int>,IVisualVariable
{
    public override void ModifyBy(int modifier) => Invoke(currentValue += modifier, null);
}
