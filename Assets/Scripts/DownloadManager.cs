using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Firebase.Storage;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class DownloadManager : MonoBehaviour
{
    [SerializeField] GameObject downloadBarTile;
    [SerializeField] Transform downloadBarTrans;

    string rootUri;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadLoadTime(FirebaseManager.instance.LoadTimeUrl));
    }

    IEnumerator LoadLoadTime(string _url)
    {
        UnityWebRequest request = UnityWebRequest.Get(_url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Root root = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            //rootUri = root.RootUri;
            foreach (Package package in root.Packages)
            {
                string bookurl = rootUri + "packs/" + package.Id + ".zip";
                StartCoroutine(DownloadContent(bookurl, package.Title, package.Id));
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    IEnumerator DownloadContent(string _url, string title, string _id)
    {
        GameObject go = Instantiate(downloadBarTile, downloadBarTrans);
        go.GetComponent<DownloadBarTile>().titleText.text = title;
        go.GetComponent<DownloadBarTile>().pauseButton.SetActive(true);
        UnityWebRequest request = UnityWebRequest.Get(_url);

        while (!request.isDone)
        {
            Debug.Log(request.downloadProgress);
            go.GetComponent<DownloadBarTile>().fillBar.fillAmount = request.downloadProgress;
            yield return new WaitForSeconds(0.2f);
        }

        yield return request.SendWebRequest();
        Debug.LogError(request.result);
        if (request.result == UnityWebRequest.Result.Success)
        {
            string savePath = string.Format("{0}/{1}.zip", Application.persistentDataPath, _id);
            System.IO.File.WriteAllBytes(savePath, request.downloadHandler.data);
            Debug.LogError(savePath);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public void TapBack()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public class Package
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }

    public class Root
    {
        public string RootUri { get; set; }
        public List<Package> Packages { get; set; }
    }
}
