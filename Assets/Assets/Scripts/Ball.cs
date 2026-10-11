using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    private float currentTime;
    UI Ui;

    // Tags on the platform parts
    const string BreakableTag = "Breakable", DangerTag = "Danger";

    private bool smash, invincible;
    // A tap that hasn't broken a platform yet, so a quick tap always breaks exactly one
    private bool tapPending;
    private float holdTime;
    private bool wasPlaying;
    private float finishTime;

    [Header("Smash")]
    // Smash speed when the press starts, ramping up to max after smashRampTime of holding
    public float minSmashSpeed = 4f, maxSmashSpeed = 15f;
    public float smashRampTime = 0.4f;
    private float normalFixedDeltaTime;
    public float bounceSpeed = 5f;
    int currentBrokenStacks, totalStacks;

    public GameObject invincibleObj;
    public Image invincibleFill;
    public GameObject fireEffect, winEffect, splashEffect;
    public enum GameState
    {
        Prepare,
        Playing,
        Died,
        Finish
    }

    [HideInInspector]
    public GameState state = GameState.Prepare;

    public AudioClip bounceClip, deadClip, winClip, destroyClip, iDestroyClip;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Fast smashing can tunnel through thin plates with Discrete detection
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        currentBrokenStacks = 0;
        normalFixedDeltaTime = Time.fixedDeltaTime;
    }
    void Start()
    {
        totalStacks = FindObjectsByType<StackController>(FindObjectsSortMode.None).Length;
        Ui = FindAnyObjectByType<UI>();
    }

    private void Update()
    {
        if (state == GameState.Playing)
        {
            // The press that started the game doesn't count as a smash tap
            if (Input.GetMouseButtonDown(0) && wasPlaying)
            {
                tapPending = true;
                holdTime = 0f;
            }

            // Holding smashes, including the hold that started the game
            smash = Input.GetMouseButton(0);
            if (smash)
                holdTime += Time.deltaTime;

            if (invincible)
            {
                currentTime -= Time.deltaTime * 0.35f;
                if (!fireEffect.activeInHierarchy)
                    fireEffect.SetActive(true);
            }
            else
            {
                if (fireEffect.activeInHierarchy)
                    fireEffect.SetActive(false);

                if (smash)
                    currentTime += Time.deltaTime * 0.8f;
                else
                    currentTime -= Time.deltaTime * 0.5f;
            }

            if (currentTime >= 0.3f || invincibleFill.color == Color.blue)
                invincibleObj.SetActive(true);
            else
                invincibleObj.SetActive(false);

            if (currentTime >= 1)
            {
                currentTime = 1;
                invincible = true;
                invincibleFill.color = Color.blue;
            }
            else if (currentTime <= 0)
            {
                currentTime = 0;
                invincible = false;
                invincibleFill.color = Color.white;
            }

            if (invincibleObj.activeInHierarchy)
                invincibleFill.fillAmount = currentTime / 1f;
        }

        if (state == GameState.Finish)
        {
            // Short delay so a tap meant for the game doesn't skip the finish screen
            if (Input.GetMouseButtonDown(0) && Time.unscaledTime - finishTime > 0.5f)
                FindAnyObjectByType<LevelSpawner>().NextLevel();
        }

        wasPlaying = state == GameState.Playing;
    }

    bool Smashing { get { return smash || tapPending; } }

    float SmashSpeed()
    {
        if (!smash)
            return minSmashSpeed;
        return Mathf.Lerp(minSmashSpeed, maxSmashSpeed, holdTime / smashRampTime);
    }

    // After breaking a platform, keep smashing only while the button is held
    void AfterBreak()
    {
        tapPending = false;
        rb.velocity = smash ? new Vector3(0, -SmashSpeed(), 0) : Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (state == GameState.Playing)
        {
            // Continuously push down while smashing
            if (Smashing)
                rb.velocity = new Vector3(0, -SmashSpeed(), 0);
        }

        if (rb.velocity.y > bounceSpeed)
            rb.velocity = new Vector3(rb.velocity.x, bounceSpeed, rb.velocity.z);

    }

    public void IncreaseBrokenStacks()
    {
        currentBrokenStacks++;
        if (!invincible)
        {
            ScoreManager.instance.UpdateScoreText(1);
            SoundManager.instance.PlaySound(destroyClip, 0.5f);
        }
        else
        {
            ScoreManager.instance.UpdateScoreText(2);
            ScoreManager.instance.ShowPopup("+2");
            SoundManager.instance.PlaySound(iDestroyClip, 0.5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // The ball can touch another part in the same physics step it died on
        if (state == GameState.Died)
            return;

        // Another part of a platform that already shattered in this physics step
        if (!collision.collider.enabled)
            return;

        if (!Smashing)
        {
            rb.velocity = new Vector3(0, bounceSpeed, 0);  // Upward bounce when not smashing
            if (!collision.gameObject.CompareTag("Finish"))
            {
                GameObject splash = Instantiate(splashEffect);
                splash.transform.SetParent(collision.transform);
                splash.transform.localEulerAngles = new Vector3(90, Random.Range(0, 359), 0);
                float randomScale = Random.Range(0.18f, 0.25f);
                splash.transform.localScale = new Vector3(randomScale, randomScale, 1);
                splash.transform.position = new Vector3(transform.position.x, transform.position.y - 0.22f, transform.position.z);
                splash.GetComponent<SpriteRenderer>().color = transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
            }

            SoundManager.instance.PlaySound(bounceClip, 0.5f);
        }
        else
        {
            // While smashing, maintain downward velocity and break blocks
            if (invincible)
            {
                if (collision.gameObject.CompareTag(BreakableTag) || collision.gameObject.CompareTag(DangerTag))
                {
                    collision.transform.parent.GetComponent<StackController>().ShatterAllParts();
                    AfterBreak();
                }
            }
            else
            {
                if (collision.gameObject.CompareTag(BreakableTag))
                {
                    collision.transform.parent.GetComponent<StackController>().ShatterAllParts();
                    AfterBreak();
                }

                if (collision.gameObject.CompareTag(DangerTag))
                    Die();
            }

            // A tap is used up by whatever it hits (a platform, or the win platform)
            tapPending = false;
        }

        Ui.LevelSliderFill(currentBrokenStacks / (float)totalStacks);

        if (collision.gameObject.CompareTag("Finish") && state == GameState.Playing)
            Win();
    }

    void Die()
    {
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
        transform.GetChild(0).gameObject.SetActive(false);
        state = GameState.Died;
        SoundManager.instance.PlaySound(deadClip, 0.5f);
        PlayerPrefs.Save();
        FindAnyObjectByType<CameraFollow>().Shake(0.25f, 0.15f);
    }

    void Win()
    {
        state = GameState.Finish;
        finishTime = Time.unscaledTime;
        SoundManager.instance.PlaySound(winClip, 0.7f);
        GameObject win = Instantiate(winEffect);
        win.transform.SetParent(Camera.main.transform);
        win.transform.localPosition = Vector3.up * 1.5f;
        win.transform.eulerAngles = Vector3.zero;
        StartCoroutine(WinSlowMotion());
    }

    IEnumerator WinSlowMotion()
    {
        SetTimeScale(0.3f);
        yield return new WaitForSecondsRealtime(0.6f);
        SetTimeScale(1f);
    }

    void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        // Keep physics smooth while slowed down
        Time.fixedDeltaTime = normalFixedDeltaTime * scale;
    }

    private void OnDestroy()
    {
        // The scene can reload during slow motion
        SetTimeScale(1f);
    }

    void OnCollisionStay(Collision collision)
    {
        // Only apply upward velocity when not smashing (normal bounce)
        // When smashing, let the FixedUpdate maintain downward velocity
        if (state == GameState.Died)
            return;

        if (!Smashing || collision.gameObject.CompareTag("Finish"))
        {
            rb.velocity = new Vector3(0, bounceSpeed, 0);
        }
    }
}
