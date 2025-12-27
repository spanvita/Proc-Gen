using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class clickValue
{
    public string asset_name;
}


public class Backend_sender : MonoBehaviour
{
    IEnumerator SendClickToBackend(clickValue payload)
    {
        string url = "http://localhost:3000/assetvalues"; 

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

    void onGetData(string asset_name)
    {
        clickValue payload = new clickValue 
        {
            asset_name = asset_name
        };

        StartCoroutine(SendClickToBackend(payload));
    }
}