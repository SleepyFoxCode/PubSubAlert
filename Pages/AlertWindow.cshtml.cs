using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace PubSubAlert.Pages
{
    public class AlertWindowModel : PageModel
    {

        public IConfiguration _configuration;
        public AlertWindowModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public AlertSettings AlertSettings;
        // Since we can't have cookies in obs we need to get the user_id from our dashboard
        public string Id;      

        public IActionResult OnGet(){
            Id = HttpContext.Request.Query["id"];
            // Clear the alert file from our websocket when we open the window (or refresh it)
            MyWebsocketHelper.ClearPubSubFile(Id, MyWebsocketHelper.WebsocketDataPathAlert);
            // Setting up an object for the settings the user set for the alert. We get them from url 
            AlertSettings = new AlertSettings();
            AlertSettings.msg = HttpContext.Request.Query["msg"];
            AlertSettings.duration = HttpContext.Request.Query["duration"];
            AlertSettings.url = HttpContext.Request.Query["img"];
            AlertSettings.fontSize = HttpContext.Request.Query["fontsize"] + "em";
            AlertSettings.fontFamily = HttpContext.Request.Query["fontfamily"];
            AlertSettings.fontColor = "#" + HttpContext.Request.Query["fontcolor"];
            AlertSettings.sound = HttpContext.Request.Query["sound"];
            AlertSettings.volume = HttpContext.Request.Query["volume"];

            return null;
        }

        // This gets called from the html file every x seconds   
        public IActionResult OnPostGetNewPubSubs(){
            // Get all alerts to play from our websocket file
            List<TwitchJsonHelper.JsonPubSubRoot> pubSubList = MyWebsocketHelper.GetPubSubs(HttpContext.Request.Query["id"], MyWebsocketHelper.WebsocketDataPathAlert);
            if(pubSubList == null) return new JsonResult("Error");
            if(pubSubList.Count < 1) return new JsonResult("Error");

            List<TwitchJsonHelper.JsonMyPubSub> events = new List<TwitchJsonHelper.JsonMyPubSub>();
            foreach(TwitchJsonHelper.JsonPubSubRoot item in pubSubList){
                TwitchJsonHelper.JsonMyPubSub pubSubObj  = new TwitchJsonHelper.JsonMyPubSub();
                pubSubObj.title = item.data.redemption.reward.title;
                pubSubObj.pubSub_id = item.data.redemption.id;
                pubSubObj.display_name = item.data.redemption.user.display_name;
                // Time parsing to read it better. This can play the wrong time because of timezones
                string date = item.data.timestamp.Substring(0,item.data.timestamp.IndexOf('T'));
                string time = item.data.timestamp.Substring(item.data.timestamp.IndexOf('T') + 1, 8);
                pubSubObj.redeemed_at = date + " " + time;
                pubSubObj.cost = item.data.redemption.reward.cost;
                if(item.data.redemption.reward.image != null) pubSubObj.image = item.data.redemption.reward.image.url_4x;
                if(item.data.redemption.reward.default_image != null) pubSubObj.default_image = item.data.redemption.reward.default_image.url_4x;
                pubSubObj.user_input = item.data.redemption.user_input;
                events.Add(pubSubObj);
            }
            // After getting all alerts from our websocket file we clear it. Important: If an alert happens in this function the alert will be destroyed
            MyWebsocketHelper.ClearPubSubFile(HttpContext.Request.Query["id"], MyWebsocketHelper.WebsocketDataPathAlert);
            // We return the json file with all the alerts. It returns a json with a list
            return new JsonResult(JsonSerializer.Serialize(events));
        }
    }

    public class AlertSettings{
        public string msg;
        public string duration;
        public string url;
        public string sound;
        public string volume;
        public string fontFamily;
        public string fontSize;
        public string fontColor;
    }
}
