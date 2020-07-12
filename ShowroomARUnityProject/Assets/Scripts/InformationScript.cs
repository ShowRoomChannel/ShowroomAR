using UnityEngine;

public class InformationScript : MonoBehaviour
{
    public GameObject vehiclePrefab;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
