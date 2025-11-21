using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public void OnStartDataButtonClicked()  //スタートボタン（画面）が押されたら呼ばれる
    {
        Debug.Log("ゲームを開始します！");

        SceneManager.LoadScene("Game");
    }
}
