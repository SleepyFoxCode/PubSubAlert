using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;




namespace RazorPagesTwitchPubSub{

    public class TwitchHelper{

        //private readonly IConfiguration _configuration;
        
        static string oAuthTokenValidationUrl = "https://id.twitch.tv/oauth2/validate";
        static string twitchNewAPIUrl = "https://api.twitch.tv/helix/";
        static string oAuthTokenRefreshUrl = "https://id.twitch.tv/oauth2/token";

        public static  Boolean AccessTokenIsValid(String access_token, IConfiguration _configuration){
            if(access_token == String.Empty){
                System.Diagnostics.Debug.WriteLine("Empty access token");
                return false;
            }
            try{
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oAuthTokenValidationUrl);
                request.Headers.Add("Authorization", "OAuth " + access_token);
                request.Headers.Add("Client-ID", _configuration["ClientId"]); 

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream)){
                    String jsonString = reader.ReadToEnd();
                    stream.Close();
                }
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e);
                Log.WriteToLog(e.ToString());
                return false;
            }
            return true;
        }
        public static TwitchJsonHelper.JsonRefresh RefreshTokens(String refresh_token, IConfiguration _configuration){
            if(refresh_token==String.Empty) return null;
            try{
                string base_url = oAuthTokenRefreshUrl;
                string grant_type = "refresh_token";
                string client_id = _configuration["ClientId"];  // todo client id outsource
                string client_secret = _configuration["SecretKey"]; // todo client_secret outsource
                TwitchJsonHelper.JsonRefresh jsonObj;

                string complete_url = base_url 
                + "?grant_type=" + grant_type 
                + "&refresh_token=" + refresh_token 
                + "&client_id=" + client_id 
                + "&client_secret=" + client_secret;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(complete_url);
                request.Method = "POST";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream)){
                    String jsonString = reader.ReadToEnd();
                    stream.Close();
                    jsonObj = JsonSerializer.Deserialize<TwitchJsonHelper.JsonRefresh>(jsonString);
                }
                if(jsonObj != null) return jsonObj;
                else return null;
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                Log.WriteToLog(e.ToString());
                return null;
            }
        }
        public static UserInformation LoadUserInformation(String access_token, IConfiguration _configuration){

            try{
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create( twitchNewAPIUrl + "users");
                request.Headers.Add("Authorization", "Bearer " + access_token);
                request.Headers.Add("Client-ID", _configuration["ClientId"]); // Todo: Outsource client id

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream)){
                    String jsonString = reader.ReadToEnd();
                    stream.Close();
                    TwitchJsonHelper.JsonUserInformationList jsonU = JsonSerializer.Deserialize<TwitchJsonHelper.JsonUserInformationList>(jsonString);
                    // Since this api call returns a list of users, we return the closest one. Maybe there is a better api call
                    return jsonU.data[0];
                }
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                Log.WriteToLog(e.ToString());
                return null;
            }
        }



        public class UserInformation {
            public String id { get; set; }
            public String login { get; set; }
            public String display_name { get; set; }
            public String type { get; set; }
            public String broadcaster_type { get; set; }
            public String description { get; set; }
            public String profile_image_url { get; set; }
            public String offline_image_url { get; set; }
            public int view_count { get; set; }
            public String email { get; set; }
        }
    }
}


