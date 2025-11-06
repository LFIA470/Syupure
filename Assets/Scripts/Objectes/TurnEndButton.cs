using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndButton : MonoBehaviour
{
    //ターンエンドボタンがクリックされた時のメソッド
    #region Click TurnEnd Methods
    public void OnTurnEndClick()    //ターンエンドボタンがクリックされたか
    {
        Debug.Log("ターンが終了しました。");
        GameManager.Instance.EndTurn(TurnOwner.Player);
    }
    #endregion
}
