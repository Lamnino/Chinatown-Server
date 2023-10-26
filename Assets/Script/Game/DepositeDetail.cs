public class DepositeDetail
{
    public byte id { get; private set;}
    public ushort idcustomer { get; private set; }
    public int amount { get; private set; }
    public byte period { get; private set; }
    public DepositeDetail(byte Id, ushort idcustomer, int amount, byte period)
    {
        this.id = id;
        this.idcustomer = idcustomer;
        this.amount = amount;
        this.period = period;
    }
}
