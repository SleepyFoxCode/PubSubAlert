using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace RazorPagesTwitchPubSub.Pages
{
    public class AlertWindowModel : PageModel
    {

        public IConfiguration _configuration;
        public AlertWindowModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public AlertSettings alertSettings;
        // Since we can't have cookies in obs we need to get the user_id from our dashboard
        public string id;      

        public async Task<IActionResult> OnGetAsync(){
            id = HttpContext.Request.Query["id"];
            // Clear the alert file from our websocket when we open the window (or refresh it)
            MyWebsocketHelper.ClearPubSubAlertFile(id);
            // Setting up an object for the settings the user set for the alert. We get them from url 
            alertSettings = new AlertSettings();
            alertSettings.duration = HttpContext.Request.Query["duration"];
            alertSettings.url = HttpContext.Request.Query["image"];
            alertSettings.font_size_title = HttpContext.Request.Query["fontsizetitle"] + "em";
            alertSettings.font_size_costs = HttpContext.Request.Query["fontsizecosts"] + "em";
            alertSettings.font_size_username = HttpContext.Request.Query["fontsizeusername"] + "em";
            alertSettings.font_size_msg = HttpContext.Request.Query["fontsizemsg"] + "em";
            alertSettings.font_family = HttpContext.Request.Query["fontfamily"];
            alertSettings.font_color_title = "#" + HttpContext.Request.Query["fontcolortitle"];
            alertSettings.font_color_costs = "#" + HttpContext.Request.Query["fontcolorcosts"];
            alertSettings.font_color_username = "#" + HttpContext.Request.Query["fontcolorusername"];
            alertSettings.font_color_msg = "#" + HttpContext.Request.Query["fontcolormsg"];
            alertSettings.sound = HttpContext.Request.Query["sound"];
            alertSettings.volume = HttpContext.Request.Query["volume"];
            return null;
        }

        public IActionResult OnPostGetNewPubSubs(){
            // Get all alerts to play from our websocket file
            List<TwitchJsonHelper.JsonPubSubRoot> pubSubList = MyWebsocketHelper.GetPubSubAlerts(HttpContext.Request.Query["id"]);
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
                if(item.data.redemption.reward.image != null) pubSubObj.image = item.data.redemption.reward.image.url_1x;
                if(item.data.redemption.reward.default_image != null) pubSubObj.default_image = item.data.redemption.reward.default_image.url_1x;
                pubSubObj.user_input = item.data.redemption.user_input;
                events.Add(pubSubObj);
            }
            // After getting all alerts from our websocket file we clear it. Important: If an alert happens in this function the alert will be destroyed
            MyWebsocketHelper.ClearPubSubAlertFile(HttpContext.Request.Query["id"]);
            // We return the json file with all the alerts. It returns a json with a list
            return new JsonResult(JsonSerializer.Serialize(events));
        }
    }

    public class AlertSettings{
        public string duration;
        public string url;
        public string sound;
        public string volume;
        public string font_family;
        public string font_size_title;
        public string font_size_costs;
        public string font_size_username;
        public string font_size_msg;
        public string font_color_title;
        public string font_color_costs;
        public string font_color_username;
        public string font_color_msg;
    }
}