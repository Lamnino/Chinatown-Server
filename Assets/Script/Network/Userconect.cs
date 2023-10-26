using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

public class Userconect 
{
    public static Dictionary<ushort, Userconect> list { get; private set;  } = new Dictionary<ushort, Userconect>();

    private ushort Id;
    private int Idserver;
    private string name;
    private bool ready;
    private int IsRoom; // futrue if use unconnect and come back the game
    //infromInRoom
    private List<byte> ground = new List<byte>();
    private List<byte> card = new List<byte>();
    private byte color = 0;
    //Banking
    private int cash = 50000;
    private int loan = 0;
    private int deposit = 0;
    //private Dictionary<byte, LoanDetail> loans = new Dictionary<byte, LoanDetail>();
    public Userconect(ushort id, int idServer,string name)
    {
        Id = id;
        Idserver = idServer;
        this.name = name;
        IsRoom = 0;
        for (int i=0; i<12; i++)
        {
            card.Add(0);
        }
    }
    public ushort id
    {
        get { return Id; }
        private set { }
    }
    public int idserver
    {
        get { return Idserver;  }
        private set { }
    }
    public string Name
    {
        get { return name; }
        private set { }
    }
    public bool Ready
    {
        get { return ready; }
        set { ready = value;  }
    }
    public void AddRoom(int IdRoom)
    {
        this.IsRoom = IdRoom;
    }
    public void OutRoom()
    {
        if (IsRoom != 0)
        {
            Room.Rooms[IsRoom].RemovePlayer(this);
            IsRoom = 0;
        }
    }
    //in room
    public List<byte> Ground()
    {
        return ground;
    }
    public void addground(byte grounds)
    {
        if (!ground.Contains(grounds))
            ground.Add(grounds);
    }
    public void removeground(byte grounds)
    {
        if (ground.Contains(grounds))
            ground.Remove(grounds);
    }
    public List<byte> Card()
    {
        return card;
    }
    public void addcard(byte cardd)
    {
        card[cardd]++;
    }
    public void removecard(byte cardd)
    {
        card[cardd]--;
    }
    public byte Color()
    {
        return color;
    }
    public void setColor(byte c)
    {
        color = c;
    }
    public int Cash()
    {
        return cash;
    }
    public void addCash(int amount)
    {
        cash += amount;
    }
    public void addLoan(int amount)
    {
        loan += amount;
    }
    public void addDeposite(int amount)
    {
        deposit += amount;
    }
    public static bool IdIsOnline(int idserver, ref ushort id)
    {
        foreach (var userid in Userconect.list.Values)
        {
            if (userid.Idserver == idserver)
            {
                id = userid.id;
                return true; 
            }
        }
        return false;
    }
}
