using System;
using System.Net;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace PubSubAlert
{

    public class TwitchHelper{
        
        static private string OAuthTokenValidationUrl = "https://id.twitch.tv/oauth2/validate";
        static private string TwitchNewAPIUrl = "https://api.twitch.tv/helix/";
        static private string OAuthTokenRefreshUrl = "https://id.twitch.tv/oauth2/token";

        // We check if the current access token is valid by calling the twitch token validation url
        public static  Boolean AccessTokenIsValid(String accessToken, IConfiguration _configuration){
            if(accessToken == String.Empty){
                return false;
            }
            try{
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(OAuthTokenValidationUrl);
                request.Headers.Add("Authorization", "OAuth " + accessToken);
                request.Headers.Add("Client-ID", _configuration["ClientId"]); 

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream()){
                    using(StreamReader reader = new StreamReader(stream)){
                        String jsonString = reader.ReadToEnd();
                        stream.Close();
                    }
                }
            }
            catch(WebException){
                // We don't this exception because it just means that the access token is not valid anymore. A new one will be created
                return false;
            }
            catch(Exception e){
                Log.WriteToLog(e.ToString());
                return false;
            }
            return true;
        }

        // We refresh tokens when access token doesn't work anymore by using the refresh token
        public static TwitchJsonHelper.JsonRefresh RefreshTokens(String refreshToken, IConfiguration _configuration){
            if(refreshToken==String.Empty) return null;
            try{
                string baseUrl = OAuthTokenRefreshUrl;
                string grantType = "refresh_token";
                string clientId = _configuration["ClientId"];  // todo client id outsource
                string clientSecret = _configuration["SecretKey"]; // todo client_secret outsource
                TwitchJsonHelper.JsonRefresh jsonObj;

                string completeUrl = baseUrl 
                + "?grant_type=" + grantType 
                + "&refresh_token=" + refreshToken 
                + "&client_id=" + clientId 
                + "&client_secret=" + clientSecret;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeUrl);
                request.Method = "POST";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream()){
                    using(StreamReader reader = new StreamReader(stream)){
                        String jsonString = reader.ReadToEnd();
                        stream.Close();
                        jsonObj = JsonSerializer.Deserialize<TwitchJsonHelper.JsonRefresh>(jsonString);
                    }
                }

                if(jsonObj != null) return jsonObj;
                else return null;
            }
            catch(Exception e){
                Log.WriteToLog(e.ToString());
                return null;
            }
        }

        // Gets user information by calling the new twitch api
        public static UserInformation LoadUserInformation(String accessToken, IConfiguration _configuration){
            try{
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TwitchNewAPIUrl + "users");
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Headers.Add("Client-ID", _configuration["ClientId"]); // Todo: Outsource client id

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream()){
                    using(StreamReader reader = new StreamReader(stream)){
                        String jsonString = reader.ReadToEnd();
                        stream.Close();
                        TwitchJsonHelper.JsonUserInformationList jsonU = JsonSerializer.Deserialize<TwitchJsonHelper.JsonUserInformationList>(jsonString);
                        // Since this api call returns a list of users, we return the closest one. Maybe there is a better api call
                        return jsonU.data[0];
                    }
                }
            }
            catch(Exception e){
                Log.WriteToLog(e.ToString());
                return null;
            }
        }


        // We can't change the name conventions of this class because we use this to deserialize json from the Twitch api
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