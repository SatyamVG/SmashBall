using UnityEngine;

// Fits this RectTransform inside the screen's safe area, away from notches,
// camera holes and the home bar. Only runs in Play mode.
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform rt;
    Rect appliedArea;
    Vector2Int appliedScreen;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        // Rotation or a resized Game view changes the safe area
        if (Screen.safeArea != appliedArea || Screen.width != appliedScreen.x || Screen.height != appliedScreen.y)
            Apply();
    }

    void Apply()
    {
        appliedArea = Screen.safeArea;
        appliedScreen = new Vector2Int(Screen.width, Screen.height);
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        rt.anchorMin = new Vector2(appliedArea.xMin / Screen.width, appliedArea.yMin / Screen.height);
        rt.anchorMax = new Vector2(appliedArea.xMax / Screen.width, appliedArea.yMax / Screen.height);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
