using UnityEngine;
using System.Collections;
class Testmono : MonoBehaviour
{
    public IEnumerator memleaktest()
    {
        while (true)
        {
            if (gameObject != null)
            {
                transform.localPosition += Vector3.one;
                Debug.Log("wow");
            }
            yield return null;
        }
    }
}
