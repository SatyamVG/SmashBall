using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject homeUI, inGameUI, finishUI, gameOverUI;
    public GameObject allbuttons;

    private bool buttons;

    [Header("PreGame")]
    public Button soundButton;
    public Sprite soundOn, soundOff;


    [Header("InGame")]
    public Image levelSlider;
    public Image currentLevelImg;
    public Image nextLevelImg;
    public Text currentLevelText,nextLevelText;

    [Header("Finish")]
    public Text finishLevelText;
    // Changed to "You Win!" / "Play Again" after the final level
    public Text completeLevelText, continueText;

    [Header("GameOver")]
    public Text gameOverScoreText;
    public Text highestScore;

    private Material ballMat;
    private Ball ball;
    private LevelSpawner levelSpawner;
    private float diedTime;
    private Ball.GameState shownState = Ball.GameState.Prepare;

    void Awake()
    {
        ball = FindAnyObjectByType<Ball>();
        levelSpawner = FindAnyObjectByType<LevelSpawner>();
        soundButton.onClick.AddListener(() => SoundManager.instance.SoundOnOff());
    }

    private void Start()
    {
        // Colors are set in Start so LevelSpawner.Awake has already picked the ball color
        ballMat = ball.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        SetColor(ballMat.color);

        currentLevelText.text = levelSpawner.level.ToString();
        // There is no next level after the final one
        nextLevelText.text = levelSpawner.IsFinalLevel ? "" : levelSpawner.level + 1 + "";
    }

    public void SetColor(Color color)
    {
        levelSlider.transform.parent.GetComponent<Image>().color = color + Color.gray;
        levelSlider.color = color;
        currentLevelImg.color = color;
        nextLevelImg.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (ball.state ==  Ball.GameState.Prepare)
        {
            if(SoundManager.instance.sound  && soundButton.GetComponent<Image>().sprite != soundOn)
                soundButton.GetComponent<Image>().sprite = soundOn;
            else if(!SoundManager.instance.sound && soundButton.GetComponent<Image>().sprite != soundOff)
                soundButton.GetComponent<Image>().sprite = soundOff;
        }

        if (Input.GetMouseButtonDown(0) &&  !IgnoreUI() && ball.state == Ball.GameState.Prepare)
            ball.state = Ball.GameState.Playing;

        // Switch panels only when the state changes
        if (ball.state != shownState)
        {
            shownState = ball.state;
            ShowPanels(shownState);
        }

        if (ball.state == Ball.GameState.Died)
        {
            // Short delay so a tap meant for the game doesn't skip the game over screen
            if (Input.GetMouseButtonDown(0) && Time.unscaledTime - diedTime > 0.5f)
            {
                ScoreManager.instance.ResetScore();
                SceneManager.LoadScene(0);
            }
        }
    }

    void ShowPanels(Ball.GameState state)
    {
        homeUI.SetActive(state == Ball.GameState.Prepare);
        inGameUI.SetActive(state == Ball.GameState.Playing);
        finishUI.SetActive(state == Ball.GameState.Finish);
        gameOverUI.SetActive(state == Ball.GameState.Died);

        if (state == Ball.GameState.Finish)
        {
            finishLevelText.text = "Level " + levelSpawner.level;
            if (levelSpawner.IsFinalLevel)
            {
                completeLevelText.text = "You Win!";
                continueText.text = "Play Again";
            }
        }

        if (state == Ball.GameState.Died)
        {
            gameOverScoreText.text = ScoreManager.instance.score.ToString();
            highestScore.text = PlayerPrefs.GetInt("HighScore").ToString();
            diedTime = Time.unscaledTime;
        }
    }

    private bool IgnoreUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject.GetComponent<Ignore>() != null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }

        return raycastResultList.Count > 0;
    }

   
    public void LevelSliderFill(float fillAmount)
    {
        levelSlider.fillAmount = fillAmount;
    }

    public void Settings()
    {
        buttons = !buttons;
        allbuttons.SetActive(buttons);
    }
}

