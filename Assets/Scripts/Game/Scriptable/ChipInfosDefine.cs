using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChipInfosDefine", menuName = "puzzlegimmicksunity/ChipInfosDefine", order = 0)]
public class ChipInfosDefine : ScriptableObject
{
    public List<Session.ChipInfo> chipInfos = new List<Session.ChipInfo>();

}