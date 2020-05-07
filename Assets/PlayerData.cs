using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Data",menuName ="Player Data")]
public class PlayerData : ScriptableObject
{
    public bool isdead;
    public int health;
    public IEnumerator offscenemethod(int x)
    {
        Debug.Log("starting from SO");
        yield return new WaitForSeconds(2f);
        Debug.LogWarning("FINISHED");
    }
    public IEnumerator Infiniteoffscene()
    {
        Debug.Log("Start infnite loop");
        while (true)
        {
            Debug.Log("repeat");
            yield return new WaitForSeconds(1f);
        }
    }

}
