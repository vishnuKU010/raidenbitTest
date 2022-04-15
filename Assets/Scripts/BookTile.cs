using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BookTile : MonoBehaviour
{
    public Text bookName;
    public string bookID;
    public string bookCategory;
    public string bookPosterUrl;
    public RawImage posterImage;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadPoster());
    }

    IEnumerator LoadPoster()
    {
        yield return new WaitForEndOfFrame();
        using (UnityWebRequest request = UnityWebRequest.Get(bookPosterUrl))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                tex.LoadImage(request.downloadHandler.data);
                posterImage.texture = tex;
            }
            else
            {
                Debug.LogError("error");
            }
        }
        
    }

    public void TapPoster()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
