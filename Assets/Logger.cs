using UnityEngine;
using System.Reflection;
using System;
public class Logger
{
    public static void Log(object obj)
    {
        Debug.Log(obj);
    }
    public static void IkramsMethod()
    {
        Debug.LogWarning("I am Mad");
    }
    public static MethodInfo GetLoggermethod(string methodname) => typeof(Logger).GetMethod(methodname);
}
