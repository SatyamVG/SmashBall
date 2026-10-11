using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public int score = 0;
    private Text scoreText;

    private void Awake()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreText(0);
    }

    private void Update()
    {
        if (scoreText == null)
        {
            scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
            scoreText.text = score.ToString();
        }
    }

    public void UpdateScoreText( int points)
    {
         score += points;
        if (score > PlayerPrefs.GetInt("HighScore", 0))
            PlayerPrefs.SetInt("HighScore", score);

        scoreText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
    }

    // Floating "+2" under the score that rises and fades out
    public void ShowPopup(string text)
    {
        Text popup = Instantiate(scoreText, scoreText.transform.parent);
        popup.name = "ScorePopup";
        popup.text = text;
        popup.raycastTarget = false;
        popup.transform.localScale = Vector3.one * 0.6f;
        StartCoroutine(AnimatePopup(popup));
    }

    IEnumerator AnimatePopup(Text popup)
    {
        RectTransform rt = popup.rectTransform;
        Vector2 start = rt.anchoredPosition + Vector2.down * 110f;
        Color color = popup.color;
        float duration = 0.6f;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            // The scene may reload while the popup is still animating
            if (popup == null)
                yield break;

            float k = t / duration;
            rt.anchoredPosition = start + Vector2.up * (60f * k);
            popup.color = new Color(color.r, color.g, color.b, 1f - k);
            yield return null;
        }

        if (popup != null)
            Destroy(popup.gameObject);
    }
}
