using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [Header("UI Refernces")]
    [SerializeField] private Text resultText;
    [SerializeField] private Text playerScoreText;
    [SerializeField] private Text enemyScoreText;
    [SerializeField] private GameObject winEffectObj;

    void Start()
    {
        ShowResult();
    }

    private void ShowResult()
    {
        //スコアの表示
        if (playerScoreText != null)
            playerScoreText.text = GameResultData.FinalPlayerScore.ToString();

        if (enemyScoreText != null)
            enemyScoreText.text = GameResultData.FinalEnemyScore.ToString();

        //勝敗による出し分け
        if (GameResultData.IsPlayerWin)
        {
            //勝利時
            if (resultText != null)
            {
                resultText.text = "YOU WIN!!";
            }
        }
        else
        {
            //敗北時
            if (resultText != null)
            {
                resultText.text = "YOU LOSE...";
            }
        }
    }


    public void OnStartDataButtonClicked()  //スタートボタン（画面）が押されたら呼ばれる
    {
        Debug.Log("ゲームを開始します！");

        SceneManager.LoadScene("Game");
    }
}
