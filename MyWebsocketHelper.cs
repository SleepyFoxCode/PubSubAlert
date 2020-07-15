using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;


namespace RazorPagesTwitchPubSub{

    class MyWebsocketHelper{
        
        // Data of the websocket. Here we store alerts, events, users
        public static string websocketDataPath = "PubSubWebsocket/data/";
        public static string websocketDataPathAlert = websocketDataPath + "alert/";
        public static string websocketDataPathTesting = websocketDataPath + "testing/";
        static string userFileName = "users.json";
        
        // Updates the user.json file for this user. When we update the user, we take the content out of the file, append the new user, and put it back
        // Gets called when token gets refreshed or when user signs up
        public static void UpdateUser(String id, String name, String accessToken){

            TwitchJsonHelper.JsonUpdateWS jsonObj = new TwitchJsonHelper.JsonUpdateWS();
            jsonObj.channel_id = id;
            jsonObj.login = name;
            jsonObj.access_token = accessToken;
            TwitchJsonHelper.JsonUpdateWSList jsonList = new TwitchJsonHelper.JsonUpdateWSList();
            
            // Read the old json file
            if(!File.Exists(websocketDataPath + userFileName)) CreateFile(websocketDataPath + userFileName);
            using (FileStream fs = new FileStream(websocketDataPath + userFileName, FileMode.Create)){
                try{
                StreamReader reader = new StreamReader(fs);
                string text = reader.ReadToEnd();
                String jsonNewString;
                // user.json is not empty so we need to check if the user already exists
                if(text != String.Empty){
                    jsonList = JsonSerializer.Deserialize<TwitchJsonHelper.JsonUpdateWSList>(text);
                    if(!jsonList.UpdateIfAlreadyExists(jsonObj)) jsonList.users.Add(jsonObj);
                    jsonNewString = JsonSerializer.Serialize<TwitchJsonHelper.JsonUpdateWSList>(jsonList);
                }
                else{
                    jsonList.users = new List<TwitchJsonHelper.JsonUpdateWS>();
                    jsonList.users.Add(jsonObj);
                    jsonNewString = JsonSerializer.Serialize<TwitchJsonHelper.JsonUpdateWSList>(jsonList);
                }

                StreamWriter writer = new StreamWriter(fs);
                writer.Write(jsonNewString);
                writer.Flush();
                
                }
                catch(Exception e){
                    Log.WriteToLog(e.ToString());
                }
            }
        }




        // This gets all pubsubs that were fetched from the websocket (Path decides if alert or event)
        public static List<TwitchJsonHelper.JsonPubSubRoot> GetPubSubs(string id, string path){
            List<TwitchJsonHelper.JsonPubSubRoot> list = new List<TwitchJsonHelper.JsonPubSubRoot>();
            if(!File.Exists(path + id + ".json")) CreateFile(path + id + ".json");
            using(FileStream fs = new FileStream(path + id + ".json", FileMode.Open)){
                try{
                    StreamReader reader = new StreamReader(fs);
                    string text = reader.ReadToEnd();
                    if(text == String.Empty) return null;
                    return list = JsonSerializer.Deserialize<List<TwitchJsonHelper.JsonPubSubRoot>>(text);
                }
                catch(Exception e){
                    Log.WriteToLog(e.ToString());
                    return null;
                }
            }
        }
        // Creates a test event on the dashboard
        // We take the json from a test json file
        public static List<TwitchJsonHelper.JsonPubSubRoot> GetPubSubEventsTest(){
            // We need to create a file if the websocket never fetched an alert before
            List<TwitchJsonHelper.JsonPubSubRoot> list = new List<TwitchJsonHelper.JsonPubSubRoot>();
            if(!File.Exists(websocketDataPathTesting + "test.json")) CreateFile(websocketDataPathTesting + "test.json");
            using(FileStream fs = new FileStream(websocketDataPathTesting + "test.json", FileMode.Open)){
                try{
                    StreamReader reader = new StreamReader(fs);
                    string text = reader.ReadToEnd();
                    if(text == String.Empty) return null;
                    return list = JsonSerializer.Deserialize<List<TwitchJsonHelper.JsonPubSubRoot>>(text);
                }
                catch(Exception e){
                    Log.WriteToLog(e.ToString());
                    return null;
                }
            }
        }
        
        // Creates a test alert in the alert window
        // We use a test file stored in a test folder and paste its content into the official alert file
        // We can't use the same approach as in the GetPubSubEventTest because we have no button on the window
        public static void GetPubSubAlertsTest(string id){
            // We need to create a file if the websocket never fetched an alert before
            if(!File.Exists(websocketDataPathTesting + "alert/" + "test.json")) CreateFile(websocketDataPathTesting + "alert/" + "test.json");
            using(FileStream fs = new FileStream(websocketDataPathTesting + "alert/" + "test.json", FileMode.Open)){
                try{
                    StreamReader reader = new StreamReader(fs);
                    string text = reader.ReadToEnd();
                    if(text == String.Empty) throw new Exception("Testfile is empty"); 

                    if(!File.Exists(websocketDataPath + "alert/" + id + ".json")) CreateFile(websocketDataPath + "alert/" + id + ".json");
                    using(FileStream fs_second = new FileStream(websocketDataPath + "alert/" + id + ".json", FileMode.Open)){
                        try{
                            StreamWriter writer = new StreamWriter(fs_second);
                            writer.Write(text);
                            writer.Flush();
                        }
                        catch(Exception e){
                            Log.WriteToLog(e.ToString());
                        }
                    }
                }
                catch(Exception e){
                    Log.WriteToLog(e.ToString());
                }
            }
        }
    
        public static void CreateFile(string path){
            Log.WriteToLog("Creating file in: " + path);
            using(FileStream fs = File.Create(path)){}
        }

        // Clears all fetched pubsubs either alert or event (Depends on the path)
        public static void ClearPubSubFile(string id, string path){
            try{
                using(FileStream fs = new FileStream(path + id + ".json", FileMode.Truncate)){}
            }
            catch(Exception e){
                Log.WriteToLog(e.ToString());
            }
        }

    }
}