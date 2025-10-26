using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEventCard", menuName = "Cards/Event Card")]


public class EventCard : Card
{
    [Header("イベントカードの情報")]
    public int cost;
}
