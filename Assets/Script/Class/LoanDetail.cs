using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class LoanDetail
{
    private byte id;
    private bool isReal;
    private ushort partieA;
    private ushort partieB;
    private int amount;
    private float rate;
    private byte period;
    public LoanDetail(byte id,ushort partieA, ushort partieB, int amount, byte period,float rate)
    {
        
        this.id = id;
        isReal = false;
        this.partieA = partieA;
        this.partieB = partieB;
        this.amount = amount;
        this.rate = rate;
        this.period = period;
    }
    public byte Id
    {
        get { return id; }
        private set { }
    }
    public bool Isreal
    {
        get { return isReal; }
        set { isReal = value; }
    }
    public ushort PartieA
    {
        get { return partieA; }
        private set { }
    }
    public ushort PartieB
    {
        get { return partieB; }
        private set { }
    }
    public int Amount
    {
        get { return amount; }
        private set { }
    }
    public float Rate
    {
        get { return rate; }
        set { rate = value; }
    }
    public byte Period
    {
        get { return period; }
        private set { }
    }
}
