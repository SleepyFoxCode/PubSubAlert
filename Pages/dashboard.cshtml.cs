using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;


namespace RazorPagesTwitchPubSub.Pages
{
    public class DashboardModel : PageModel{

        public CurrentUser user;
        public IConfiguration _configuration;
        public DashboardModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        

        public IActionResult OnGet(){
            user = new CurrentUser(this.HttpContext, _configuration);
            if(user.information == null) return Redirect("https://id.twitch.tv/oauth2/authorize?client_id=" + _configuration["ClientId"] + "&redirect_uri=https://" + _configuration["Host"] + "/dashboard?handler=redirect&response_type=code&scope=channel:read:redemptions+user:read:email");
            MyWebsocketHelper.ClearPubSubEventFile(user.information.id);
            return null;
        }
        
        public RedirectResult OnGetRedirect(){
            
            try{
                String getCode = HttpContext.Request.Query["code"].ToString();
                String baseUrl = "https://id.twitch.tv/oauth2/token";
                String clientId = "client_id=" + _configuration["ClientId"];
                String clientSecret = "client_secret=" + _configuration["SecretKey"];
                String code = "code=" + getCode;
                String grantType = "grant_type=authorization_code";
                String redirectUri = "redirect_uri=https://" + _configuration["Host"] + "/dashboard?handler=redirect";
                String completeUrl = baseUrl + '?' + clientId + '&' + clientSecret + '&' + code + '&' + grantType + '&' + redirectUri;
                Uri uri = new Uri(completeUrl);
                TwitchJsonHelper.JsonUserAuth jsonUserAuth;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method="POST";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream)){
                    String jsonString = reader.ReadToEnd();
                    stream.Close();
                    jsonUserAuth = JsonSerializer.Deserialize<TwitchJsonHelper.JsonUserAuth>(jsonString);
                }

                user = new CurrentUser(jsonUserAuth.access_token, _configuration);
                if(user.information == null){
                    Response.Cookies.Delete("access_token");
                    Response.Cookies.Delete("refresh_token");
                    throw new Exception("User.information was null in OnGetRedirect in Authentication");
                }

                var options = new CookieOptions
                {
                    IsEssential = true
                };

                Response.Cookies.Delete("access_token");
                Response.Cookies.Delete("refresh_token");
                Response.Cookies.Append("access_token", jsonUserAuth.access_token, options);
                Response.Cookies.Append("refresh_token", jsonUserAuth.refresh_token, options);
                MyWebsocketHelper.UpdateUser(user.information.id, user.information.login, jsonUserAuth.access_token);
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                Log.WriteToLog(e.ToString());
                return Redirect("https://" + _configuration["Host"]);
            }

            return Redirect("https://" + _configuration["Host"] + "/dashboard");
        }

        public IActionResult OnPostGetNewPubSubs(){

            user = new CurrentUser(this.HttpContext, _configuration);
            if(user.information == null) return new JsonResult("Error");

            List<TwitchJsonHelper.JsonPubSubRoot> pubSubList = MyWebsocketHelper.GetPubSubEvents(user.information.id);
            if(pubSubList == null) return new JsonResult("Error");
            if(pubSubList.Count < 1) return new JsonResult("Error");

            List<TwitchJsonHelper.JsonMyPubSub> events = new List<TwitchJsonHelper.JsonMyPubSub>();
            foreach(TwitchJsonHelper.JsonPubSubRoot item in pubSubList){
                TwitchJsonHelper.JsonMyPubSub pubSubObj  = new TwitchJsonHelper.JsonMyPubSub();
                pubSubObj.title = item.data.redemption.reward.title;
                pubSubObj.pubSub_id = item.data.redemption.id;
                pubSubObj.display_name = item.data.redemption.user.display_name;
                // Time parsing to read it better
                string date = item.data.timestamp.Substring(0,item.data.timestamp.IndexOf('T'));
                string time = item.data.timestamp.Substring(item.data.timestamp.IndexOf('T') + 1, 8);
                pubSubObj.redeemed_at = date + " " + time;
                pubSubObj.cost = item.data.redemption.reward.cost;
                if(item.data.redemption.reward.image != null) pubSubObj.image = item.data.redemption.reward.image.url_1x;
                if(item.data.redemption.reward.default_image != null) pubSubObj.default_image = item.data.redemption.reward.default_image.url_1x;
                pubSubObj.user_input = item.data.redemption.user_input;
                events.Add(pubSubObj);
            }
            // Return to ajax request
            MyWebsocketHelper.ClearPubSubEventFile(user.information.id);
            return new JsonResult(JsonSerializer.Serialize(events));
        }
        public IActionResult OnPostGetNewPubSubsTest(){

            user = new CurrentUser(this.HttpContext, _configuration);
            if(user.information == null) return new JsonResult("Error");

            // This adds a testing alert to the official alert file for alert window
            MyWebsocketHelper.GetPubSubAlertsTest(user.information.id);

            // This creates a test event for the dashboard from a testfile
            List<TwitchJsonHelper.JsonPubSubRoot> pubSubList = MyWebsocketHelper.GetPubSubEventsTest();
            if(pubSubList == null) return new JsonResult("Error");
            if(pubSubList.Count < 1) return new JsonResult("Error");

            List<TwitchJsonHelper.JsonMyPubSub> events = new List<TwitchJsonHelper.JsonMyPubSub>();
            foreach(TwitchJsonHelper.JsonPubSubRoot item in pubSubList){
                TwitchJsonHelper.JsonMyPubSub pubSubObj  = new TwitchJsonHelper.JsonMyPubSub();
                pubSubObj.title = item.data.redemption.reward.title;
                pubSubObj.pubSub_id = item.data.redemption.id;
                pubSubObj.display_name = item.data.redemption.user.display_name;
                // Time parsing to read it better
                string date = item.data.timestamp.Substring(0,item.data.timestamp.IndexOf('T'));
                string time = item.data.timestamp.Substring(item.data.timestamp.IndexOf('T') + 1, 8);
                pubSubObj.redeemed_at = date + " " + time;
                pubSubObj.cost = item.data.redemption.reward.cost;
                if(item.data.redemption.reward.image != null) pubSubObj.image = item.data.redemption.reward.image.url_1x;
                if(item.data.redemption.reward.default_image != null) pubSubObj.default_image = item.data.redemption.reward.default_image.url_1x;
                pubSubObj.user_input = item.data.redemption.user_input;
                events.Add(pubSubObj);
            }
            // Return to ajax request
            return new JsonResult(JsonSerializer.Serialize(events));
        }
    }
       
}



