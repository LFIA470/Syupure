using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterCard", menuName = "Cards/Character Card")]


public class CharacterCard : Card
{
    [Header("キャラクターカードの情報")]
    public int appeal;
    public int hp;
    public int cost;
}
