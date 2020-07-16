using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class CaptureScreen : MonoBehaviour
{
    // For taking a picture
    public GameObject UIButtons;
    public GameObject pointCloud;

    public void CaptureIt()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
            string fileName = "Photo-" + timeStamp + ".png";
            string pathToSave = fileName;
            ScreenCapture.CaptureScreenshot(pathToSave, 1);
            StartCoroutine(CaptureScreenCoroutine());
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

    public IEnumerator CaptureScreenCoroutine()
    {
        // Wait till the last possible moment before screen rendering to hide the UI
        yield return null;
        //UIButtons.GetComponent<Canvas>().enabled = false;
        UIButtons.SetActive(false);
        pointCloud.SetActive(false);

        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        // Save the screenshot to Gallery/Photos
        Debug.Log("Permission result: " + NativeGallery.SaveImageToGallery(ss, "GalleryTest", "Image.png"));

        // To avoid memory leaks
        Destroy(ss);
        UIButtons.SetActive(true);
        pointCloud.SetActive(true);
    }

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
