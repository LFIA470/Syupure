using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public void StartEnemyTurn()    //相手のターンの行動を開始する
    {
        StartCoroutine(AIActionRoutine());
    }

    private IEnumerator AIActionRoutine()
    {
        Debug.Log("AI：考え中...");

        //人間らしく見せるために少し待つ
        yield return new WaitForSeconds(1.5f);

        //カードを出す処理

        //ターン終了
        Debug.Log("AI：ターンエンド！");
        GameManager.Instance.EndTurn(TurnOwner.Enemy);
    }
}
