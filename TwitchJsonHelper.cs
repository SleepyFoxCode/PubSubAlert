using System;
using System.Collections.Generic;




namespace RazorPagesTwitchPubSub{

    public class TwitchJsonHelper{



        public class JsonUserAuth {
            public String access_token { get; set; }
            public String refresh_token { get; set; }
            public int expires_in { get; set; }
            public String[] scope { get; set; }
            public String token_type { get; set; }
        }

        // Json format to update the websocket user file
        public class JsonUpdateWS {
            public String channel_id {get;set;}
            public String login {get;set;}
            public String access_token{get;set;}
        }

        // Json format to update the websocket user file but the list of it (This will be dumped into user.json)
        public class JsonUpdateWSList {
            public List<JsonUpdateWS> users {get;set;}
            public bool UpdateIfAlreadyExists(JsonUpdateWS obj){
                foreach(JsonUpdateWS item in users){
                    if(item.channel_id.Equals(obj.channel_id)){
                        item.access_token = obj.access_token;
                        item.login = obj.login;
                        return true;
                    }
                }
                return false;
            }
        }


    // json userinformation classes. Basic information about the user
        public class JsonUserInformationList {
            public List<TwitchHelper.UserInformation> data {get; set;}
        }

    // json pubsub classes
        public class JsonMyPubSub{
            public string title {get;set;}
            public string pubSub_id {get;set;}
            public string display_name {get;set;}
            public string redeemed_at {get;set;}
            public int cost {get;set;}
            public string image {get;set;}
            public string default_image {get;set;}
            public string user_input {get;set;}
            
        }
        public class JsonPubSubRoot{   
            public JsonPubSubData data {get;set;}   
        }
        public class JsonPubSubData{
            public string timestamp {get;set;}
            public JsonPubSubRedemption redemption {get;set;}
        }
        public class JsonPubSubRedemption{
            public string id {get;set;}
            public JsonPubSubUser user {get;set;}
            public string channel_id {get;set;}
            public string redeemed_at {get;set;}
            public JsonPubSubReward reward {get;set;}
            public string user_input {get;set;}
            public string status {get;set;}
        }
        public class JsonPubSubUser{
            public string id {get;set;}
            public string login {get;set;}
            public string display_name {get;set;}
        }
        public class JsonPubSubReward{
            public string id {get;set;}
            public string channel_id {get;set;}
            public string title {get;set;}
            public string prompt {get;set;}
            public int cost {get;set;}
            public bool is_user_input_required {get;set;}
            public bool is_sub_only {get;set;}
            public JsonPubSubImage image {get;set;}
            public JsonPubSubDefaultImage default_image {get;set;}
            public string background_color {get;set;}
            public bool is_enabled {get;set;}
            public bool is_paused {get;set;}
            public bool is_in_stock {get;set;}
            public JsonPubSubMaxPerStream max_per_stream {get;set;}
            public bool should_redemptions_skip_request_queue {get;set;}
            public string template_id {get;set;}
            public string updated_for_indicator_at {get;set;}

        }

        public class JsonPubSubImage{
            public string url_1x {get;set;}
            public string url_2x {get;set;}
            public string url_4x {get;set;}
        }
        public class JsonPubSubDefaultImage{
            public string url_1x {get;set;}
            public string url_2x {get;set;}
            public string url_4x {get;set;}
        }
        public class JsonPubSubMaxPerStream{
            public bool is_enabled {get;set;}
            public int max_per_stream {get;set;}
        }


        public class JsonRefresh{
            public string access_token {get;set;}
            public string refresh_token {get;set;}
            public List<string> scope{get;set;}
            public string token_type{get;set;}
        }
    }

}