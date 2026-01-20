using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLeaderCard", menuName = "Cards/Leader Card")]

public class LeaderCard : Card
{
    [Header("リーダーカードの情報")]
    public int appeal;
}
