using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using UnityEngine.Networking;
using System;
using System.IO;
using Newtonsoft.Json;

public class Menu : MonoBehaviour
{
    private void Start()
    {
        Userconect user = new Userconect(1, 36, "pp");
       // Userconect.list.Add(1, user);
    }
    //getavater
    private static IEnumerator GetAvatar(ushort IdUser, int id, string name)
    {
        // add user to list user connected
        Userconect user = new Userconect(IdUser, id, name);
        Userconect.list.Add(IdUser, user);
        //Get picture from wweb;
        WWWForm fr = new WWWForm();
        fr.AddField("IDAvatar", id);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/GetAvatarMain.php", fr))
        {
            yield return www.SendWebRequest();
            byte[] data = www.downloadHandler.data;
            int chunklarge = 1222;
            int numChunks = (int)Math.Ceiling((double)data.Length / chunklarge);
            for (int i = 0; i < numChunks; i++)
            {
                Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.AvatarMain);
                int ofset = i * chunklarge;
                if (i == numChunks - 1) chunklarge = data.Length - i * chunklarge;
                byte[] bytes = new byte[chunklarge];
                Array.Copy(data, ofset, bytes, 0, chunklarge);
                message.AddBytes(bytes, true);
                bool isfinish;
                isfinish = i == numChunks - 1;
                message.AddBool(isfinish);
                NetworkManager.Singleton.server.Send(message, IdUser, true);
            }
        }
    }
    [MessageHandler((ushort)ClientToServer.AvatarMain)]
    private static void ReceiveAvatarMain(ushort IDuser, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(GetAvatar(IDuser, message.GetInt(), message.GetString()));
    }

    // Notification list in Menu
    private static IEnumerator Notificationmenu(ushort clientID, int IDuser)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("IDuserNotification", IDuser);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/GetNotificationByNameMenu.php", fr))
        {
            yield return www.SendWebRequest();
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.NotificationMenu);
            message.Add(www.downloadHandler.text);
            NetworkManager.Singleton.server.Send(message, clientID, true);
        }
    }

    [MessageHandler((ushort)ClientToServer.NotificationMenu)]
    private static void ReceiveNotificationMenu(ushort clientId, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(Notificationmenu(clientId, message.GetInt()));
    }

    //Message list in menu
    [MessageHandler((ushort)ClientToServer.ListMessageInMenu)]
    private static void ReceiveListMessageInMenu(ushort IDuser, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(SovleListMessageInMenu(IDuser, message.GetInt(), message.GetInt()));
    }
    private static IEnumerator SovleListMessageInMenu(ushort IDuser, int id, int StartConversation)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("IDUserListConversation", id);
        fr.AddField("StartConersation", StartConversation);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/GetListMessageInMenu.php", fr))
        {
            yield return www.SendWebRequest();
            Message message = Message.Create(MessageSendMode.Reliable, ((ushort)ServerToClient.ListMessageInMenu));
            message.Add(www.downloadHandler.text);
            NetworkManager.Singleton.server.Send(message, IDuser, true);
        }
    }

    //Message chat
    private static IEnumerator SQLGetMessageChatInMenu(ushort UserId, int id, int Start)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("idGetMessageChatInmenu", id);
        fr.AddField("StartMessageChatInMenu", Start);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/GetMessageChatInMenu.php", fr))
        {
            yield return www.SendWebRequest();
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.GetChatMessageInMenu);
            message.AddString(www.downloadHandler.text);
            NetworkManager.Singleton.server.Send(message, UserId, true);
        }
    }
    [MessageHandler((ushort)ClientToServer.GetChatMessageInMenu)]
    private static void ReceiveGetMessageInMenu(ushort UserId, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(SQLGetMessageChatInMenu(UserId, message.GetInt(), message.GetInt()));
    }

    private static IEnumerator SQLSendMessageChat(int IdUserCreate, string namechat, string Name, string message, int room)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("IdUserCrateMessage", IdUserCreate);
        fr.AddField("MessageChat", message);
        fr.AddField("IdChatRomm", room);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/Messagechatinmenu.php", fr))
        {
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;
            string[] Idmember = result.Split(";");
            for (int i = 0; i < Idmember.Length - 1; i++)
            {
                Debug.Log(Idmember[i]);
                ushort id = 0;
                if (Userconect.IdIsOnline(int.Parse(Idmember[i]), ref id))
                {
                    Debug.Log(id);
                    Message messag = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.SendMessageMenu);
                    messag.AddString(namechat);
                    messag.AddString(message);
                    messag.AddString(Name);
                    messag.AddInt(room);
                    NetworkManager.Singleton.server.Send(messag, id, true);
                    break;
                }
            }
        }
    }
    [MessageHandler((ushort)ClientToServer.SendMessageMenu)]
    private static void ReceiveSendMessage(ushort iduser, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(SQLSendMessageChat(message.GetInt(), message.GetString(), message.GetString(), message.GetString(), message.GetInt()));
    }

    //AddFriend request
    private static IEnumerator SQlGetAddFriendRequest(ushort userId, int id)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("IdUserGetAddFriendReques", id);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/GetAddFriendRequest.php", fr))
        {
            yield return www.SendWebRequest();
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.GetAddFriendRequest);
            message.AddString(www.downloadHandler.text);
            NetworkManager.Singleton.server.Send(message, userId, true);
        }
    }
    [MessageHandler((ushort)ClientToServer.GetAddFriendRequest)]
    private static void ReceiveGetAddFreindRequest(ushort userId, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(SQlGetAddFriendRequest(userId, message.GetInt()));
    }
    private static IEnumerator SQLAddFriendRequest(ushort UserId, int IdUserCreate, string nameCreate, int IdUserReceive)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("IdUserCreateAddFriend", IdUserCreate);
        fr.AddField("IdUserReceiveAddFriend", IdUserReceive);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/AddFriendRequest.php", fr))
        {
            yield return www.SendWebRequest();
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.AddFriendRequest);
            message.AddInt(0);
            message.AddString(www.downloadHandler.text);
            message.AddBool(true);
            NetworkManager.Singleton.server.Send(message, UserId, true);
            ushort id = 0;
            if (www.downloadHandler.text == "ok" && Userconect.IdIsOnline(IdUserReceive, ref id))
            {
                Message ms = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.AddFriendRequest);
                ms.AddInt(IdUserCreate);
                ms.AddString(nameCreate);
                ms.AddBool(false);
                NetworkManager.Singleton.server.Send(ms, id, true);
            }
        }
    }
    [MessageHandler((ushort)ClientToServer.AddFriendRequest)]
    private static void ReceiveAddFriendRequest(ushort UserId, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(SQLAddFriendRequest(UserId, message.GetInt(), message.GetString(), message.GetInt()));
    }
    private static IEnumerator SQLReplyAddFriendRequest(ushort userid, int id, int result)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("idRequestAddFriend", id);
        fr.AddField("resultAddFriend", result);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/ReplyAddFriendRequest.php", fr))
        {
            yield return www.SendWebRequest();
            Debug.Log(www.downloadHandler.text);
        }
    }
    [MessageHandler((ushort)ClientToServer.ReplyAddFriendRequest)]
    private static void ReceiveReplyAddFriendRequest(ushort userid, Message message)
    {
        Menu menu = GameObject.FindObjectOfType<Menu>();
        menu.StartCoroutine(SQLReplyAddFriendRequest(userid, message.GetInt(), message.GetInt()));
    }
    // join game
    [MessageHandler((ushort)ClientToServer.JoinRoom)]
    private static void ReceiveJoinRoom(ushort userid, Message message)
    {
        int idroom = message.GetInt(); 
        Userconect player = Userconect.list[userid];
        Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.JoinRoom);
        // if exist room
        if (Room.Rooms.ContainsKey(idroom))
        {
            Room room = Room.Rooms[idroom];
            // success join room
            if (room.Member.Count < 6 && room.Year() == 0) 
            {
                    string inform = JsonConvert.SerializeObject(room.Member);
                    mess.Add(inform);
                    JoinAlreadyRoom(room, player);
            }
            else // full member
            {
                mess.AddString("FullMember");
            }
        }
        else
        {
            //no exist room=> create new room
            new Room(idroom, player, false);
            mess.AddString("NewRoom");
        }
        mess.AddInt(idroom);
        NetworkManager.Singleton.server.Send(mess, userid, true);
    }

    //Playgame now
    [MessageHandler((ushort)ClientToServer.PlayGameNow)]
    private static void ReceivePlayGameNow(ushort userid, Message message)
    {
        Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.PlayGameNow);
        int roomid = Room.idRoomAllowJoin();
        Userconect player = Userconect.list[userid];
        if (Room.Rooms.Count == 0 || roomid == -1)
        {
            new Room(roomid, player, false);
            mess.AddString("NewRoom");
            mess.AddInt(roomid);
        }
        else
        {
            Room room = Room.Rooms[roomid];
            JoinAlreadyRoom(room, player);
            mess.AddInt(roomid);
        }
        NetworkManager.Singleton.server.Send(mess, userid, true);
    }
    private static void JoinAlreadyRoom(Room room, Userconect player)
    {
        
        //Send to other member in room
        Message newmember = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.NewmemberJoinRoom);
        newmember.AddString(JsonConvert.SerializeObject(player));
        foreach (var orthermember in room.Member)
        {
            NetworkManager.Singleton.server.Send(newmember, orthermember.id, true);
        }
        room.AddPlayer(player);
    }
}
  
