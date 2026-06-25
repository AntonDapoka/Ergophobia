using UnityEngine;

public class BlocksReferenceScript : MonoBehaviour
{
    [SerializeField] private GameObject[] blocks;

    public GameObject[] GetBlocks()
    {
        return blocks;
    }
}
