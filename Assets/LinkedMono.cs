using UnityEngine;
using VisualEvent;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Events;
class LinkedMono : MonoBehaviour,IDelegateSerializer
{
    public intpub Cosmik;
    public UnityEvent unity;
    public Vector3 vecfield = Vector3.one;
    public int RandomPropp { get => 2; set { Debug.Log("set value to be" + value); } }
    public int mynum;
    public void methodforikra() => Debug.Log("<color=green>this is for ikram</color>");
    public void methodforikram(int x) => Debug.Log("printing as an int" + x);
    public void throwexception() => throw new UnityException("TRY AND STOP THIS");
    public void methodforikram(string s) => Debug.Log("print as a string");
    private List<Func<IEnumerator>> coroutine_delegate;
    [SerializeField] Testmono mymonotest;
    public bool ExampleSwitch;
    public char idk;
     
    public enum nums { one,two,thre,four,five}
    public nums mynumss;
    public int input = 3;
    public void customtype(Person p) => Debug.Log("custom type");
    public void oddFunction(Action a) => Debug.Log("useless");
    private void Start()
    { 
        Cosmik.initialize();
        Cosmik.OnInvoke += val => Debug.Log("nice little runtime");
    }
    public void testobj(ScriptableObject o)
    {
        Debug.LogError("bad so");
    }
    private void IkramislookingProgrammer(int x) => Debug.Log("<color=blue> she loves me with code</color>");
    public void IkramislookingArt(int x) => Debug.Log("<color=blue> she loves me without code </color>");
    IEnumerator first()
    {
        Debug.Log("first");
        yield return new WaitForSeconds(1f);
        Debug.Log("first end");
    }
    public IEnumerator infinity()
    { 
        while (true)
        {
            Debug.Log("forever");
            yield return null;
        }
    }
    IEnumerator second()
    {
        Debug.Log("second");
        yield return new WaitForSeconds(1f);
        Debug.Log("second end");
    }


    IEnumerator testerco(IEnumerator routine)
    {
        while (true)
        {
            object Current;
            try
            {
                Current = routine.Current;
                if (!routine.MoveNext())
                    yield break;
            }
            catch (MissingReferenceException)
            {
                yield break;
            }
            yield return Current;

        }
    }
    public void hope(Func<int> f) => Debug.Log(f());
    private void saythis() => Debug.Log("im saying something");
    private void saythisalso() => Debug.Log("im saying this also");

    public IEnumerator WaitForSwitch(bool Activation)
    {
        Debug.Log("wow");
        yield return new WaitUntil(() => Activation);
        Debug.Log("made it past here");
    }
    public IEnumerator WaitForSwitch(Func<bool> Activation)
    {
        Debug.Log("wow");
        yield return new WaitUntil(Activation);
        Debug.Log("made it past here");
    }
    public IEnumerator WaitforIntValue(Func<int> value)
    {
        Debug.Log("got in here");
        yield return new WaitUntil(()=>value() == 748);
        Debug.Log("finally got a good value");
    }
    public IEnumerator WaitforIntValue(Func<int> value, int targetValue)
    {
        while (value() != targetValue)
        {
            Debug.Log($"current value is {value()}");
            yield return null;
        }
        Debug.Log("finally got a good value");
    }
    public IEnumerator WaitforIntValue(int value, int targetValue)
    {
        while (value != targetValue)
        {
            Debug.Log($"bad value is {value}");
            yield return null;
        }
        Debug.Log("finally got a good value");
    }

    private void methodWithNum(int x)
    {
        Debug.Log("ayeeeeeee" + x);
    }
    public IEnumerator _Coroutinetest2()
    {
        Debug.Log("test 2");
        yield return new WaitForSeconds(2f);
        Debug.Log("test 2 end");
    }
    public IEnumerator WaitForSeconds(int time)
    {
        Debug.Log("starting wait time of" + time);
        yield return new WaitForSeconds(time);
    }
    private void Testmethod()
    {
        Debug.LogWarning("real method");
    }
    public void doDamage() => Destroy(this);
    private void Update()
    {
        //UnityEngine.Profiling.Profiler.BeginSample("delgate invoke", this);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Cosmik.Invoke(2);
            // StartCoroutine(RunDelegate());
            // UnityEditor.EditorApplication.isPlaying = false;
        }
        //UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("delgate init", this);
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Break();
        }
       UnityEngine.Profiling.Profiler.EndSample();
    }
}
[System.Serializable]
public sealed class intpub : VisualDelegate<int> { }
[System.Serializable]
public class visual4 : VisualDelegate<int, string, bool, char> { }
[System.Serializable]
public class stringpub : VisualDelegate<string> { }
