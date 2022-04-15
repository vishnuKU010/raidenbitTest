using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using System;

public class FirebaseManager : MonoBehaviour
{
    #region Singleton
    private static FirebaseManager _instance;
    public static FirebaseManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<FirebaseManager>();
            }
            return _instance;
        }
    }
    #endregion

    public bool isLoadData = false;
    public string LoadTimeUrl;
    public string BookshelfUrl;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
        // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        //// [START set_defaults]
        //System.Collections.Generic.Dictionary<string, object> defaults =
        //  new System.Collections.Generic.Dictionary<string, object>();

        //// These are the values that are used if we haven't fetched data from the
        //// server
        //// yet, or if we ask for values that the server doesn't have:
        //defaults.Add("config_test_string", "default local string");
        //defaults.Add("config_test_int", 1);
        //defaults.Add("config_test_float", 1.0);
        //defaults.Add("config_test_bool", false);

        //Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
        //  .ContinueWithOnMainThread(task =>
        //  {
        //      // [END set_defaults]
        //      Debug.Log("RemoteConfig configured and ready!");
        //      isFirebaseInitialized = true;
        //  });
        FetchDataAsync();
    }

    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        System.Threading.Tasks.Task fetchTask =
        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            Debug.Log("Fetch canceled.");
        }
        else if (fetchTask.IsFaulted)
        {
            Debug.Log("Fetch encountered an error.");
        }
        else if (fetchTask.IsCompleted)
        {
            Debug.Log("Fetch completed successfully!");
        }

        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;

        switch (info.LastFetchStatus)
        {
            case Firebase.RemoteConfig.LastFetchStatus.Success:
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
          .ContinueWithOnMainThread(task => {
              Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                                   info.FetchTime));
          });

                Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
                BookshelfUrl = FirebaseRemoteConfig.DefaultInstance.GetValue("BookshelfUrl").StringValue;
                Debug.Log("BookshelfUrl: " + BookshelfUrl);
                LoadTimeUrl = FirebaseRemoteConfig.DefaultInstance.GetValue("LoadTimeUrl").StringValue;
                Debug.Log("LoadTimeUrl: " + LoadTimeUrl);
                UIController.instance.LoadBookData(BookshelfUrl);
                isLoadData = true;
                // Also tried this way, but then it doesn't enter the IF block
                /*if (FirebaseRemoteConfig.ActivateFetched())
                { 
                     Debug.Log(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));

                     string stop = FirebaseRemoteConfig.GetValue("stops").StringValue;
                     Debug.Log("Value: " + (string.IsNullOrEmpty(stop) ? "NA" : stop));
                }*/
                break;
            case LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case FetchFailureReason.Error:
                        Debug.Log("Fetch failed for unknown reason");
                        break;
                    case FetchFailureReason.Throttled:
                        Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                        break;
                }
                break;
            case LastFetchStatus.Pending:
                Debug.Log("Latest Fetch call still pending.");
                break;
        }
    }
}
