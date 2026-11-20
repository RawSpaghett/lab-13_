using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiKey = "ccff35769d4969c7b8a1ca7d8945c6ad";

    [Header("Scene References")]
    [SerializeField] private Light sun;
    
    [Header("Skyboxes")]
    [SerializeField] private Material sunnySkybox;
    [SerializeField] private Material cloudySkybox;
    [SerializeField] private Material stormySkybox;
    [SerializeField] private Material nightSkybox;

    [Header("Sun Settings")]
    [SerializeField] private Color daySunColor = new Color(1f, 0.95f, 0.84f);
    [SerializeField] private Color sunsetSunColor = new Color(1f, 0.55f, 0.25f);
    [SerializeField] private float maxSunIntensity = 1.2f;

    private string xmlApi;

    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    private void Start()
    {
        SetOrlando();
    }

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.certificateHandler = new BypassCertificate();
            request.disposeCertificateHandlerOnDispose = true;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"response error: {request.responseCode}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    private void OnXMLDataLoaded(string xmlData)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlData);

        XmlNode weatherNode = doc.SelectSingleNode("/current/weather");
        int weatherCode = int.Parse(weatherNode.Attributes["number"].Value);
        string weatherDesc = weatherNode.Attributes["value"].Value;

        XmlNode timezoneNode = doc.SelectSingleNode("/current/city/timezone");
        int timezoneOffsetSeconds = int.Parse(timezoneNode.InnerText);

        XmlNode sunNode = doc.SelectSingleNode("/current/city/sun");
        DateTime sunriseUtc = DateTime.Parse(sunNode.Attributes["rise"].Value, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
        DateTime sunsetUtc = DateTime.Parse(sunNode.Attributes["set"].Value, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);

        DateTime cityLocalTime = DateTime.UtcNow.AddSeconds(timezoneOffsetSeconds);
        DateTime citySunrise = sunriseUtc.AddSeconds(timezoneOffsetSeconds);
        DateTime citySunset = sunsetUtc.AddSeconds(timezoneOffsetSeconds);

        bool isDaytime = cityLocalTime >= citySunrise && cityLocalTime <= citySunset;

        Debug.Log($"Weather: {weatherDesc} ({weatherCode}) | City Time: {cityLocalTime:HH:mm} | Daytime: {isDaytime}");

        SetSkybox(weatherCode, isDaytime);
        SetSun(weatherCode, isDaytime, cityLocalTime, citySunrise, citySunset);
    }

    private void SetSkybox(int weatherCode, bool isDaytime)
    {
        if (!isDaytime && nightSkybox != null)
        {
            RenderSettings.skybox = nightSkybox;
        }
        else if (weatherCode >= 200 && weatherCode < 700)
        {
            RenderSettings.skybox = stormySkybox;
        }
        else if (weatherCode >= 700 && weatherCode > 800)
        {
            RenderSettings.skybox = cloudySkybox;
        }
        else
        {
            RenderSettings.skybox = sunnySkybox;
        }

        DynamicGI.UpdateEnvironment();
    }

    private void SetSun(int weatherCode, bool isDaytime, DateTime now, DateTime sunrise, DateTime sunset)
    {
        if (sun == null) return;

        if (!isDaytime)
        {
            sun.intensity = 0f;
            sun.transform.rotation = Quaternion.Euler(-90f, 170f, 0f);
            return;
        }

        double totalDayMinutes = (sunset - sunrise).TotalMinutes;
        double elapsedMinutes = (now - sunrise).TotalMinutes;
        float dayProgress = Mathf.Clamp01((float)(elapsedMinutes / totalDayMinutes));

        float sunAngle = dayProgress * 180f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        float weatherMultiplier = 1f;
        if (weatherCode >= 200 && weatherCode < 700) weatherMultiplier = 0.3f;
        else if (weatherCode > 800) weatherMultiplier = 0.6f;

        float sunHeightFactor = Mathf.Sin(dayProgress * Mathf.PI);
        sun.intensity = maxSunIntensity * sunHeightFactor * weatherMultiplier;
        sun.color = Color.Lerp(sunsetSunColor, daySunColor, sunHeightFactor);
    }

    private void FetchCityWeather(string cityQuery)
    {
        xmlApi = $"https://api.openweathermap.org/data/2.5/weather?q={cityQuery}&mode=xml&appid={apiKey}";
        StartCoroutine(CallAPI(xmlApi, OnXMLDataLoaded));
    }

    [ContextMenu("Orlando")]
    private void SetOrlando() => FetchCityWeather("Orlando,us");

    [ContextMenu("Topeka")]
    private void SetTopeka() => FetchCityWeather("Topeka,us");

    [ContextMenu("Sacramento")]
    private void SetSacramento() => FetchCityWeather("Sacramento,us");

    [ContextMenu("Madrid")]
    private void SetMadrid() => FetchCityWeather("Madrid,es");

    [ContextMenu("Canberra")]
    private void SetCanberra() => FetchCityWeather("Canberra,au");
}