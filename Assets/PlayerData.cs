using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Data",menuName ="Player Data")]
public class PlayerData : ScriptableObject
{
    public bool isdead;
    public int health;
}
