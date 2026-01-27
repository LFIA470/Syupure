using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Animator starAnimator;

    void Start()
    {
        StartCoroutine(PlayStarAnimationDelay());
    }

    public void PlayShootingStar()
    {
        if (starAnimator != null)
        {
            starAnimator.SetTrigger("PlayStar");
        }
    }

    IEnumerator PlayStarAnimationDelay()
    {
        yield return new WaitForSeconds(1.5f);

        PlayShootingStar();
    }

    public void OnStartDataButtonClicked()  //スタートボタン（画面）が押されたら呼ばれる
    {
        Debug.Log("ゲームを開始します！");

        SceneManager.LoadScene("DeckEdit");
    }
}
