using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnStartDataButtonClicked()  //スタートボタンが押されたら呼ばれる
    {
        Debug.Log("ゲームを開始します！");

        SceneManager.LoadScene("Game");
    }
}
