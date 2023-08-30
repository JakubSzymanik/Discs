using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitor : MonoBehaviour {

    [SerializeField] private float animationSpeed;

    //reference
    [SerializeField] private Image loadingImage;

    public IEnumerator LoadSceneAsync(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;
        // Wait until the asynchronous scene fully loads and screen turns black
        Time.timeScale = 1;
        while (!asyncLoad.isDone)
        {
            loadingImage.color = Color.black * asyncLoad.progress;
            yield return null;
        }
    }

    public IEnumerator SceneLoadedAnimation()
    {
        while(loadingImage.color != Color.clear)
        {
            //loadingImage.color -= Color.black * Time.unscaledDeltaTime * animationSpeed;
            loadingImage.color = Color.black * Mathf.Clamp(loadingImage.color.a - Time.unscaledDeltaTime * animationSpeed, 0, 1);
            yield return null;
        }
        this.gameObject.SetActive(false);
    }
}
