using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MixDefine", menuName = "puzzlegimmicksunity/MixDefine")]
public class MixDefine : ScriptableObject
{
    public List<Session.Mix> mixes = new List<Session.Mix>();
}