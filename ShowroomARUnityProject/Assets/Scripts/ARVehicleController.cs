namespace GoogleARCore.Examples.ObjectManipulation
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.Android;

    public class ARVehicleController : MonoBehaviour
    {
        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;
        public GameObject placementIndicator;
        public GameObject VehicleObject;
        public GameObject VehiclePrefab;
        private GameObject VehicleInstance;
        private GameObject placementIndicatorInstance;
        private bool isClicked;

        // To move the vehicle
        private bool isMove;
        private bool isAdded;

        // For expanding button panel when add button is clicked
        public GameObject addButtonGameObject;
        public GameObject buttonPanel;

        private bool _isVehicleActive;
        private bool _isPlacementIndicatorActive;

        // For vehicle driving behaviour
        public GameObject joystickObject;
        private Joystick joystick;
        public GameObject vehiclePlane;

        // For translucent vehicle preview
        private Dictionary<MeshRenderer, Material[]> _materialDictionary = new Dictionary<MeshRenderer, Material[]>();

        public void ResetButton()
        {
            isClicked = true;

            placementIndicator.SetActive(true);
            SetPlacementIndicator();
            addButtonGameObject.SetActive(true);
            buttonPanel.SetActive(false);
        }

        public void SetClicked()
        {
            isClicked = true;
            if (_isPlacementIndicatorActive && !_isVehicleActive)
            {
                addButtonGameObject.SetActive(false);
                buttonPanel.SetActive(true);
            }
        }

        public void onPress()
        {
            isMove = true;
        }

        public void onRelease()
        {
            isMove = false;
        }

        private void SetPlacementIndicator()
        {
            if (placementIndicatorInstance != null)
            {
                DestroyImmediate(placementIndicatorInstance);
            }
            joystickObject.SetActive(false);
            placementIndicatorInstance = Instantiate(VehiclePrefab, placementIndicator.transform.position, placementIndicator.transform.rotation);
            placementIndicatorInstance.transform.Rotate(0, 90, 0);
            placementIndicatorInstance.GetComponent<Rigidbody>().useGravity = false;
            placementIndicatorInstance.GetComponent<InputManager>().joystick = joystick;
            placementIndicatorInstance.transform.parent = placementIndicator.transform;
            placementIndicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            placementIndicatorInstance.GetComponent<TurnTranslucentThenBack>().Start();
            _materialDictionary = placementIndicatorInstance.GetComponent<TurnTranslucentThenBack>().materialDictionary;
            placementIndicatorInstance.GetComponent<TurnTranslucentThenBack>().TurnTranslucent();
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds;
            if (isClicked && (isAdded == true))
            {
                isAdded = false;
                VehicleObject.SetActive(false);
                SetClicked();
            }
            else if (Frame.Raycast(Screen.width / 2, Screen.height / 2, raycastFilter, out hit))
            {

                Vector3 pt = hit.Pose.position;
                Vector3 planePt = pt;
                planePt.y -= 0.05f;
                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                var rotation = Quaternion.LookRotation(cameraBearing);

                if (isMove && isAdded)
                {
                    var sameRotation = VehicleObject.transform.rotation;
                    VehicleInstance.transform.position = pt;
                    VehicleInstance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    VehicleInstance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    VehicleObject.transform.position = pt;
                    VehicleObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    VehicleObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    vehiclePlane.transform.position = planePt;
                }
                if (isAdded == false)
                {
                    placementIndicator.transform.SetPositionAndRotation(pt, rotation);
                    placementIndicator.SetActive(true);
                    _isPlacementIndicatorActive = true;

                    if (isClicked)
                    {
                        if (VehicleInstance != null)
                        {
                            DestroyImmediate(VehicleInstance);
                        }
                        VehicleObject.transform.SetPositionAndRotation(pt, rotation);
                        joystick.enabled = true;
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                        VehicleObject.transform.parent = anchor.transform;
                        VehicleObject.SetActive(true);
                        VehicleInstance = placementIndicatorInstance;
                        VehicleInstance.GetComponent<Rigidbody>().useGravity = true;
                        VehicleInstance.transform.parent = VehicleObject.transform;
                        joystickObject.SetActive(true);
                        VehicleInstance.GetComponent<InputManager>().joystick = joystick;
                        VehicleInstance.GetComponent<TurnTranslucentThenBack>().materialDictionary = _materialDictionary;
                        VehicleInstance.GetComponent<TurnTranslucentThenBack>().TurnBack();
                        VehicleInstance.AddComponent<Lean.Touch.LeanTwistRotateAxis>();
                        VehicleInstance.AddComponent<Lean.Touch.LeanPinchScale>();
                        vehiclePlane.transform.SetPositionAndRotation(planePt, rotation);
                        isAdded = true;
                        SetClicked();
                        placementIndicator.SetActive(false);
                        _isPlacementIndicatorActive = false;
                    }
                }
            }
            else
            {
                placementIndicator.SetActive(false);
                _isPlacementIndicatorActive = false;
            }
            _UpdateApplicationLifecycle();
            isClicked = false;
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            VehiclePrefab = FindObjectOfType<InformationScript>().vehiclePrefab;

            joystick = joystickObject.GetComponent<Joystick>();

            _isVehicleActive = false;

            SetPlacementIndicator();

            addButtonGameObject.SetActive(true);
            buttonPanel.SetActive(false);

            // Enable ARCore to target 30fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            Application.targetFrameRate = 30;
            VehicleObject.SetActive(false);
            isClicked = false;
            isAdded = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}