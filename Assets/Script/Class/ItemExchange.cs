using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class ItemExchange
{
    private int id;
    private ushort idsent;
    private ushort idreceive;
    private List<byte> groundChose = new List<byte>();
    private List<byte> groudGet = new List<byte>();
    private List<byte> cardChose = new List<byte>();
    private List<byte> cardGet = new List<byte>();
    private int money;
    private int moneyGet;
    public ItemExchange(int id, ushort idsent, ushort idreceive, byte[] groundChose, byte[] groudGet, byte[] cardChose, byte[] cardGet, int money, int moneyGet)
    {
        this.id = id;
        this.idsent = idsent;
        this.idreceive = idreceive;
        this.groundChose = groundChose.ToList();
        this.groundChose.Sort();
        this.groudGet = groudGet.ToList();
        this.groudGet.Sort();
        this.cardChose = cardChose.ToList();
        this.cardGet = cardGet.ToList();
        this.money = money;
        this.moneyGet = moneyGet;
    }
    public int Id
    {
        get { return id; }
        private set { }
     }
    public ushort IdSent
    {
        get { return idsent; }
        private set { }
    }
    public ushort IdReceive
    {
        get { return idreceive; }
        private set { }
    }
    public List<byte> GrousdChose
    {
        get { return groundChose; }
        private set { }
    }
    public List<byte> GrousdGet
    {
        get { return groudGet; }
        private set { }
    }
    public List<byte> CardChose
    {
        get { return cardChose; }
        private set { }
    }
    public List<byte> CardGet
    {
        get { return cardGet; }
        private set { }
    }
    public int Money
    {
        get { return money; }
        private set { }
    }
    public int MoneyGet
    {
        get { return moneyGet; }
        private set { }
    }
}
