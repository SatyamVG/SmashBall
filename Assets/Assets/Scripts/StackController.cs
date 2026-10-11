using System.Collections;
using UnityEngine;

public class StackController : MonoBehaviour
{
    [SerializeField]
    private StackPartController[] stackPartControlls = null;
    private bool shattered;

    public void ShatterAllParts()
    {
        // The ball can touch two parts of the same stack in one physics step
        if (shattered)
            return;
        shattered = true;

        if(transform.parent != null)
        {
            transform.parent = null;
            FindAnyObjectByType<Ball>().IncreaseBrokenStacks();
        }

        foreach (StackPartController part in stackPartControlls)
        {
            part.Shatter();
        }
        StartCoroutine(RemoveParts()) ;
    }

    IEnumerator RemoveParts()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
