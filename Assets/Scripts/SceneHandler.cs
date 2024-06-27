using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public GameObject slider;
    public Image Filling;
    public string scene_tag;

    void Start()
    {
        slider.SetActive(false);
    }

IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene_tag);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            Filling.fillAmount = progressValue;
            yield return null;
        }
    }

    public void LoadScene(string scene)
    {
        scene_tag = scene;
        slider.SetActive(true);
        StartCoroutine(LoadSceneAsync());
    }
}
