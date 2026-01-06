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
    [Header("Turn UI")]
    [SerializeField] private List<Image> TurnIconPrefab;
    [SerializeField] private Sprite trueTurnIcon;

    [SerializeField] private Image turnNumber;
    [SerializeField] private List<Sprite> turnNumberPrefab;

    [Header("AppealTexts")]
    [SerializeField] private Text playerAppealPointText;
    [SerializeField] private Text enemyAppealPointText;

    [SerializeField] private Image playerGaugeImage;
    [SerializeField] private Image enemyGaugeImage;

    [SerializeField] private float gaugeMaxScore = 20.0f;

    [Header("CheerPower UI")]
    [SerializeField] private Transform playerCheerPowerArea; // 親オブジェクト
    [SerializeField] private Transform enemyCheerPowerArea; // 親オブジェクト※相手用
    [SerializeField] private GameObject cheerPowerIconPrefab; // アイコンのプレハブ

    [SerializeField] private List<GameObject> playerManaIcons;
    [SerializeField] private List<GameObject> enemyManaIcons;

    [Header("Buttons")]
    [SerializeField] private GameObject turnEndButton;
    #endregion

    //UI更新メソッド
    #region UppdataUI Methods
    public void UpdateTurnCount(int turnCount)
    {
        TurnIconPrefab[turnCount - 1].sprite = trueTurnIcon;

        turnNumber.sprite = turnNumberPrefab[turnCount];
    }
    public void UppdateAppealPointUI    //GameManagerから呼び出され、アピールポイントのUIを更新する
    (int playerPoints, int enemyPoints)
    {
        if (playerAppealPointText != null)  playerAppealPointText.text = playerPoints.ToString();
        if (enemyAppealPointText != null)   enemyAppealPointText.text = enemyPoints.ToString();
    
        if (playerGaugeImage != null)
        {
            float ratio  = (float) playerPoints / gaugeMaxScore;

            playerGaugeImage.fillAmount = Mathf.Clamp01(ratio);
        }

        if (enemyGaugeImage != null)
        {
            float ratio = (float)enemyPoints / gaugeMaxScore;

            enemyGaugeImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }
    public void UpdateCheerPowertUI(int currentMana, int maxMana, TurnOwner owner)
    {
        //操作するアイコンリストを決める
        List<GameObject> targetIcons = (owner == TurnOwner.Player) ? playerManaIcons : enemyManaIcons;

        //リストが設定されていなければエラー回避のため何もしない
        if (targetIcons == null || targetIcons.Count == 0) return;

        //マナの数だけアイコンを表示し、それ以外を非表示にする
        for (int i = 0; i < targetIcons.Count; i++)
        {
            if (i < currentMana)
            {
                //現在のマナ数以下なら表示
                targetIcons[i].SetActive(true);
            }
            else
            {
                //マナが減っている分は非表示
                targetIcons[i].SetActive(false);
            }
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
