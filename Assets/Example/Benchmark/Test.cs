using UnityEngine;
using UnityEngine.Events;
using System;
using VisualDelegates;

namespace EventsPlusTest
{
    //##########################
    // Class Declaration
    //##########################
    [Serializable]
    public class UnityEventInt : UnityEvent<int>
    {
    }

    [Serializable]
    public class Publisher1 : VisualDelegate<int>
    {
    }

    [Serializable]
    public class Publisher2 : VisualDelegate<int, int>
    {
    }

    [Serializable]
    public class Publisher3 : VisualDelegate<int, int, int>
    {
    }

    [Serializable]
    public class Publisher4 : VisualDelegate<int, int, int, int>
    {
    }

    [Serializable]
    public class Publisher5 : VisualDelegate<int, int, int, int, int>
    {
    }

    [Serializable]
    public class Publisher6 : VisualDelegate<int, int, int, int, int, int>
    {
    }

    [Serializable]
    public class Publisher7 : VisualDelegate<int, int, int, int, int, int, int>
    {
    }

    [Serializable]
    public class Publisher8 : VisualDelegate<int, int, int, int, int, int, int, int>
    {
    }

    //##########################
    // Class Declaration
    //##########################
    public class Test : MonoBehaviour
    {
        //=======================
        // Variables
        //=======================
        [Header("Speed Comparison")]
        public int iterations = 1000000;
        public int speedTestVariable;
        public VisualDelegate publisherMethodVoid;
        public UnityEvent unityEventMethod;
        public VisualDelegate publisherMethodReturn;
        public Publisher1 publisherVariable;
        public Publisher1 publisherProperty;
        public UnityEventInt unityEventProperty;
        [Header("Delegate Test")]
        public VisualDelegate publisher0;
        public Publisher1 publisher1;
        public int test1Variable;
        public Publisher2 publisher2;
        public Publisher3 publisher3;
        public Publisher4 publisher4;
        public Publisher5 publisher5;
        public Publisher6 publisher6;
        public Publisher7 publisher7;
        public Publisher8 publisher8;

        //=======================
        // Initialize
        //=======================
        protected virtual void Awake()
        {
            publisherMethodVoid.initialize();
            publisherMethodReturn.initialize();
            publisherVariable.initialize();
            publisherProperty.initialize();
            publisher0.initialize();
            publisher1.initialize();
            publisher2.initialize();
            publisher3.initialize();
            publisher4.initialize();
            publisher5.initialize();
            publisher6.initialize();
            publisher7.initialize();
            publisher8.initialize();
        }

        protected virtual void Start()
        {
            // Speed Test
            Debug.Log("STARTING SPEED TEST");
            float tempTime = Time.realtimeSinceStartup;
            for (int i = iterations; i >= 0; --i)
            {
                publisherMethodVoid.Invoke();
            }
            Debug.Log("Speed Test: visualdelegate Method Void: " + (Time.realtimeSinceStartup - tempTime));



            tempTime = Time.realtimeSinceStartup;
            for (int i = iterations; i >= 0; --i)
            {
                unityEventMethod.Invoke();
            }
            Debug.Log("Speed Test: UnityEvent Method: " + (Time.realtimeSinceStartup - tempTime));

            tempTime = Time.realtimeSinceStartup;
            for (int i = iterations; i >= 0; --i)
            {
                publisherMethodReturn.Invoke();
            }
            Debug.Log("Speed Test: Publisher Method Return: " + (Time.realtimeSinceStartup - tempTime));
            tempTime = Time.realtimeSinceStartup;
            for (int i = iterations; i >= 0; --i)
            {
                publisherVariable.Invoke(999);
            }
            Debug.Log("Speed Test: Publisher Variable: " + (Time.realtimeSinceStartup - tempTime));

            tempTime = Time.realtimeSinceStartup;
            for (int i = iterations; i >= 0; --i)
            {
                publisherProperty.Invoke(999);
            }
            Debug.Log("Speed Test: Publisher Property: " + (Time.realtimeSinceStartup - tempTime));

            tempTime = Time.realtimeSinceStartup;
            for (int i = iterations; i >= 0; --i)
            {
                unityEventProperty.Invoke(999);
            }
            Debug.Log("Speed Test: UnityEvent Property: " + (Time.realtimeSinceStartup - tempTime));

            // Delegate Test
            Debug.Log("STARTING DELEGATE TEST");
            publisher0.Invoke();
            publisher1.Invoke(1);
            publisher2.Invoke(2, 2);
            publisher3.Invoke(3, 3, 3);
            publisher4.Invoke(4, 4, 4, 4);
            publisher5.Invoke(5, 5, 5, 5, 5);
            publisher6.Invoke(6, 6, 6, 6, 6, 6);
            publisher7.Invoke(7, 7, 7, 7, 7, 7, 7);
            publisher8.Invoke(8, 8, 8, 8, 8, 8, 8, 8);
        }

