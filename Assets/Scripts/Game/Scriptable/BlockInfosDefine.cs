using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockInfosDefine", menuName = "puzzlegimmicksunity/BlockInfosDefine")]
public class BlockInfosDefine : ScriptableObject {
    public List<Session.BlockInfo> blockInfos = new List<Session.BlockInfo>();
}