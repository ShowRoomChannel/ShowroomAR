//-----------------------------------------------------------------------
// <copyright file="ObjectManipulationController.cs" company="Google">
//
// Copyright 2018 Google LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.ObjectManipulation
{
    using System.Collections;
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using System;
    using UnityEngine.UI;
    using UnityEngine.Android;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
    using Lean.Touch;
#endif

    /// <summary>
    /// Controls the HelloObjectManipulation example.
    /// </summary>
    public class ObjectManipulationController : MonoBehaviour
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
        public Button addButton;
        private bool isClicked;

        // To change Add/Reset button image
        private bool isAdded;
        public Sprite addSprite;
        public Sprite resetSprite;

        // To move the vehicle
        private bool isMove;

        // For taking a picture
        public GameObject UIButtons;
        public GameObject pointCloud;

        // For expanding button panel when add button is clicked
        public GameObject addButtonGameObject;
        public GameObject buttonPanel;
        public GameObject[] buttonPanelGameObjects;

        private bool _isVehicleActive;
        private bool _isPlacementIndicatorActive;

        public GameObject joystickObject;
        private Joystick joystick;

        public GameObject vehiclePlane;

        private Dictionary<MeshRenderer, Material[]> _materialDictionary = new Dictionary<MeshRenderer, Material[]>();

        public void CaptureIt()
        {
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
                string fileName = "Photo-" + timeStamp + ".png";
                string pathToSave = fileName;
                ScreenCapture.CaptureScreenshot(pathToSave, 1);
                StartCoroutine(CaptureScreen());
                _ShowAndroidToastMessage("Picture taken, check your gallery.");
            }
            else
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                {
                    _ShowAndroidToastMessage("Picture cannot be taken without granting storage permission.");
                }
            }
        }

        public IEnumerator CaptureScreen()
        {
            /*
            // Wait till the last possible moment before screen rendering to hide the UI
            yield return null;
            //UIButtons.GetComponent<Canvas>().enabled = false;
            UIButtons.SetActive(false);
        
            // Wait for screen rendering to complete
            yield return new WaitForEndOfFrame();
        
            // Take screenshot
            string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
            string fileName = "Photo-" + timeStamp + ".png";
            string pathToSave = fileName;
            ScreenCapture.CaptureScreenshot(pathToSave, 1);
            
            // Save the screenshot to Gallery/Photos
	        Debug.Log( "Permission result: " + NativeGallery.SaveImageToGallery( ss, "GalleryTest", "Image.png" ) );
        
            // Show UI after we're done
            //UIButtons.GetComponent<Canvas>().enabled = true;
            UIButtons.SetActive(true);
            */
            // Wait till the last possible moment before screen rendering to hide the UI
            yield return null;
            //UIButtons.GetComponent<Canvas>().enabled = false;
            UIButtons.SetActive(false);
            pointCloud.SetActive(false);

            yield return new WaitForEndOfFrame();

            Texture2D ss = new Texture2D( Screen.width, Screen.height, TextureFormat.RGB24, false );
            ss.ReadPixels( new Rect( 0, 0, Screen.width, Screen.height ), 0, 0 );
            ss.Apply();

            // Save the screenshot to Gallery/Photos
            Debug.Log( "Permission result: " + NativeGallery.SaveImageToGallery( ss, "GalleryTest", "Image.png" ) );
            
            // To avoid memory leaks
            Destroy( ss );
            UIButtons.SetActive(true);
            pointCloud.SetActive(true);
        }

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
                foreach (GameObject buttonsInPanel in buttonPanelGameObjects)
                {
                    buttonsInPanel.SetActive(true);
                }
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
            if (VehicleInstance)
            {
                //vehiclePlane.transform.position = VehicleInstance.transform.position;
            }

            addButton.onClick.AddListener( () => isClicked = true );
            TrackableHit hit;
            //TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            //    TrackableHitFlags.FeaturePointWithSurfaceNormal; 
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds;
            //var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));    
            if (isClicked && (isAdded == true))
            {
                isAdded = false;
                VehicleObject.SetActive(false);
                SetClicked();
            }
            else if (Frame.Raycast (Screen.width/2, Screen.height/2, raycastFilter, out hit))
            {
                
                Vector3 pt = hit.Pose.position;
                Vector3 planePt = pt;
                planePt.y -= 0.05f;
                //pt.y += 0.1f;
                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                var rotation = Quaternion.LookRotation(cameraBearing);
                //var rotation = Quaternion.LookRotation(Vector3.zero);

                if (isMove && isAdded)
                {
                    var sameRotation = VehicleObject.transform.rotation;
                    VehicleInstance.transform.position = pt;
                    //VehicleInstance.transform.rotation = rotation;
                    VehicleInstance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    VehicleInstance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    VehicleObject.transform.position = pt;
                    //VehicleObject.transform.rotation = rotation;
                    VehicleObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    VehicleObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    vehiclePlane.transform.position = planePt;
                    //vehiclePlane.transform.rotation = rotation;
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
                            DestroyImmediate (VehicleInstance);
                        }
                        VehicleObject.transform.SetPositionAndRotation(pt, rotation);
                        joystick.enabled = true;
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                        VehicleObject.transform.parent = anchor.transform;
                        VehicleObject.SetActive(true);
                        //VehicleObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        //VehicleInstance = Instantiate(placementIndicatorInstance, placementIndicator.transform);
                        VehicleInstance = placementIndicatorInstance;
                        VehicleInstance.GetComponent<Rigidbody>().useGravity = true;
                        VehicleInstance.transform.parent = VehicleObject.transform;
                        joystickObject.SetActive(true);
                        VehicleInstance.GetComponent<InputManager>().joystick = joystick;
                        VehicleInstance.GetComponent<TurnTranslucentThenBack>().materialDictionary = _materialDictionary;
                        VehicleInstance.GetComponent<TurnTranslucentThenBack>().TurnBack();
                        VehicleInstance.AddComponent<Lean.Touch.LeanTwistRotateAxis>();
                        VehicleInstance.AddComponent<Lean.Touch.LeanPinchScale>();
                        //vehiclePlane.transform.SetPositionAndRotation(planePt, Quaternion.LookRotation(Vector3.zero));
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

        private void Start()
        {
            VehiclePrefab = FindObjectOfType<InformationScript>().vehiclePrefab;
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            VehiclePrefab = FindObjectOfType<InformationScript>().vehiclePrefab;

            joystick = joystickObject.GetComponent<Joystick>();

            _isVehicleActive = false;

            //placementIndicatorInstance = Instantiate(VehiclePrefab, placementIndicator.transform.position, placementIndicator.transform.rotation);
            SetPlacementIndicator();

            addButtonGameObject.SetActive(true);
            //RectTransform buttonPanelRectangle = buttonPanel.GetComponent<RectTransform>();
            buttonPanel.SetActive(false);
            //foreach (GameObject button in buttonPanel.GetComponents<GameObject>())
            //{
            //    button.SetActive(false);
            //}

            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
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
            /*
            else if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                _ShowAndroidToastMessage("Storage permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            */
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
