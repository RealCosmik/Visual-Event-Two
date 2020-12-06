using UnityEditor.IMGUI.Controls;
using System;
namespace VisualEvents.Editor
{
    class DynamicResponseTreeElement : GenericResponseElement
    {
        public string methodMessage;
        public string targetMessage;

        public DynamicResponseTreeElement(Delegate response, int subscriberid, int priority, int eventindex) : base(subscriberid, priority, eventindex)
        {
            methodMessage = ParseDynamicMethodName(response.Method.Name);
            targetMessage = ParseDynamicTargetName(response.Target.GetType().FullName);
        }
        public static string ParseDynamicMethodName(string methodname)
        {
            if (methodname[0] == '<')//anon method
            {
                return $@"Method: Anonymous method created in ""{methodname.Substring(1, methodname.IndexOf('>') - 1)}""";
            }
            else return $@"Method: ""{methodname}""";
        }
        public static string ParseDynamicTargetName(string TargetType)
        {
            if (TargetType.Contains("+"))
            {
                return $@"Target: Anonymous Type Created in ""{TargetType.Substring(0, TargetType.IndexOf('+'))}""";
            }
            return $@"Target: ""{TargetType}""";
        }
    }

}