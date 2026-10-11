using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 camFollow;
    Transform ball, win;
    float shakeTime, shakeMagnitude;

    private void Start()
    {
        ball = FindAnyObjectByType<Ball>().transform;

        // Win is spawned by LevelSpawner in Awake, so it exists by Start
        GameObject winObj = GameObject.Find("Win(Clone)");
        if (winObj != null)
            win = winObj.transform;

        camFollow = transform.position;
    }

    void Update()
    {
        float minY = win != null ? win.position.y + 5f : float.MinValue;

        // Only follow the ball downward, and stop above the win platform
        if (camFollow.y > ball.position.y && camFollow.y > minY)
            camFollow.y = ball.position.y;

        Vector3 shakeOffset = Vector3.zero;
        if (shakeTime > 0f)
        {
            shakeTime -= Time.unscaledDeltaTime;
            shakeOffset = Random.insideUnitCircle * shakeMagnitude;
        }

        transform.position = camFollow + shakeOffset;
    }

    public void Shake(float duration, float magnitude)
    {
        shakeTime = duration;
        shakeMagnitude = magnitude;
    }
}
