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
    public GameObject Planes;
    public TargetSpawner targetSpawner;
    public GameObject PointCloud;

    private DetectionFacts detectionFacts;


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
        Planes.SetActive(false);
        targetSpawner.SetVisibleMeshes(false);
        PointCloud.SetActive(false);

        yield return null;

        detectionFacts = new DetectionFacts();
        detectionFacts.pixelWidth = Camera.main.pixelWidth;
        detectionFacts.pixelHeight = Camera.main.pixelHeight;
        detectionFacts.nearClipPlane = Camera.main.nearClipPlane;
        detectionFacts.worldToScreen = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;

        lastPosition = Camera.main.transform.position;
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        var resized = Resize(tex, tex.width / 4, tex.height / 4);
        //tex.Resize(tex.width / 4, tex.height / 4);
        //tex.Apply();

        var bytes = resized.EncodeToPNG();
        //var base64 = Convert.ToBase64String(bytes);
        StartCoroutine(SendPostRequestBytes(bytes));

        yield return null;
        UI.SetActive(true);
        Planes.SetActive(true);
        targetSpawner.SetVisibleMeshes(true);
        PointCloud.SetActive(true);
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

            var Left1 = new Vector3(json.points.left[0][0] * 4, detectionFacts.pixelHeight - json.points.left[0][1] * 4, 0);
            var Left2 = new Vector3(json.points.left[1][0] * 4, detectionFacts.pixelHeight - json.points.left[1][1] * 4, 0);
            var Right1 = new Vector3(json.points.right[0][0] * 4, detectionFacts.pixelHeight - json.points.right[0][1] * 4, 0);
            var Right2 = new Vector3(json.points.right[1][0] * 4, detectionFacts.pixelHeight - json.points.right[1][1] * 4, 0);

            var l1 = targetSpawner.CreateTrackAnchorRaycast(ScreenPointToRay(Left1));
            var l2 = targetSpawner.CreateTrackAnchorRaycast(ScreenPointToRay(Left2));
            Debug.DrawLine(l1.position, l2.position, Color.green, 20f);

            var r1 = targetSpawner.CreateTrackAnchorRaycast(ScreenPointToRay(Right1));
            var r2 = targetSpawner.CreateTrackAnchorRaycast(ScreenPointToRay(Right2));
            Debug.DrawLine(r1.position, r2.position, Color.red, 20f);
        }
    }

    public Ray ScreenPointToRay(Vector3 sp)
    {
        var v0 = manualScreenPointToWorld(detectionFacts.worldToScreen, detectionFacts.pixelWidth,
                detectionFacts.pixelHeight, 0, sp);
        var v1 = manualScreenPointToWorld(detectionFacts.worldToScreen, detectionFacts.pixelWidth,
                detectionFacts.pixelHeight, 1, sp);

        var ray = new Ray(v0, (v1-v0).normalized);
        return ray;
    }

    Vector3 manualScreenPointToWorld(Matrix4x4 world2Screen, int pixelWidth, int pixelHeight, float nearClipPlane, Vector3 sp)
    {
        Matrix4x4 screen2World = world2Screen.inverse;

        float[] inn = new float[4];

        inn[0] = 2.0f * (sp.x / pixelWidth) - 1.0f;
        inn[1] = 2.0f * (sp.y / pixelHeight) - 1.0f;
        inn[2] = nearClipPlane;
        inn[3] = 1.0f;

        Vector4 pos = screen2World * new Vector4(inn[0], inn[1], inn[2], inn[3]);

        pos.w = 1.0f / pos.w;

        pos.x *= pos.w;
        pos.y *= pos.w;
        pos.z *= pos.w;

        return new Vector3(pos.x, pos.y, pos.z);
    }

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
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

        public int pixelWidth;
        public int pixelHeight;
        public float nearClipPlane;

        public Matrix4x4 worldToScreen;

        public Vector3 cameraPosition;
        public Quaternion cameraRotation;

    }
}