        //=======================
        // Speed Test
        //=======================
        public virtual void speedTestMethodVoid()
        {
            ++speedTestVariable;
        }

        public virtual bool speedTestMethodReturn()
        {
            ++speedTestVariable;
            return true;
        }

        public virtual int speedTestProperty
        {
            set
            {
                speedTestVariable = value;
            }
        }

        public virtual void speedTestMethod(int tVariable)
        {
            speedTestVariable = tVariable;
        }

        //=======================
        // Delegate Test
        //=======================
        public virtual void test0Void()
        {
            Debug.Log("Success: 0-parameter void method");
        }

        public virtual bool test0Return()
        {
            Debug.Log("Success: 0-parameter return method");
            return true;
        }

        public virtual void test1Void(int tA)
        {
            Debug.Log("Success: 1-parameter void method " + tA);
        }

        public virtual bool test1Return(int tA)
        {
            Debug.Log("Success: 1-parameter return method " + tA);
            return true;
        }

        public virtual int test1Property
        {
            set
            {
                Debug.Log("Success: 1-parameter return property " + value);
            }
        }

        public virtual void test2Void(int tA, int tB)
        {
            Debug.Log("Success: 2-parameter void method " + tA + "," + tB);
        }

        public virtual bool test2Return(int tA, int tB)
        {
            Debug.Log("Success: 2-parameter return method " + tA + "," + tB);
            return true;
        }

        public virtual void test3Void(int tA, int tB, int tC)
        {
            Debug.Log("Success: 3-parameter void method " + tA + "," + tB + "," + tC);
        }

        public virtual bool test3Return(int tA, int tB, int tC)
        {
            Debug.Log("Success: 3-parameter return method " + tA + "," + tB + "," + tC);
            return true;
        }

        public virtual void test4Void(int tA, int tB, int tC, int tD)
        {
            Debug.Log("Success: 4-parameter void method " + tA + "," + tB + "," + tC + "," + tD);
        }

        public virtual bool test4Return(int tA, int tB, int tC, int tD)
        {
            Debug.Log("Success: 4-parameter return method " + tA + "," + tB + "," + tC + "," + tD);
            return true;
        }

        public virtual void test5Void(int tA, int tB, int tC, int tD, int tE)
        {
            Debug.Log("Success: 5-parameter void method " + tA + "," + tB + "," + tC + "," + tD + "," + tE);
        }

        public virtual bool test5Return(int tA, int tB, int tC, int tD, int tE)
        {
            Debug.Log("Success: 5-parameter return method " + tA + "," + tB + "," + tC + "," + tD + "," + tE);
            return true;
        }

        public virtual void test6Void(int tA, int tB, int tC, int tD, int tE, int tF)
        {
            Debug.Log("Success: 6-parameter void method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF);
        }

        public virtual bool test6Return(int tA, int tB, int tC, int tD, int tE, int tF)
        {
            Debug.Log("Success: 6-parameter return method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF);
            return true;
        }

        public virtual void test7Void(int tA, int tB, int tC, int tD, int tE, int tF, int tG)
        {
            Debug.Log("Success: 7-parameter void method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG);
        }

        public virtual bool test7Return(int tA, int tB, int tC, int tD, int tE, int tF, int tG)
        {
            Debug.Log("Success: 7-parameter return method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG);
            return true;
        }

        public virtual void test8Void(int tA, int tB, int tC, int tD, int tE, int tF, int tG, int tH)
        {
            Debug.Log("Success: 8-parameter void method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG + "," + tH);
        }

        public virtual bool test8Return(int tA, int tB, int tC, int tD, int tE, int tF, int tG, int tH)
        {
            Debug.Log("Success: 8-parameter return method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG + "," + tH);
            return true;
        }

        public virtual void test9Void(int tA, int tB, int tC, int tD, int tE, int tF, int tG, int tH, int tI)
        {
            Debug.Log("Success: 9-parameter void method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG + "," + tH + "," + tI);
        }

        public virtual bool test9Return(int tA, int tB, int tC, int tD, int tE, int tF, int tG, int tH, int tI)
        {
            Debug.Log("Success: 9-parameter return method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG + "," + tH + "," + tI);
            return true;
        }

        public virtual void test10Void(int tA, int tB, int tC, int tD, int tE, int tF, int tG, int tH, int tI, int tJ)
        {
            Debug.Log("Success: 10-parameter void method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG + "," + tH + "," + tI + "," + tJ);
        }

        public virtual bool test10Return(int tA, int tB, int tC, int tD, int tE, int tF, int tG, int tH, int tI, int tJ)
        {
            Debug.Log("Success: 10-parameter return method " + tA + "," + tB + "," + tC + "," + tD + "," + tE + "," + tF + "," + tG + "," + tH + "," + tI + "," + tJ);
            return true;
        }
    }
}