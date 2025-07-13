using Newtonsoft.Json;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebCommManager : MonoBehaviour
{
    private static WebCommManager _instance;
    public static WebCommManager Instance { get { return _instance; } }

    public GameObject Canvas;
    public InputField IDInputField;
    public InputField PWInputField;
    public Button OKBtn;

    public class UserData
    {
        public string ID;
        public string PW;
        public string Salt;
        public DateTime Created;
        public DateTime? LastLogin;
    }

    string _baseUrl = "https://localhost:5001/api";

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            Init();
            DontDestroyOnLoad(gameObject);
        }
    }

    void Init()
    {

    }

    public void SendPostRequest(string url, object obj, Action<UnityWebRequest> callback)
    {
        StartCoroutine(CoSendWebRequest(url, "POST", obj, callback));
    }

    public void SendGetAllRequest(string url, Action<UnityWebRequest> callback)
    {
        StartCoroutine(CoSendWebRequest(url, "GET", null, callback));
    }


    IEnumerator CoSendWebRequest(string url, string method, object obj, Action<UnityWebRequest> callback)
    {
        string sendUrl = $"{_baseUrl}/{url}/";

        byte[] jsonBytes = null;

        if (obj != null)
        {
            string jsonStr = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                NullValueHandling = NullValueHandling.Ignore
            });
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        var uwr = new UnityWebRequest(sendUrl, method);
        uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Debug.Log("Recv " + uwr.downloadHandler.text);
            callback.Invoke(uwr);
        }
    }

    public void OnCheckButtonClicked()
    {
        UserData res = new UserData()
        {
            ID = IDInputField.text,
            PW = PWInputField.text,
            Salt = "###Temp###",
            Created = DateTime.UtcNow,
            LastLogin = null
        };

        string Destination = "UserTable";
        SendPostRequest(Destination, res, (uwr) =>
        {
            Canvas.SetActive(false);
        });

        Debug.Log($"ID : {IDInputField.text}, PW : {PWInputField.text}");
    }
}
