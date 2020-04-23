using UnityEngine;
using System.Collections;
class Testmono : MonoBehaviour
{
    public void testmonomethod() => Debug.Log("so will this leak");
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
