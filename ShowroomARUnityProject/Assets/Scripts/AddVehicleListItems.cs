using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddVehicleListItems : MonoBehaviour
{
    public GameObject[] vehicleItems; 
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject obj in vehicleItems)
        {
            var vehObj = Instantiate(obj);
            vehObj.transform.parent = this.transform;
        }
    }
}
