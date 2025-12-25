using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAccessoryCard", menuName = "Cards/Accessory Card")]

public class AccessoryCard : Card
{
    [Header("アクセサリーカードの情報")]
    public int cost;

    [Header("進化情報")]
    public int evolveBaseID;
    public int evolveTargetID;
}
