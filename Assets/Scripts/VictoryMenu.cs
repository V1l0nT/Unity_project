using UnityEngine;
using UnityEditor;

public class VictoryMenu : MonoBehaviour
{
    private LTDescr restartAnimation;

    //[SerializeField]
    //private TMPro.TextMeshProUGUI highScoreText; // Убрали поле для вывода high score

    private void OnEnable()
    {
        // Убрали вывод high score
        // highScoreText.text = $"High Score: {GameManager.Instance.HighScore}";

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, rectTransform.rect.height);

        rectTransform.LeanMoveY(0, 1f).setEaseOutElastic().delay = 0.5f;

        if (restartAnimation == null)
        {
            restartAnimation = GetComponentInChildren<TMPro.TextMeshProUGUI>()
                .gameObject.LeanScale(new Vector3(1.2f, 1.2f), 0.5f)
                .setLoopPingPong();
        }
        restartAnimation.resume();
    }

    public void Restart()
    {
        restartAnimation.pause();
        gameObject.SetActive(false);

        GameManager.Instance.Enable();
        GameManager.Instance.EnablePlayerControl();
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
