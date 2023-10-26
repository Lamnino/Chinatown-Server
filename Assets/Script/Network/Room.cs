using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Newtonsoft.Json;
[System.Serializable]
public class Room 
{
    public static Dictionary<int, Room> Rooms = new Dictionary<int,Room>();
    private List<Userconect> member;
    private bool ispublic;
    public List<DepositeDetail> deposites = new List<DepositeDetail>();
    public byte idDeposite = 0;
    //in game
    private List<byte> ground = new List<byte>();
    private List<byte> card = new List<byte>();
    private List<byte> cardValue = new List<byte>();
    private byte year = 0;
    private byte[] color = new byte[5] { 1,2,3,4,5};
    private List<byte>[] groundChose = new List<byte>[5];
    public List<ItemExchange> itemexchanges = new List<ItemExchange>();
    public byte idexchange = 0;
    public List<LoanDetail> Loans = new List<LoanDetail>();
    public byte idLoan = 0;
     public int IdRoom{ get; private set;}
    public bool Ispublic { get; private set;}

    public List<Userconect> Member
    {
        get { return member; }
         set { }
    }
    public Room(int id, string json)
    {
        IdRoom = id;
        member = JsonConvert.DeserializeObject<List<Userconect>>(json);
        setvalueCard();
    }
    public byte[] Color()
    {
        return color;
    }
    public ushort IdManager()
    {
        return member[0].id;
    }
    public List<byte>[] GroundChose()
    {
        return groundChose;
    }
    public void AddGroundChose(List<byte> listid,byte i)
    {
        groundChose[i] = new List<byte>();
        foreach (var id in listid)
        {
            groundChose[i].Add(id);
        }
    }
    public void ClearGroundChose()
    {
        for (int i = 0; i< groundChose.Length; i++)
        {
            if (groundChose[i] != null)
            {
                groundChose[i] = null;
            }
        }
    }
    public void removeColor(int col)
    {
        color[col] = 0;
    }
    private void setvalueCard()
    {
        for (byte i = 1; i <= 88; i++) ground.Add(i);
        byte d =0;
        for (byte i = 0; i <= 3; i++)
        {
            byte k = (byte)(i + 3);
            for (byte j = 1; j <= 3; j++)
            {
                card.Add(k);
                cardValue.Add(d);
                d++;
            }
        }
    }
    public void SetReadyAll(bool ready)
    {
        foreach (var m in member)
        {
            m.Ready = ready;
        }
    }
    public bool alReady()
    {
        foreach (var m in member)
        {
            if (m.Ready == false)
                return false;
        }
        return true;
    }
    public Room(int IdRoom, Userconect player, bool ispublic )
    {
        this.IdRoom = IdRoom;
        Ispublic = ispublic;
        member = new List<Userconect>();
        member.Add(player);
        Rooms.Add(IdRoom, this);
        player.AddRoom(IdRoom);
        setvalueCard();
    }
    public List<byte> Ground()
    {
        return ground;
    }
    public void removeGround(byte i)
    {
        ground.Remove(i);
    }
    public List<byte> Card()
    {
        return card;
    }
    public List<byte> CardValue()
    {
        return cardValue;
    }
    public void removeCard(int i, byte value)
    {
        card[i]--;
        if (card[i] == 0)
        {
            cardValue.Remove(value);
            card.Remove(0);
        }
    }
    public void AddPlayer(Userconect player)
    {
            if (!member.Contains(player))
                member.Add(player);
    }
    public void RemovePlayer(Userconect player)
    {
        if (member.Contains(player))
            member.Remove(player);
        else
            Debug.Log($"none player {player} in this room");
    }
    public byte Year()
    {
        return year;
    }

    public void increaseYear()
    {
        year++;
    }
    public static int idRoomAllowJoin()
    {
        foreach (var room in Rooms)
        {
            if (room.Value.ispublic && room.Value.Year() == 0 && room.Value.Member.Count < 5)
            {
                return room.Key;
            }
        }
        return -1;
    }
}
