using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //シングルトン
    #region Singleton
    public static UIManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
        }
    }

    #endregion

    //変数宣言
    #region Variables
    [SerializeField] private Text playerAppealPointText;
    [SerializeField] private Text enemyAppealPointText;
    #endregion

    //UI更新メソッド
    #region UppdataUI Methods
    public void UppdateAppealPointUI    //GameManagerから呼び出され、画面のUIを更新する
    (int playerPoints, int enemyPoints)
    {
        if (playerAppealPointText != null)
        {
            playerAppealPointText.text = playerPoints.ToString();
        }
        if (enemyAppealPointText != null)
        {
            enemyAppealPointText.text = enemyPoints.ToString();
        }
    }
    #endregion
}
