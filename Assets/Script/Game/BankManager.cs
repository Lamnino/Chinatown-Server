using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Riptide;
using Newtonsoft.Json;

public class BankManager : MonoBehaviour
{
    
    [MessageHandler((ushort)ClientToServer.commitechange)]
    private static void Receivecommitchangeitem(ushort userid,Message mess)
    {
        // Get information
        int idroom = mess.GetInt();
        ushort idRecieve = mess.GetUShort();
        byte[] groundChose = mess.GetBytes();
        byte[] groundChoseGet = mess.GetBytes();
        byte[] cardChose = mess.GetBytes();
        byte[] cardChoseGet = mess.GetBytes();
        int money = mess.GetInt();
        int moneyGet = mess.GetInt();
        string check = checkitem(idroom, userid, idRecieve, groundChose, groundChoseGet, cardChose, cardChoseGet, money, moneyGet);
            Message messsage = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.commitechange);
        // if exchange request is success
        if (check == "done")
        {
            Room room = Room.Rooms[idroom];
            ItemExchange newItemexchange = new ItemExchange(room.idexchange, userid, idRecieve, groundChose, groundChoseGet, cardChose, cardChoseGet, money, moneyGet);
            room.itemexchanges.Add(newItemexchange);
            room.idexchange++;
            string json = JsonConvert.SerializeObject(newItemexchange);
            messsage.AddString(json);
            NetworkManager.Singleton.server.Send(messsage, idRecieve, true);
            NetworkManager.Singleton.server.Send(messsage, userid, true);
        }
        else
        {
            messsage.AddString(check);
            NetworkManager.Singleton.server.Send(messsage, userid, true);
        }
    }
    private static string checkitem(int idroom, ushort idsent, ushort idreceive, byte[] groundChose, byte[] groudGet, byte[] cardChose, byte[] cardGet, int money, int moneyGet)
    {
        Room room = Room.Rooms[idroom];
        Userconect senter = room.Member.Find(user => user.id == idsent);
        Userconect receiver = room.Member.Find(user => user.id == idreceive);
        //check ground chose
        foreach(byte gr in groundChose)
        {
            if (!senter.Ground().Contains(gr)) return "Your ground were used";
        }
        //check ground get
        foreach (byte gr in groudGet)
        {
            if (!receiver.Ground().Contains(gr)) return "Friend's ground were used";
        }
        //check money
        if (senter.Cash() < money) return "Your money is lower than your request";
        if (receiver.Cash() < moneyGet) return "Friend's money is lower than your request";
        //check card store
         for (int i=0; i<12; i++)
        {
            if (senter.Card()[i] - cardChose[i] < 0) return "Your store card were used";
            if (receiver.Card()[i] - cardGet[i] < 0) return "Friend's store card were used";
        }
        return "done";
    }
    [MessageHandler((ushort)ClientToServer.ReplyExchangeItemRequest)]
    private static void ReceiveReplyExchangeItemRequest(ushort userid, Message mess)
    {
        int idroom = mess.GetInt();
        byte idRequest = mess.GetByte();
        bool result = mess.GetBool();
        Room room = Room.Rooms[idroom];
        ItemExchange RequestExchange = room.itemexchanges.Find(item => item.Id == idRequest);
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.ReplyExchangeItemRequest);
        if (checkitem(idroom,RequestExchange.IdSent,RequestExchange.IdReceive, RequestExchange.GrousdChose.ToArray(),
            RequestExchange.GrousdGet.ToArray(), RequestExchange.CardChose.ToArray(), RequestExchange.CardGet.ToArray(), RequestExchange.Money,
            RequestExchange.MoneyGet) == "done")
        {
            //change information of parties
            if (result)
            {
                Userconect sent = room.Member.Find(user => user.id == RequestExchange.IdSent);
                Userconect receive = room.Member.Find(user => user.id == RequestExchange.IdReceive);
                // add ground to receive
                if (RequestExchange.GrousdChose.Count > 0)
                {
                    foreach (var gr in RequestExchange.GrousdChose)
                    {
                        receive.addground(gr);
                        sent.removeground(gr);
                    }
                }
                //add ground to sent
                if (RequestExchange.GrousdGet.Count > 0)
                {
                    foreach (var gr in RequestExchange.GrousdGet)
                    {
                        sent.addground(gr);
                        receive.removeground(gr);
                    }
                }
                for (byte i=0; i <12; i++)
                {
                //add card to receive
                    if (RequestExchange.CardChose[i] > 0)
                    {
                        receive.addcard(i);
                        sent.removecard(i);
                    }
                //add card to sent
                    if (RequestExchange.CardGet[i] > 0)
                    {
                        sent.addcard(i);
                        receive.removecard(i);
                    }
                }
                //add money to sent
                sent.addCash(RequestExchange.MoneyGet);
                receive.addCash(-RequestExchange.MoneyGet);
                //add moeny to receive
                receive.addCash(RequestExchange.MoneyGet);
                sent.addCash(-RequestExchange.MoneyGet);
            }
            //create a message
            message.AddByte(idRequest);
            message.AddBool(result);
            //sent result to change in client
            NetworkManager.Singleton.server.Send(message, RequestExchange.IdSent, true);
            NetworkManager.Singleton.server.Send(message, userid, true);
            Message messagee = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.actionExchangeItem);
            //sent action to another member in room
            string requestexhcnagejson = JsonConvert.SerializeObject(RequestExchange);
            messagee.AddString(requestexhcnagejson);
            foreach(Userconect mem in room.Member)
            {
                if (mem.id != RequestExchange.IdSent && mem.id != userid)
                {
                    NetworkManager.Singleton.server.Send(messagee, mem.id, true);
                }
            }
            //remove request exchange
            room.itemexchanges.Remove(RequestExchange);
        }
        else
        {
            message.AddByte(idRequest);
            message.AddBool(false);
            NetworkManager.Singleton.server.Send(message, userid, true);
            NetworkManager.Singleton.server.Send(message, RequestExchange.IdSent, true);
        }
    }
    [MessageHandler((ushort)ClientToServer.commitLoan)]
    private static void ReceivecommitLoan(ushort userid, Message message)
    {
        int idroom = message.GetInt();
        int amount = message.GetInt();
        byte period = message.GetByte();
        ushort partieB = message.GetUShort();
        float rate = message.GetFloat();
        Room room = Room.Rooms[idroom];
        LoanDetail newLoan = new LoanDetail(room.idLoan, userid, partieB, amount, period,rate);
        room.idLoan++;
            Message mess1 = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.commitLoan);
       if (partieB == 0)
        {
             room.Loans.Add(newLoan);
            mess1.Add(JsonConvert.SerializeObject(newLoan));
            mess1.AddBool(true);
            mess1.AddBool(true);
            newLoan.Rate = 9.87f;
;           room.Member.Find(item => item.id == userid).addCash(newLoan.Amount);
            room.Member.Find(item => item.id == userid).addLoan(newLoan.Amount);
            NetworkManager.Singleton.server.Send(mess1, userid, true);
            // commutLoan: id = partieA && true Bank loan success 
        }
        else
        {
            if (room.Member.Find(item => item.id == partieB).Cash() < newLoan.Amount)
            {
                mess1.AddString(JsonConvert.SerializeObject(newLoan));
                mess1.AddBool(false);
                NetworkManager.Singleton.server.Send(mess1, userid, true);
                // commitLoan id = partieA && false => Fail to request Loan
            }
            else
            {
                mess1.AddString(JsonConvert.SerializeObject(newLoan));
                mess1.AddBool(true);
                mess1.AddBool(false);
                room.Loans.Add(newLoan);
                NetworkManager.Singleton.server.Send(mess1, userid, true);
                NetworkManager.Singleton.server.Send(mess1, partieB, true);
                // commitLoan id == partieB => Spawn request to reply
            }
        }
    }
    [MessageHandler((ushort)ClientToServer.ReplyLoan)]
    private static void ReceiveReplyLoan(ushort userid, Message mess)
    {
        Room room = Room.Rooms[mess.GetInt()];
        byte idLoan = mess.GetByte();
        LoanDetail loan = room.Loans.Find(item => item.Id == idLoan);
        bool result = mess.GetBool();
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.ReplyLoan);
        message.AddByte(loan.Id);
        message.AddBool(result);
        if (!result)
        {
            // refuse so delete requset and send result to partieA
            NetworkManager.Singleton.server.Send(message, loan.PartieA, true);
            NetworkManager.Singleton.server.Send(message, userid, true);
            room.Loans.Remove(loan);
        }
        else
        {
            bool ischangerate = mess.GetBool();
            if (ischangerate)
            {
                message.AddBool(true);
                float rate = mess.GetFloat();
                message.AddFloat(rate);
                loan.Rate = rate;
            }
            else
            {
                message.AddBool(false);
                loan.Isreal = true;
                room.Member.Find(item => item.id == loan.PartieA).addCash(loan.Amount);
                room.Member.Find(item => item.id == loan.PartieA).addLoan(loan.Amount);
                room.Member.Find(item => item.id == loan.PartieB).addCash(-loan.Amount);
                room.Member.Find(item => item.id == loan.PartieB).addDeposite(loan.Amount);
            }
                NetworkManager.Singleton.server.Send(message, userid, true);
            NetworkManager.Singleton.server.Send(message, loan.PartieA, true);
        }
    }
    [MessageHandler((ushort)ClientToServer.ReLoan)]
    private static void ReceiveReLoan(ushort userid, Message mess)
    {
        Room room = Room.Rooms[mess.GetInt()];
        LoanDetail loan = room.Loans.Find(item => item.Id == mess.GetByte());
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.ReLoan);
        message.AddByte(loan.Id);
        bool result = mess.GetBool();
        message.AddBool(result);
        if (result)
        {
            loan.Isreal = true;
            room.Member.Find(item => item.id == loan.PartieA).addCash(loan.Amount);
            room.Member.Find(item => item.id == loan.PartieA).addLoan(loan.Amount);
            room.Member.Find(item => item.id == loan.PartieB).addCash(-loan.Amount);
            room.Member.Find(item => item.id == loan.PartieB).addDeposite(loan.Amount);
        }
        else
        {
            room.Loans.Remove(loan);
        }
        NetworkManager.Singleton.server.Send(message, userid,true);
        NetworkManager.Singleton.server.Send(message, loan.PartieB,true);
    }
    [MessageHandler((ushort)ClientToServer.Deposite)]
    private static void ReceiveDeposite(ushort userid, Message message)
    {
        Room room = Room.Rooms[message.GetInt()];
        Userconect mem = room.Member.Find(item => item.id == userid);
        int amount = message.GetInt();
            Message mess = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.Deposite);
        if (mem.Cash() >= amount)
        {
            mem.addDeposite(amount);
            mem.addCash(-amount);
            Debug.Log(mem.Cash());
            DepositeDetail newsoposite = new DepositeDetail(room.idDeposite, userid, amount, (byte)(room.Year() + mess.GetByte()));
            room.idDeposite++;
            room.deposites.Add(newsoposite);
            mess.AddBool(true);
            mess.AddString(JsonConvert.SerializeObject(newsoposite));
            NetworkManager.Singleton.server.Send(mess, userid, true);
        }
        else
        {
            mess.AddBool(false);
            NetworkManager.Singleton.server.Send(mess, userid, true);
        }
    }
}

