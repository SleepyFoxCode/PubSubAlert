import asyncio
import websockets
import time
import random
import string
import json
import os
from datetime import datetime

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
                    data = json.loads(server_recv)
                    if "error" in data:
                        if "ERR_BADMESSAGE" in data["error"]:
                            raise Exception("Error: BADMESSAGE for user:{}".format(self.channel_id))
                        elif "ERR_BADAUTH" in data["error"]:
                            raise Exception("Error: BADAUTH for user:{}".format(self.channel_id))
                        elif "ERR_SERVER" in data["error"]:
                            raise Exception("Error: SERVER for user:{}".format(self.channel_id))
                        elif "ERR_BADTOPIC" in data["error"]:
                            raise Exception("Error: BADTOPIC for user:{}".format(self.channel_id))
                        elif not data["error"]: # When error in response is empty it was successful 
                            log_to_console("Successful connection for user:{}".format(self.channel_id))
                            continue
                        else: 
                            raise Exception("Error: Unknown error message for user:{}".format(self.channel_id))
                    elif "MESSAGE" in data["type"]:
                        log_to_console("New data for user:{}".format(self.channel_id))
                        message_data = data["data"]["message"]
                        message_data_obj = json.loads(message_data)
                        message_data_obj.pop("type")
                        write_pubsub_file(message_data_obj, self.channel_id, pub_sub_event_folder)
                        write_pubsub_file(message_data_obj, self.channel_id, pub_sub_alert_folder)
                    elif "RECONNECT" in data["type"]:
                        raise Exception("Error: RECONNECT for user:{}".format(self.channel_id))
                    else:
                        raise Exception("Error: Unknown while reading json data we received for user:{}".format(self.channel_id))
        except Exception as e:
            log_to_console(e)
            log_to_console("Reconnecting {} to {} in 5 seconds".format(self.channel_id, server))
            await asyncio.sleep(5)
            return await self.listen_to_server()

def write_pubsub_file(json_obj, channel_id, folder):

    #This json will be dumped in the end to the file
    updated_json_obj = None
    # if channel id file not exist we need to create it
    if not os.path.exists(folder + channel_id + ".json"):
        newFile = open(folder + channel_id + ".json", "w")
        newFile.flush()
        newFile.close()

    # File is empty so we create an empty json list
    json_file = open(folder + channel_id + ".json", "r")
    if os.stat(folder + channel_id + ".json").st_size == 0:
        json_list = []
        json_list.append(json_obj)
        updated_json_obj = json_list

    # File is not empty so we get the whole json data, append the new data to it, and dump it all back
    else:
        old_json = json.loads(json_file.read())
        old_json_obj = old_json
        old_json_obj.append(json_obj)
        updated_json_obj = old_json
    json_file.close()

    json_file = open(folder + channel_id + ".json", "w")
    json.dump(updated_json_obj, json_file)
    json_file.flush()
    json_file.close()

# We need to ping the twitch server at least once in 5 minutes to show it that we are still connected
async def ping_to_server():
    while True:
        async with websockets.connect(server) as websocket:
            log_to_console ("PING to " + server)
            await websocket.send("{\"type\": \"PING\"}")
            server_recv = await websocket.recv()
            if "PONG" in server_recv:
                log_to_console ("PONG received")
            else: 
                log_to_console ("No PONG received")
        await asyncio.sleep(random.randint(200,250))

def get_user_list():
    user_list = []
    try:
        json_file = open(os.path.dirname(os.path.realpath(__file__)) + "\\data\\users.json", "r")
    # User File doesn't exist so we create one and call get_user_list() again
    except FileNotFoundError as e:
        log_to_console(e)
        log_to_console("Creating user file...")
        json_file = open(os.path.dirname(os.path.realpath(__file__)) + "\\data\\users.json", "w")
        json_file.close()
        get_user_list()
    # No users in user list
    if os.stat(os.path.dirname(os.path.realpath(__file__)) + "\\data\\users.json").st_size == 0:
        json_file.close()
        return None
    else:
        try:
            # We create a user object for every entry
            data = json.load(json_file)
            for user in data['users']:
                user_obj = User()
                user_obj.channel_id = user["channel_id"]
                user_obj.access_token = user["access_token"]
                user_list.append(user_obj)
            json_file.close()
            return user_list
        except Exception as e:
            log_to_console(e)
            json_file.close()
            return None

async def loop_update_connected_list():
    while True:
        new_user_list = get_user_list()
        # No users in user file
        if new_user_list is None:
            await asyncio.sleep(5)
            continue
        adjusted_new_user_list = []
        adjusted_new_user_list.extend(new_user_list)
        #Check if any user is new or has a new access token. If true, create new user/change access token and connect it
        for item in new_user_list:
            for con_item in connected_user:
                if item.channel_id == con_item.channel_id:
                    # user already exists as connection. We check if the access token is different
                    if item.access_token != con_item.access_token:
                        con_item.access_token = item.access_token
                        log_to_console("Updated Access-Token for {}".format(con_item.channel_id))
                    # We remove the user from the adjusted list since it already exists as connection
                    adjusted_new_user_list.remove(item)
        # adjusted_new_user_list now has only the users which aren't connected
        for item in adjusted_new_user_list:
            conn = Connection(item.channel_id, item.access_token)
            connected_user.append(conn)
            log_to_console("New connection with ID {}".format(item.channel_id))
            # We connect the user
            loop.create_task(conn.listen_to_server()) 
        await asyncio.sleep(5)

def log_to_console(string):
    now = datetime.now()
    print("{datetime}: {string}".format(datetime=now,string=string))


def main():
    loop.create_task(loop_update_connected_list())
    loop.create_task(ping_to_server())
    loop.run_forever()

main()


