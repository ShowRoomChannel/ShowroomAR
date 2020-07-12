using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagerScript : MonoBehaviour
{
    public Slider slider;
    public GameObject infoObject;

    public void LoadListingScene(GameObject prefab)
    {
        FindObjectOfType<InformationScript>().vehiclePrefab = prefab;
        SceneManager.UnloadSceneAsync(0);
        infoObject.GetComponent<InformationScript>().vehiclePrefab = prefab;
        SceneManager.LoadScene(1);
    }

    public void LoadARCoreScene()
    {
        StartCoroutine(LoadAsynchronously());
    }

    IEnumerator LoadAsynchronously ()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(2);
        while (operation.isDone == false)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;
            Debug.Log(progress);

            yield return null;
        }
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                int buildIndex = SceneManager.GetActiveScene().buildIndex;
                if (buildIndex == 0)
                {
                    Application.Quit();
                }
                else if (buildIndex == 1)
                {
                    SceneManager.UnloadSceneAsync(1);
                    SceneManager.LoadScene(0);
                }
                else
                {
                    SceneManager.LoadScene(1);
                }
            }
        }
    }
}
