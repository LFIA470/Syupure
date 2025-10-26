using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvolveCharacterCard", menuName = "Cards/EvolveCharacter Card")]


public class EvolveCharacterCard : Card
{
    [Header("進化キャラクターカードの情報")]
    public int appeal;
    public int evolveCost;
}
