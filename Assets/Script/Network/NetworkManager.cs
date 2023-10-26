using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Riptide;
using Riptide.Utils;
using UnityEngine.Networking;
public enum ClientToServer : ushort
{
    Login = 1,
    Signin,
    JoinRoom,
    AvatarMain,
    NotificationMenu,
    ListMessageInMenu,
    GetChatMessageInMenu,
    SendMessageMenu,
    GetAddFriendRequest,
    AddFriendRequest,
    ReplyAddFriendRequest,
    NewmemberJoinRoom,
    PlayGameNow,
    ChoseColor,
    readyMember,
    StartGame,
    seperateCrad,
    commitGround,
    commitechange,
    ReplyExchangeItemRequest,
    actionExchangeItem,
    commitLoan,
    ReplyLoan,
    ReLoan,
    Deposite,
}
public enum ServerToClient : ushort
{
    Login = 1,
    Signin,
    JoinRoom,
    AvatarMain,
    NotificationMenu,
    ListMessageInMenu,
    GetChatMessageInMenu,
    SendMessageMenu,
    GetAddFriendRequest,
    AddFriendRequest,
    ReplyAddFriendRequest,
    NewmemberJoinRoom,
    PlayGameNow,
    ChoseColor,
    readyMember,
    StartGame,
    seperateCrad,
    commitGround,
    commitechange,
    ReplyExchangeItemRequest,
    actionExchangeItem,
    commitLoan,
    ReplyLoan,
    ReLoan,
    Deposite,
}
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager singleton;
    public static NetworkManager Singleton
    {
        get => singleton;
        private set
        {
            if (singleton == null) singleton = value;
            else if (singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exist");
                Destroy(value);
            }
        }
    }
    public Server server { get; private set; }
    [SerializeField] private ushort Port;
    [SerializeField] private ushort MaxClient;
    private void Awake()
    {
        Singleton = this;
    }
    private void Start()
    {
        Application.targetFrameRate = 30;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogWarning, false);
        server = new Server();
        server.Start(Port, MaxClient);
        server.ClientDisconnected += PlayerLeft;
    }
    
    private void FixedUpdate()
    {
        server.Update();    
    }
    private void OnApplicationQuit()
    {
        server.Stop();
        server.ClientDisconnected -= PlayerLeft;
    }
    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        StartCoroutine(UnOnline(Userconect.list[e.Client.Id].idserver)); 
        Userconect.list.Remove(e.Client.Id);
        Userconect.list[e.Client.Id].OutRoom();
    }
    private IEnumerator UnOnline(int id)
    {
        WWWForm fr = new WWWForm();
        fr.AddField("IdPlayerLfet", id);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:81/Game/SetStatePlayerLeft.php", fr))
        {
            yield return www.SendWebRequest();
        }

    }
}
