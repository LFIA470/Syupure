using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAppealCard", menuName = "Cards/Appeal Card")]


public class AppealCard : Card
{
    [Header("アピールカードの情報")]
    public int cost;
}
