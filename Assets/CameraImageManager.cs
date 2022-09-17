using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraImageManager : MonoBehaviour
{
    private string url = "http://34.83.254.195/detect";
    //private string url = "https://httpbin.org/post";

    private string lastMessageReceived = "";
    public TMP_Text text;

    public GameObject UI;

    Vector3 lastPosition;

    public void createScreenshot()
    {
        //StartCoroutine(AsyncScreenshot());

        StartCoroutine(CreateScrenshotWithoutUI());

        //lastPosition = Camera.main.transform.position;
        //var tex = ScreenCapture.CaptureScreenshotAsTexture();
        //var bytes = tex.EncodeToPNG();
        //var base64 = Convert.ToBase64String(bytes);
        //StartCoroutine(SendPostRequest(base64));
    }

    //private RenderTexture renderTexture;

    //IEnumerator TextureScreenshot()
    //{
    //    yield return new WaitForEndOfFrame();

    //    renderTexture = new RenderTexture(Screen.width/4, Screen.height/4, 0);
    //    ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture);
    //    AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, ReadbackCompleted);
    //}

    //void ReadbackCompleted(AsyncGPUReadbackRequest request)
    //{
    //    // Render texture no longer needed, it has been read back.
    //    DestroyImmediate(renderTexture);

    //    using (var imageBytes = request.GetData<byte>())
    //    {
    //        var base64 = Convert.ToBase64String(imageBytes.ToArray());
    //        StartCoroutine(SendPostRequest(base64));
    //    }
    //}

    public IEnumerator CreateScrenshotWithoutUI()
    {
        UI.SetActive(false);
        yield return null;

        lastPosition = Camera.main.transform.position;
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        tex.Resize(tex.width / 4, tex.height / 4);
        tex.Apply();

        var bytes = tex.EncodeToPNG();
        //var base64 = Convert.ToBase64String(bytes);
        StartCoroutine(SendPostRequestBytes(bytes));

        yield return null;
        UI.SetActive(true);
    }

    public IEnumerator SendPostRequestBytes(byte[] imgBytes)
    {
        text.text = "Sending Request...";
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("image64=" + img64));
        //formData.Add(new MultipartFormDataSection("test=" + "test123"));
        formData.Add(new MultipartFormFileSection("image", imgBytes, "image.png", "image/png"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            text.text = www.error;
        }
        else
        {
            Debug.Log("Form upload complete!");
            text.text = "Form upload complete!";
        }

        while (!www.downloadHandler.isDone)
        {
            yield return null;
        }

        lastMessageReceived = www.downloadHandler.text;
        text.text = "Received: " + lastMessageReceived;

        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<DetectionResponse>(lastMessageReceived);
        if (!json.points.tracks)
        {
            text.text = "No tracks found";
        }
        else
        {
            text.text = "Tracks found!";
        }
    }


    public IEnumerator SendPostRequest(string img64)
    {
        text.text = "Sending Request...";
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("image64=" + img64));
        //formData.Add(new MultipartFormDataSection("test=" + "test123"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            text.text = www.error;
        }
        else
        {
            Debug.Log("Form upload complete!");
            text.text = "Form upload complete!";
        }

        while (!www.downloadHandler.isDone)
        {
            yield return null;
        }

        lastMessageReceived = www.downloadHandler.text;
        text.text = "Received: " + lastMessageReceived.Length;

    }

    public class DetectionResponse
    {
        public Points points;
    }

    public class Points
    {
        public bool tracks;

        public int[][] left;
        public int[][] right;
    }

    public class DetectionFacts
    {
        public int imgWidth;
        public int imgHeight;

        public Vector3 cameraPosition;
        public Quaternion cameraRotation;

    }
}
