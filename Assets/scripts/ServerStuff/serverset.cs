using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class serverset : MonoBehaviour
{
    public Button button;
    public Button halfTimeButton;
    public Slider slider;
    public const float GAME_LENGTH_IN_SECONDS = 100f;

    private float offset = 0.0f;

    private float game_timer_starts;
    public float game_timer = 0f;
    //MessageType for Scores
    const short RecieveScore = 112;
    const short MessageTime = 144;
    const short GoToAR = 155;

    bool gameStart = false;
    //Dictionary for all players IDs and scores
    Dictionary<int, int> scoresComparison = new Dictionary<int, int>();//0 is score //1 is rank
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        slider.maxValue = GAME_LENGTH_IN_SECONDS;
        button.onClick.AddListener(() => StartGame());
        halfTimeButton.onClick.AddListener(() => SendARMessage());
    }

    void StartGame()
    {
        gameStart = true;
        game_timer_starts = Time.time;
    }
    //Register Server and handlers for callbacks
    void Start()
    {
        NetworkServer.Listen(8888);
        Debug.Log("Registering server callbacks");
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(RecieveScore, ReceieveScore);
    }

    public class ARTime:MessageBase
    {
        public bool s;
    }

    void SendARMessage()
    {
        ARTime art = new ARTime();
        art.s = true;
        NetworkServer.SendToAll(GoToAR, art);

    }

    //Get Score from client and update in dictionary and send rank
    void ReceieveScore(NetworkMessage netMsg)
    {
        Debug.Log("Receiving Message from" + netMsg.conn.connectionId);
        SendDamageDone dr = netMsg.ReadMessage<SendDamageDone>();
        Debug.Log("Score" + dr.score);
        scoresComparison[netMsg.conn.connectionId] = dr.score;
        int rank = GetRankVal(netMsg.conn.connectionId);
        GetRank gr = new GetRank();
        gr.rank = rank;
        gr.device_name = dr.device_name;
        netMsg.conn.Send(RecieveScore, gr);
        Debug.Log("Score Sent");
    }

    //Claculate rank
    private int GetRankVal(int index)
    {
        Dictionary<int, int>.ValueCollection listScores = scoresComparison.Values;
        List<int> scores = new List<int>();
        foreach (int i in listScores)
        {
            scores.Add(i);
        }
        scores.Sort((a, b) => -1 * a.CompareTo(b));
        int r = scores.IndexOf(scoresComparison[index]);
        Debug.Log("Rank calculated");
        return (r + 1);
    }

    //MessageBase for incoming damage from client
    public class SendDamageDone : MessageBase
    {
        public string device_name;
        public int score;
    }

    //MessageBase for outgoing rank to client
    public class GetRank : MessageBase
    {
        public string device_name;
        public int rank;
    }

    public class Timer : MessageBase
    {
        public float timer;
    }

    //Add into dictionary when new client is connected
    void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Client " + netMsg.conn.connectionId + " connected");
        scoresComparison.Add(netMsg.conn.connectionId, 0);
    }

    //Pop from dictionary when client disconnects
    void OnDisconnected(NetworkMessage netMsg)
    {
        scoresComparison.Remove(netMsg.conn.connectionId);
        Debug.Log("Client" + netMsg.conn.connectionId + "disconnected");
    }

    void SendGameTimer(float t)
    {
        Timer ti = new Timer();
        ti.timer = t;
        NetworkServer.SendToAll(MessageTime, ti);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            if (game_timer >= 100.0f)
            {
                game_timer_starts = game_timer;
                game_timer = 0.0f;
            }
            SendGameTimer(game_timer);
            if (SliderUISet.instance.isDragged)
            {
                game_timer = slider.value;
                offset = slider.value- SliderUISet.instance.time;
            }
            else
            {
                slider.value = game_timer;
                game_timer = offset + Time.time - game_timer_starts;

            }
        }
        else
        {
            Debug.Log("Not started");
        }
    }
}
