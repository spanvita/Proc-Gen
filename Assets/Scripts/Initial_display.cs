using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro; 


public class Initial_display : MonoBehaviour
{
    public TextMeshProUGUI popupText;
    public MapGenerator mapGen;
    public float popupDuration = 5f;
    public float hideTime = 0f;
    void Start()
    {
        StartCoroutine(GetData());
        
    }

    IEnumerator GetData()
    {
        string url = "http://localhost:3000/data"; // your backend

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.LogError("Received: " + json);

            // Convert JSON → C# object
            Data data = JsonUtility.FromJson<Data>(json);
            Debug.LogError($"stateId={data.stateId}, stateName={data.stateName}");
            popupText.text = $"State: {data.stateName}";
            
            Debug.LogError(popupText.text);



        }
    }
    [System.Serializable]
    public class Data
    {
        public int stateId;
        public string stateName;
    }
}

