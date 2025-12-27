using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class value_to_be_sent
{
    public int[,] stateIds;          // grid → stateId
    public Vector2Int[] capitals;    // index = stateId, value = (x,y)

}


public class MapDataSender : MonoBehaviour
{
    public MapGenerator mapGen;
    IEnumerator SendClickToBackend(value_to_be_sent payload)
    {
        string url = "http://localhost:3000/mapData"; 

        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("POST failed: " + request.error);
        }
        else
        {
            Debug.Log("POST success: " + request.downloadHandler.text);
        }
    }

    void Awake()
    {
        
        value_to_be_sent payload = new value_to_be_sent 
        {
            stateIds=mapGen.stateIds,
            capitals=mapGen.capitals
        };

        StartCoroutine(SendClickToBackend(payload));
    }
}