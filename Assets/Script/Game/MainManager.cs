using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Newtonsoft.Json;

public class MainManager : MonoBehaviour
{
    private void Awake()
    {
        string json = "[{ \"id\":1,\"idserver\":37,\"Name\":\"Two\",\"Ready\":false},{ \"id\":2,\"idserver\":37,\"Name\":\"Two\",\"Ready\":false},{ \"id\":3,\"idserver\":37,\"Name\":\"Two\",\"Ready\":false}]";
        Room room = new Room(12, json);
        Room.Rooms.Add(room.IdRoom, room);
    }
    private void Start()
    {
       
        //for (byte i=1; i<9; i++)
        //{
        //    room.Member[0].addground(i);
        //    room.Member[1].addground((byte)(i+9));
        //}
        ////store card of player1
        //Userconect player1 = room.Member[0];
        //player1.addcard(1); player1.addcard(1); player1.addcard(1); player1.addcard(5); player1.addcard(6 ); player1.addcard(10);
        ////store card of player 2
        //Userconect player2 = room.Member[1];
        //player2.addcard(2); player2.addcard(1); player2.addcard(6); player2.addcard(6); player2.addcard(9 ); player2.addcard(7);
    }
    [MessageHandler((ushort)ClientToServer.NewmemberJoinRoom)]
    private static void receiveJoinRoom(ushort userid, Message message)
    {
        int idroom = message.GetInt();
        Userconect newmember;
        Userconect.list.TryGetValue(userid, out newmember);
        Message mess = Message.Create(MessageSendMode.Reliable, ((ushort)ServerToClient.NewmemberJoinRoom));
        string json = JsonConvert.SerializeObject(newmember);
        mess.AddString(json);
        Room room;
        Room.Rooms.TryGetValue(idroom, out room);
        foreach ( var member in room.Member)
        {
            NetworkManager.Singleton.server.Send(mess, member.id, true);
        }
        room.AddPlayer(newmember);
    }
    //Member chose color
    [MessageHandler((ushort)ClientToServer.ChoseColor)]
    private static void RecieveChoseColor(ushort userid, Message message)
    {
        int idroom = message.GetInt();
            byte[] color = Room.Rooms[idroom].Color();
            Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.ChoseColor);
            mess.AddBytes(color);
            NetworkManager.Singleton.server.Send(mess, userid, true);
    }
    //Member ready
    [MessageHandler((ushort)ClientToServer.readyMember)]
    private static void RecieveReadyMember(ushort userid, Message message)
    {
        int idRoom = message.GetInt();
        byte color = message.GetByte();
        Room room = Room.Rooms[idRoom];
        List<Userconect> members = room.Member;
        // update color and ready for member
        room.Member.Find(user => user.id == userid).Ready = true;
        room.Member.Find(user => user.id == userid).setColor(color);
        room.removeColor(color-1);
        Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.readyMember);
        mess.AddUShort(userid);
        mess.AddByte(color);
        //check all member is ready
        if (room.alReady() && room.Member.Count>2)
        {
            mess.AddUShort(room.Member[0].id);
        }
        else
        {
            mess.AddUShort(0);
        }
            foreach (var user in members)
        {
             NetworkManager.Singleton.server.Send(mess, user.id, true);
        }
    }

    //StartGame
    [MessageHandler((ushort)ClientToServer.StartGame)]
    private static void ReceiveStartGame(ushort userid, Message message)
    {
        Room room = Room.Rooms[message.GetInt()];
        // start game room
        room.increaseYear();
        Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.StartGame);
        byte[] color = new byte[5];
        int i = 0;
        foreach(var m in room.Member)
        {
            color[i] = m.Color();
            i++;
        }
        // Add color of member to messsage
        mess.AddBytes(color);
        foreach(var m in room.Member)
        {
            NetworkManager.Singleton.server.Send(mess, m.id, true);
        }
        SeperateCard(room);
    }
    static void SeperateCard(Room room)
    {
        byte numground = 0;
        byte numcard = 0;
        room.SetReadyAll(false);
        switch (room.Member.Count)
        {
            case 3:
                if (room.Year() == 1)
                {
                    numground = 7;
                    numcard = 7;
                }
                else
                {
                    numground = 6;
                    numcard = 4;
                }
                break;
            case 4:
                if (room.Year() == 1)
                {
                    numground = 6;
                    numcard = 6;
                }
                else
                {
                    numground = 5;
                    numcard = 3;
                }
                break;
            case 5:
                if (room.Year() < 4)
                {
                    numground = 5;
                    if (room.Year()==1)
                        numcard = 5;
                    else 
                        numcard = 3;
                }
                else
                {
                    numground = 4;
                        numcard = 2;
                }
                break;
        }
        
        foreach (var member in room.Member )
        {
            int[] ground = new int[numground];
            int[] card = new int[numcard];
            for (byte i =0; i< numground; i++)
            {
                int grd = Random.Range(0, room.Ground().Count);
                ground[i] = room.Ground()[grd];
                room.removeGround(room.Ground()[grd]);
            }
            
            for (byte i =0; i< numcard; i++)
            {
                int crd = Random.Range(0, room.Card().Count);
                card[i] = room.CardValue()[crd];
                member.addcard(room.CardValue()[crd]);
                room.removeCard(crd, room.CardValue()[crd]);
            }
            Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.seperateCrad);
            mess.AddString(JsonConvert.SerializeObject(ground));
            mess.AddString(JsonConvert.SerializeObject(card));
            //mess.AddBytes(room.Color());
            NetworkManager.Singleton.server.Send(mess, member.id, true);
        }
    }
    [MessageHandler((ushort)ClientToServer.commitGround)]
    private static void RecieveGroundCommit(ushort userid, Message message)
    {
        byte[] Grounds = JsonConvert.DeserializeObject<byte[]>(message.GetString());
        byte[] GroundLost = JsonConvert.DeserializeObject<byte[]>(message.GetString());
        Room room = Room.Rooms[message.GetInt()];
        Userconect member = room.Member.Find(user => user.id == userid);
        member.Ready = true;
        foreach (var g in GroundLost)
        {
            room.Ground().Add(g);
        }
        foreach (var gr in Grounds)
        {
            member.addground(gr);
        }
        room.AddGroundChose(member.Ground(), (byte)room.Member.FindIndex(user => user.id == member.id));
        if (room.alReady())
        {
            Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.commitGround);
            List<byte>[] Cards = new List<byte>[room.Member.Count];
            for (int i = 0; i < room.Member.Count; i++)
            {
                Cards[i] = room.Member[i].Card();
            }
            mess.AddString(JsonConvert.SerializeObject(room.GroundChose()));
            mess.AddString(JsonConvert.SerializeObject(Cards));
            // set all member to not ready after done chose ground
            room.SetReadyAll(false);
            foreach (var m in room.Member)
            {
                NetworkManager.Singleton.server.Send(mess, m.id, true);
            }
            // clear ground chose to commit
            room.ClearGroundChose();
            foreach (var m in room.Member)
            {
                Debug.Log(m.Ground().Count);
            }
        }
    }
}
