using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Riptide;
using System;
public class Login : MonoBehaviour
{
    //Check inf login
    private static IEnumerator Web(ushort IdUser, string email, string password)
    {
        WWWForm wf = new WWWForm();
        wf.AddField("EmailLogin", email);
        wf.AddField("PassLogin", password);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/UserLogin.php", wf))
        {
            yield return www.SendWebRequest();
            SendResutl(www.downloadHandler.text, IdUser);
        }
    }
    private static void SendResutl(string result, ushort IdUser)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.Login);
        message.Add(result);
        message.AddUShort(IdUser);
        NetworkManager.Singleton.server.Send(message, IdUser, true);
    }
    #region Message
    //get avatar
    
    [MessageHandler((ushort)ClientToServer.Login)]
    private static void login(ushort IdUser, Message message)
    {
        Login llogin = GameObject.FindObjectOfType<Login>();
        string Email = message.GetString();
        string pass = message.GetString();
        llogin.StartCoroutine(Web(IdUser, Email, pass));
    }
    #endregion
}

