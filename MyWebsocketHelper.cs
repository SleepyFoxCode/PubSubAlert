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

namespace RazorPagesTwitchPubSub{

    class MyWebsocketHelper{

        public static FileStream fs;

        static string websocketDataPath = "~/PubSubWebsocket/data/";
        static string userFileName = "users.json";
        public static void UpdateUser(String id, String name, String access_token){

            TwitchJsonHelper.JsonUpdateWS jsonObj = new TwitchJsonHelper.JsonUpdateWS();
            jsonObj.channel_id = id;
            jsonObj.login = name;
            jsonObj.access_token = access_token;

            TwitchJsonHelper.JsonUpdateWSList jsonList = new TwitchJsonHelper.JsonUpdateWSList();
            
            String jsonNewString;

            // Read the old json file
            try{    
                fs = new FileStream(websocketDataPath + userFileName, FileMode.Open);
                using (StreamReader reader = new StreamReader(fs)){
                    string text = reader.ReadToEnd();
                    fs.Close();
                    if(text != String.Empty){
                        jsonList = JsonSerializer.Deserialize<TwitchJsonHelper.JsonUpdateWSList>(text);
                        if(!jsonList.UpdateIfAlreadyExists(jsonObj)) jsonList.users.Add(jsonObj);
                        jsonNewString = JsonSerializer.Serialize<TwitchJsonHelper.JsonUpdateWSList>(jsonList);
                    }
                    else{
                        //jsonList.users
                        jsonList.users = new List<TwitchJsonHelper.JsonUpdateWS>();
                        jsonList.users.Add(jsonObj);
                        jsonNewString = JsonSerializer.Serialize<TwitchJsonHelper.JsonUpdateWSList>(jsonList);
                    }

                }
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                Log.WriteToLog(e.ToString());
                if(fs == null){
                    return;
                }
                fs.Close();
                return;
            }

            // Write the new json file
            try{
                fs = new FileStream(websocketDataPath + userFileName, FileMode.Create);
                using(StreamWriter writer = new StreamWriter(fs)){
                    writer.Write(jsonNewString);
                    writer.Flush();
                    fs.Close();
                }
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                Log.WriteToLog(e.ToString());
                if(fs == null){
                    return;
                }
                fs.Close();
                return;
            }
        }

        

        public static List<TwitchJsonHelper.JsonPubSubRoot> GetPubSubEvents(string id){
            List<TwitchJsonHelper.JsonPubSubRoot> list = new List<TwitchJsonHelper.JsonPubSubRoot>();
            try{    
                fs = new FileStream(websocketDataPath + id + ".json", FileMode.Open);
                using (StreamReader reader = new StreamReader(fs)){
                    string str = reader.ReadToEnd();
                    fs.Close();
                    if(str == String.Empty) return null;
                    return list = JsonSerializer.Deserialize<List<TwitchJsonHelper.JsonPubSubRoot>>(str);
                }
            }
            catch(System.IO.FileNotFoundException e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return null;
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                if(fs == null){
                    return null;
                }
                fs.Close();
                return null;
            }
        }


        public static List<TwitchJsonHelper.JsonPubSubRoot> GetPubSubAlerts(string id){
            List<TwitchJsonHelper.JsonPubSubRoot> list = new List<TwitchJsonHelper.JsonPubSubRoot>();
            try{    
                fs = new FileStream(websocketDataPath + "/alert/" + id + ".json", FileMode.Open);
                using (StreamReader reader = new StreamReader(fs)){
                    
                    string str = reader.ReadToEnd();
                    fs.Close();
                    if(str == String.Empty) return null;
                    return list = JsonSerializer.Deserialize<List<TwitchJsonHelper.JsonPubSubRoot>>(str);
                }
            }
            catch(System.IO.FileNotFoundException e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return null;
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                if(fs == null){ 
                    return null;
                }
                fs.Close();
                return null;
            }
        }
    
        public static void ClearPubSubEventFile(string id){
            try{
                fs = new FileStream(websocketDataPath + id + ".json", FileMode.Truncate);
                fs.Close();
            }
            catch(System.IO.FileNotFoundException e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                if(fs == null){
                    return;
                }
                fs.Close();
            }
        }
        public static void ClearPubSubAlertFile(string id){
            try{
                fs = new FileStream(websocketDataPath + "/alert/" + id + ".json", FileMode.Truncate);
                fs.Close();
            }
            catch(System.IO.FileNotFoundException e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.ToString());
                if(fs == null){
                    return;
                }
                fs.Close();
            }
        }
    }

}