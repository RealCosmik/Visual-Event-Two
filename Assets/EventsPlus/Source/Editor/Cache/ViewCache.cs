using System.Collections.Generic;
using EventsPlus;
using UnityEditor;
public static class ViewCache
{
    public static Dictionary<string, List<RawDelegateView>> Cache { get; private set; } = new Dictionary<string, List<RawDelegateView>>();
}
