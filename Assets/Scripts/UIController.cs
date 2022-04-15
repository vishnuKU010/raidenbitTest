using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class UIController : MonoBehaviour
{
    #region Singleton
    private static UIController _instance;
    public static UIController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UIController>();
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] GameObject shelfRawTile;
    [SerializeField] GameObject bookTile;
    [SerializeField] Transform shelfRawTrans;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (FirebaseManager.instance.isLoadData)
        {
            LoadBookData(FirebaseManager.instance.BookshelfUrl);
        }
    }

    public void LoadBookData(string url)
    {
        StartCoroutine(LoadBookShelf(url));
    }

    IEnumerator LoadBookShelf(string _url)
    {
        UnityWebRequest request = UnityWebRequest.Get(_url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Root root = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            foreach(Category category in root.Categories)
            {
                GameObject go = Instantiate(shelfRawTile, shelfRawTrans);
                go.GetComponent<ShelfTile>().categoryName.text = category.Title.en;
                go.GetComponent<ShelfTile>().categoryID = category.Id;
                foreach (Story story in root.Stories)
                {
                    foreach(string storyCate in story.Categories)
                    {
                        if (storyCate == category.Id)
                        {
                            GameObject go2 = Instantiate(bookTile, go.GetComponent<ShelfTile>().bookTrans);
                            go2.GetComponent<BookTile>().bookName.text = story.Title.en;
                            go2.GetComponent<BookTile>().bookID = story.Id;
                            go2.GetComponent<BookTile>().bookCategory = category.Id;
                            go2.GetComponent<BookTile>().bookPosterUrl = "https://storage.googleapis.com/kbp/content/posters/"+story.Id+"/bookshelf.png";

                            go2.SetActive(true);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public class Title
    {
        public string en { get; set; }
        public string hi { get; set; }
    }

    public class Category
    {
        public string Id { get; set; }
        public Title Title { get; set; }
    }

    public class Story
    {
        public string Id { get; set; }
        public Title Title { get; set; }
        public List<string> Categories { get; set; }
    }

    public class Root
    {
        public List<Category> Categories { get; set; }
        public List<Story> Stories { get; set; }
    }
}
