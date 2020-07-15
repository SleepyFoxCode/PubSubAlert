import asyncio
import websockets
import time
import random
import string
import json
import os

server = "wss://pubsub-edge.twitch.tv"

pub_sub_event_folder = os.path.dirname(os.path.realpath(__file__)) + "\\data\\"
pub_sub_alert_folder = os.path.dirname(os.path.realpath(__file__)) + "\\data\\alert\\"
loop = asyncio.get_event_loop()
connected_user = []
user_list = []

class User:
    channel_id = ""
    access_token = ""

class Connection:
    channel_id = ""
    access_token = ""

    def __init__(self, channel_id, access_token):
        self.channel_id = channel_id
        self.access_token = access_token

    async def listen_to_server(self):
        try:
            async with websockets.connect(server) as websocket:
                nonce = string.ascii_lowercase
                ''.join(random.choice(nonce) for i in range(20))
                message = "{"
                message += "\"type\": \"LISTEN\","
                message += "\"nonce\": \"" + nonce + "\","
                message += "\"data\": {"
                message += "\"topics\":[\"channel-points-channel-v1." + self.channel_id + "\"],"
                message += "\"auth_token\": \"" + self.access_token + "\""
                message += "}"
                message += "}"
                await websocket.send(message)
                while True:
                    server_recv = await websocket.recv()
                    #print (server_recv)
                    data = json.loads(server_recv)
                    if "error" in data:
                        if "ERR_BADMESSAGE" in data["error"]:
                            raise Exception("Error: BADMESSAGE")
                        elif "ERR_BADAUTH" in data["error"]:
                            raise Exception("Error: BADAUTH")
                        elif "ERR_SERVER" in data["error"]:
                            raise Exception("Error: SERVER")
                        elif "ERR_BADTOPIC" in data["error"]:
                            raise Exception("Error: BADTOPIC")
                        elif not data["error"]: # Means response from server is a message. Not an Error
                            continue
                        else: 
                            raise Exception("Error: Unknown")
                    elif "MESSAGE" in data["type"]:
                        message_data = data["data"]["message"]
                        message_data_obj = json.loads(message_data)
                        message_data_obj.pop("type")
                        write_pubsub_file(message_data_obj, self.channel_id, pub_sub_event_folder)
                        write_pubsub_file(message_data_obj, self.channel_id, pub_sub_alert_folder)
                    elif "RECONNECT" in data["type"]:
                        raise Exception("Error: RECONNECT")
        except Exception as e:
            print(e)
            print("Reconnecting to server in 5 seconds")
            await asyncio.sleep(5)
            return await self.listen_to_server()

def write_pubsub_file(json_obj, channel_id, folder):
    new_json_obj = None
    if not os.path.exists(folder + channel_id + ".json"):
        tmp_file = open(folder + channel_id + ".json", "w")
        tmp_file.flush()
        tmp_file.close()

    json_file = open(folder + channel_id + ".json", "r")
    if os.stat(folder + channel_id + ".json").st_size == 0:
        empty_json_obj = []
        empty_json_obj.append(json_obj)
        new_json_obj = empty_json_obj

    else:
        old_json_string = json.loads(json_file.read())
        old_json_obj = old_json_string
        old_json_obj.append(json_obj)
        new_json_obj = old_json_obj
    json_file.close()

    json_file = open(folder + channel_id + ".json", "w")
    json.dump(new_json_obj, json_file)
    json_file.flush()
    json_file.close()

async def ping_to_server():
    while True:
        async with websockets.connect(server) as websocket:
            print ("PING to " + server)
            await websocket.send("{\"type\": \"PING\"}")
            server_recv = await websocket.recv()
            if "PONG" in server_recv:
                print ("PONG received")
            else: 
                print ("No PONG received")
        await asyncio.sleep(random.randint(200,250))

def get_user_list():
    user_list = []
    try:
        json_file = open(os.path.dirname(os.path.realpath(__file__)) + "\\data\\users.json", "r")
    except FileNotFoundError as e:
        print(e)
        print("Creating user file...")
        json_file = open(os.path.dirname(os.path.realpath(__file__)) + "\\data\\users.json", "w")
        json_file.close()
        get_user_list()
    if os.stat(os.path.dirname(os.path.realpath(__file__)) + "\\data\\users.json").st_size == 0:
        json_file.close()
        return
    else:
        try:
            data = json.load(json_file)
            for user in data['users']:
                user_obj = User()
                user_obj.channel_id = user["channel_id"]
                user_obj.access_token = user["access_token"]
                user_list.append(user_obj)
        except Exception as e:
            print(e)
        json_file.close()
        return user_list

def update_new_tokens():
    pass

async def continuing_update_user_list():
    while True:
        new_user_list = get_user_list()
        if new_user_list is None:
            print("No Users in Userfile found")
            await asyncio.sleep(10)
            continue
        adjusted_new_user_list = []
        adjusted_new_user_list.extend(new_user_list)
        #Check if any user is new. If yes, create new user and connect it
        for item in new_user_list:
            for con_item in connected_user:
                if item.channel_id == con_item.channel_id:
                    if item.access_token != con_item.access_token:
                        con_item.access_token = item.access_token
                        print("Updated Access-Token for {}".format(con_item.channel_id))
                    adjusted_new_user_list.remove(item)

        for item in adjusted_new_user_list:
            conn = Connection(item.channel_id, item.access_token)
            connected_user.append(conn)
            print("New connection with ID {}".format(item.channel_id))
            loop.create_task(conn.listen_to_server()) # Task for every User (Connection)
            
        await asyncio.sleep(10)



def main():
    loop.create_task(continuing_update_user_list())
    loop.create_task(ping_to_server())
    loop.run_forever()

main()


