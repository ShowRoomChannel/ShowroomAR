using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ConventionalSceneController : MonoBehaviour
{
    private GameObject _vehiclePrefab;
    public GameObject vehicleObject;
    private GameObject _vehicleInstance;
    public GameObject vehicleNameObject;
    public GameObject vehicleDetailsObject;
    public GameObject vehicleAboutObject;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        if (GameObject.Find("SceneManager") != null)
        {
            Debug.Log("Found SceneManager");
        }
        _vehiclePrefab = FindObjectOfType<InformationScript>().vehiclePrefab;
    }

    // Start is called before the first frame update
    void Start()
    {
        _vehiclePrefab = FindObjectOfType<InformationScript>().vehiclePrefab;

        displayVehicle();
        vehicleNameObject.GetComponent<Text>().text = _vehicleInstance.GetComponent<VehiclePrefabInfo>().vehicleName.text;
        vehicleDetailsObject.GetComponent<Text>().text = _vehicleInstance.GetComponent<VehiclePrefabInfo>().vehicleDetails.text;
        vehicleAboutObject.GetComponent<TextMeshProUGUI>().text = _vehicleInstance.GetComponent<VehiclePrefabInfo>().vehicleAbout.text;
    }

    void Update()
    {
        float rotationSpeed = 24f * Time.deltaTime;
        vehicleObject.transform.Rotate(0, rotationSpeed, 0);
    }

    private void displayVehicle()
    {
        // Display vehicle prefab
        //Vector3 startingPosition = new Vector3(-0.15f, 2f, 0f);
        //Quaternion startingRotation = Quaternion.Euler(20f, 135f, -20f);
        _vehicleInstance = Instantiate(_vehiclePrefab, Vector3.zero, Quaternion.identity);
        _vehicleInstance.GetComponent<Rigidbody>().useGravity = false;
        _vehicleInstance.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        _vehicleInstance.transform.parent = vehicleObject.transform;
        _vehicleInstance.GetComponent<InputManager>().enabled = false;
        vehicleObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}
