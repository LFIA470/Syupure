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
        Debug.Log("UIManagerのAwake()が呼ばれました！");

        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            // この（重複した）コンポーネントだけを破棄する
            Debug.LogWarning("UIManagerが重複しています。このコンポーネントを破棄します。");
            Destroy(this);
        }
    }

    #endregion

    //変数宣言
    #region Variables
    [Header("AppealTexts")]
    [SerializeField] private Text playerAppealPointText;
    [SerializeField] private Text enemyAppealPointText;

    [Header("Buttons")]
    [SerializeField] private GameObject turnEndButton;
    #endregion

    //UI更新メソッド
    #region UppdataUI Methods
    public void UppdateAppealPointUI    //GameManagerから呼び出され、アピールポイントのUIを更新する
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

    public void SetTurnEndButtonActive(bool isActive)   //ボタンの表示・非表示を切り替えるメソッド
    {
        if (turnEndButton != null)
        {
            turnEndButton.SetActive(isActive);
        }
    }
    #endregion
}
