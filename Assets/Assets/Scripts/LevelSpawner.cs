
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelSpawner : MonoBehaviour
{
    public GameObject[] blockPrefabs;
    [HideInInspector] public GameObject[] blocks = new GameObject[4];
    public GameObject WinPrefab;

    private GameObject temp1, temp2;

    [HideInInspector] public int level = 1;
    public int addOn = 7;

    [Header("Difficulty")]
    // Last level of the game. Difficulty ramps up every level until here.
    public int maxLevel = 50;
    public bool IsFinalLevel { get { return level >= maxLevel; } }
    // Tower rotation speed at level 1 and at maxLevel
    public float minRotateSpeed = 100f, maxRotateSpeed = 150f;
    // Average dark parts per 4-piece platform at level 1 and at maxLevel (versions have 0-3)
    public float minDarkParts = 0.25f, maxDarkParts = 2.75f;
    float i = 0;

    public Material plateMat, baseMat;
    public MeshRenderer ballMesh;
    UI ui;

    void Awake()
    {
        // Phones default to 30 FPS
        Application.targetFrameRate = 60;

        // Work on runtime copies so the .mat assets on disk are never modified
        Material plateAsset = plateMat, baseAsset = baseMat;
        plateMat = new Material(plateAsset);
        baseMat = new Material(baseAsset);

        plateMat.color = Random.ColorHSV(0, 1, 0.5f, 1, 1, 1);
        baseMat.color = plateMat.color + Color.gray;
        ballMesh.material.color = plateMat.color;

        // Saves from before the level limit can be past maxLevel
        level = Mathf.Clamp(PlayerPrefs.GetInt("Level", 1), 1, maxLevel);
        int length = level;
        // 0 at level 1, 1 at maxLevel
        float progress = maxLevel > 1 ? Mathf.Clamp01((level - 1f) / (maxLevel - 1f)) : 1f;

        float random = Random.value;
        BlockSelection();

        // Hex has 6 pieces instead of 4, so it needs more dark parts to be as hard
        int pieces = blocks[0].GetComponentsInChildren<Collider>(true).Length;
        float darkParts = Mathf.Lerp(minDarkParts, maxDarkParts, progress) * pieces / 4f;

        Rotater rotaterComp = FindAnyObjectByType<Rotater>();
        rotaterComp.speed = Mathf.Lerp(minRotateSpeed, maxRotateSpeed, progress);
        Transform rotater = rotaterComp.transform;

        for (i = 0; i > -length - addOn; i -= 0.5f)
        {
            // The first platform is always safe so the opening smash can't kill instantly
            temp1 = Instantiate(blocks[i == 0 ? 0 : PickBlock(darkParts)]);

            temp1.transform.position = new Vector3(0, i - 0.01f, 0);
            temp1.transform.eulerAngles = new Vector3(0, i * 8, 0);

            if (Mathf.Abs(i) >= length * 0.3f && Mathf.Abs(i) <= length * 0.6f)
            {
                temp1.transform.eulerAngles = new Vector3(0, i * 8, 0);
                temp1.transform.eulerAngles += Vector3.up * 180;
            }
            else if (Mathf.Abs(i) >= length * 0.8f)
            {
                temp1.transform.eulerAngles = new Vector3(0, i * 8, 0);

                if (random > 0.75f)
                    temp1.transform.eulerAngles += Vector3.up * 180;

            }
            temp1.transform.parent = rotater;

        }

        temp2 = Instantiate(WinPrefab);
        temp2.transform.position = new Vector3(0, i - 0.01f, 0);

        SwapMaterial(plateAsset, plateMat);
        SwapMaterial(baseAsset, baseMat);
    }

    // blocks[n] has n dark parts. Mixes the two nearest versions so the average equals darkParts.
    int PickBlock(float darkParts)
    {
        int low = Mathf.FloorToInt(darkParts);
        int index = Random.value < darkParts - low ? low + 1 : low;
        return Mathf.Clamp(index, 0, blocks.Length - 1);
    }

    void SwapMaterial(Material from, Material to)
    {
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Material[] mats = r.sharedMaterials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == from)
                {
                    mats[m] = to;
                    changed = true;
                }
            }
            if (changed)
                r.sharedMaterials = mats;
        }
    }

    void Start()
    {
        ui = FindAnyObjectByType<UI>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            plateMat.color = Random.ColorHSV(0, 1, 0.5f, 1, 1, 1);
            baseMat.color = plateMat.color + Color.gray;
            ballMesh.material.color = plateMat.color;
            ui.SetColor(plateMat.color);
        }

    }

    void BlockSelection()
    {
        // blockPrefabs holds sets of 4 (Circle, Flower, Hex, Spikes, Square)
        int randomBlock = Random.Range(0, blockPrefabs.Length / blocks.Length);


        switch (randomBlock)
        {
            case 0:
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i] = blockPrefabs[i];
                break;
            case 1:
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i] = blockPrefabs[i + 4];
                break;
            case 2:
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i] = blockPrefabs[i + 8];
                break;
            case 3:
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i] = blockPrefabs[i + 12];
                break;
            case 4:
                for (int i = 0; i < blocks.Length; i++)
                    blocks[i] = blockPrefabs[i + 16];
                break;

        }
    }

    public void NextLevel()
    {
        if (IsFinalLevel)
        {
            // Game beaten: start a new run from level 1. The high score is kept.
            PlayerPrefs.SetInt("Level", 1);
            ScoreManager.instance.ResetScore();
        }
        else
        {
            PlayerPrefs.SetInt("Level", level + 1);
        }
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }
}
