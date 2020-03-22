using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ConvSceneController : MonoBehaviour
{
    public AssetReference remoteVehicle;
    public GameObject vehicleObject;
    // Start is called before the first frame update
    void Start()
    {
        displayVehicle();
    }

    void Update()
    {
        
    }

    private void displayVehicle()
    {
        // Display remote prefab
        remoteVehicle.InstantiateAsync(Vector3.zero, Quaternion.identity);
        //vehicleObject.transform.parent = remoteVehicle.transform;
    }
}
