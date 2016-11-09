using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;
using System.IO;

public class clientset : MonoBehaviour
{

    //New Client
    NetworkClient myClient;
    //MessageType for receiving rank
    const short ReceieveData = 112;
    const short MessageTime = 144;
    const short GoToAR = 155;
    const short Reset = 166;
    //int i = 0;
    int score;
    const int port = 8888;
    bool halfTimeBool = false;
    public string deviceId = "1002";

    float gameTimer;

    public static clientset instance = null;
    private int intend_port;
    private string intend_ip;

    //MessageBase for outgoign damage
    public class SendDamageDone : MessageBase
    {
        public string device_name;
        public int score;
    }

    //MessageBase for incoming rank
    public class GetRank : MessageBase
    {
        public string device_name;
        public int rank;
    }

    public class ResetScore : MessageBase
    {
        public bool reset;
    }

    void GetARMessage(NetworkMessage netMsg)
    {
        ARTime art = netMsg.ReadMessage<ARTime>();
        halfTimeBool = art.s;
        Debug.Log("gOT tO hALF Time");
    }


    public class ARTime : MessageBase
    {
        public bool s;
    }

    void GetResetScores(NetworkMessage netMsg)
    {
        ResetScore rs = netMsg.ReadMessage<ResetScore>();
        if(rs.reset)
        {
                File.WriteAllText(Path.Combine(Application.dataPath, "playerSave.json"), "0");
        }
        Debug.Log("Game ended. Player Score JSON reset");
    }


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        if (!File.Exists(Path.Combine(Application.dataPath, "playerSave.json")))
        {
            File.WriteAllText(Path.Combine(Application.dataPath, "playerSave.json"), "0");
        }
        score = int.Parse(File.ReadAllText(Path.Combine(Application.dataPath, "playerSave.json")));
        Debug.Log("On loading score:" + score);
    }


    //Connect with server and register callbacks from server
    void ConnectIt()
    {
        //intend_port=System.Convert.ToInt32(port_input.text);
        //intend_ip=ip_input.text.ToString();
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        myClient.RegisterHandler(ReceieveData, ReceieveRank);
        myClient.RegisterHandler(MessageTime, GetGameTimer);
        myClient.RegisterHandler(GoToAR, GetARMessage);
        myClient.RegisterHandler(Reset, GetResetScores);
        myClient.Connect("128.2.236.108", 8888);
    }

    //Connected with server
    void OnDisconnected(NetworkMessage netMsg)
    {
        string data = score.ToString();
        File.WriteAllText(Path.Combine(Application.dataPath, "playerSave.json"), data);
        Debug.Log("Saved");
    }

    void Start()
    {
        ConnectIt();
    }

    //Temp for sending message

    //CALL ON TAPPING ON MONSTER
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SendScoreData();
        }
    }

    //Connected with server
    void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
        SendDamageDone msg = new SendDamageDone();
        msg.device_name = deviceId;
        msg.score = score;
        myClient.Send(ReceieveData, msg);
        //info_text.text="Connected to server";
    }

    //Send score to server
    public void SendScoreData()
    {
        Debug.Log("send score data here...");
        score += 100;
        string data = score.ToString();
        File.WriteAllText(Path.Combine(Application.dataPath, "playerSave.json"), data);
        Debug.Log("Written score is" + data);
        SendDamageDone msg = new SendDamageDone();
        msg.device_name = deviceId;
        msg.score = score;
        //Debug.Log(score);
        myClient.Send(ReceieveData, msg);
    }

    public class Timer : MessageBase
    {
        public float timer;
    }

    public void GetGameTimer(NetworkMessage netMsg)
    {
        Timer t = netMsg.ReadMessage<Timer>();
        gameTimer = t.timer;
    }

    public float ReturnGameTimer()
    {
        return gameTimer;
    }

    //Incoming rank
    void ReceieveRank(NetworkMessage netMsg)
    {
        GetRank msg = netMsg.ReadMessage<GetRank>();
        Debug.Log("Data is " + msg.rank + " " + msg.device_name);
    }
}
