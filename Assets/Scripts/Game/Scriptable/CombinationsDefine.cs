using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombinationsDefine", menuName = "puzzlegimmicksunity/CombinationsDefine", order = 0)]
public class CombinationsDefine : ScriptableObject
{
    public List<Session.Combinations> combinations = new List<Session.Combinations>(); //load combination

}