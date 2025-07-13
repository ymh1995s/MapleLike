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

        // 이하 콜백은 아니고 코루틴
        if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(uwr.error);
            yield break;
        }
        else
        {
            Debug.Log("Recv " + uwr.downloadHandler.text);
            callback.Invoke(uwr);
        }
    }

    // 서버에서 응답한 메시지를 JSON으로 변환용
    [Serializable]
    public class ServerResponse
    {
        // 웹서버가 Message = "###", ID = ### 꼴로 주기 때문에 아래처럼 2개로 구성됨
        public string Message;
        public string ID;
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
            var json = uwr.downloadHandler.text;
            var response = JsonUtility.FromJson<ServerResponse>(json);

            if (response == null)
            {
                Debug.LogError("서버 응답 파싱 실패");
                return;
            }

            if (response.Message == "RegisterSuccess")
            {
                Debug.Log("회원가입 성공: " + response.ID);
                // 회원가입 성공 처리
            }
            else if (response.Message == "LoginSuccess")
            {
                Debug.Log("로그인 성공: " + response.ID);
                // 로그인 성공 처리
            }
            else
            {
                Debug.LogWarning("알 수 없는 메시지: " + response.Message);
            }

            //Canvas.SetActive(false);
        });

        Debug.Log($"ID : {IDInputField.text}, PW : {PWInputField.text}");
    }
}
