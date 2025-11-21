using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private Text resultText;

    void Start()
    {
        //データを読み取って表示を変える
        if (GameResultData.IsPlayerWin)
        {
            resultText.text = "YOU WIN!!";
            resultText.color = Color.red; // 例：勝ちなら赤文字
        }
        else
        {
            resultText.text = "YOU LOSE...";
            resultText.color = Color.white; // 例：負けなら青文字
        }
    }

    public void OnStartDataButtonClicked()  //スタートボタン（画面）が押されたら呼ばれる
    {
        Debug.Log("ゲームを開始します！");

        SceneManager.LoadScene("Game");
    }
}
