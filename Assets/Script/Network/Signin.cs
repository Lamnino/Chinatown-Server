using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using UnityEngine.Networking;

public class Signin : MonoBehaviour
{
    //Check infomation in SQL
    private static IEnumerator CheckSignin(ushort ClietnId, string name, string email, string password, string cfpass)
    {
        WWWForm wf = new WWWForm();
        wf.AddField("NameSignin", name);
        wf.AddField("EmailSignin", email);
        wf.AddField("PassSignin", password);
        wf.AddField("CfPassSignin", cfpass);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/Signin.php", wf))
        {
            yield return www.SendWebRequest();
             SendReply(ClietnId, www.downloadHandler.text);
        }
    }
    //Reply to client
    private static void SendReply(ushort ClietnId, string result)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.Signin);
        message.Add(result);
        NetworkManager.Singleton.server.Send(message, ClietnId, true);
    }

    //receive information from client
    [MessageHandler((ushort)ClientToServer.Signin)]
   private static void Received(ushort ClientId, Message message)
    {
        Signin signin = GameObject.FindObjectOfType<Signin>();
        signin.StartCoroutine(CheckSignin(ClientId,message.GetString(), message.GetString(), message.GetString(), message.GetString()));
    }
}
