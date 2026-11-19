using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/*
   Check what data you receive.
   - Next, we need to parse the XML or JSON and use the data that we receive to make changes in our scene. We want to change:
   - The skybox based on weather
   - The sun's intensity and color
   - You can read the PC time (System.DateTime.Now) (System.DateTime Methods - ADO.NET | Microsoft Learn) and based on time change between where you are and the city, you could also change day/night cycle.
   - Make a simple UI or inspector-based choice to change the city. When we change the city, the scene should update.
 */

//USE A HOTSPOTTTTTT

public class WeatherManager: MonoBehaviour
{
   private class BypassCertificate : CertificateHandler //fixes certificate issues
   {
      protected override bool ValidateCertificate(byte[] certificateData) => true;
   }

   private string xmlApi;
   private IEnumerator CallAPI(string url, Action<string> callback) {
      using (UnityWebRequest request = UnityWebRequest.Get(url)) {
         //bypass
         request.certificateHandler = new BypassCertificate();
         request.disposeCertificateHandlerOnDispose = true;
         yield return request.SendWebRequest();
         
         if (request.result == UnityWebRequest.Result.ConnectionError) {
            Debug.LogError($"network problem: {request.error}");
         } else if (request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError($"response error: {request.responseCode}");
         } else {
             callback(request.downloadHandler.text);
         }
      }
   }

   public IEnumerator GetWeatherXML(Action<string> callback) {
      return CallAPI(xmlApi, callback);
   }
   
   public void OnXMLDataLoaded(string data) 
   {
      Debug.Log(data);
   }
   
   
   //skyboxes ------
   //sunny
   //cloudy
   //stormy
   
   
   /*
      <current>
      <city id="4167147" name="Orlando">
      <coord lon="-81.3792" lat="28.5383"/>
      <country>US</country>
      <timezone>-18000</timezone>
      <sun rise="2026-11-18T11:50:34" set="2026-11-18T22:31:06"/> -------- Sun Intensity and color, if nighttime = off
      </city>
      <temperature value="293.65" min="292.53" max="294.18" unit="kelvin"/>
      <feels_like value="294.11" unit="kelvin"/>
      <humidity value="90" unit="%"/>
      <pressure value="1020" unit="hPa"/>
      <wind>
      <speed value="7.2" unit="m/s" name="Moderate breeze"/>
      <gusts value="11.83"/>
         <direction value="70" code="ENE" name="East-northeast"/>
      </wind>
      <clouds value="100" name="overcast clouds"/>
      <visibility value="10000"/>
         <precipitation mode="no"/>
         <weather number="804" value="overcast clouds" icon="04n"/> ---------- Skyboxes
      <lastupdate value="2026-11-19T01:12:44"/>
         </current>
   */
   private void SetCity()
   {
      StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
   }
   
   [ContextMenu("Orlando")]
   private void SetOrlando()
   {
      xmlApi = "https://api.openweathermap.org/data/2.5/weather?q=Orlando,us&mode=xml&appid=ccff35769d4969c7b8a1ca7d8945c6ad";
      SetCity();
   }

   [ContextMenu("Topeka")]
   private void SetTopeka()
   {
      xmlApi = "https://api.openweathermap.org/data/2.5/weather?q=Orlando,us&mode=xml&appid=APIKEY";
      SetCity();
   }

   [ContextMenu("Sacramento")]
   private void SetSacramento()
   {
      xmlApi = "https://api.openweathermap.org/data/2.5/weather?q=Orlando,us&mode=xml&appid=APIKEY";
      SetCity();
   }

   [ContextMenu("Madrid")]
   private void SetMadrid()
   {
      xmlApi = "https://api.openweathermap.org/data/2.5/weather?q=Orlando,us&mode=xml&appid=APIKEY";
      SetCity();
   }

   [ContextMenu("Canberra")]
   private void SetCanberra()
   {
      xmlApi = "https://api.openweathermap.org/data/2.5/weather?q=Orlando,us&mode=xml&appid=APIKEY";
      SetCity();
   }
   
}