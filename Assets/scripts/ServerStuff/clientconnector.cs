using UnityEngine;
using System.Collections;
using UnityEngine.Networking; 
using UnityEngine.UI;


public class clientconnector : MonoBehaviour {
	

	public Text infotext;
	private int hostId;
	// Use this for initialization


	void Start () {
		NetworkTransport.Init();
		ConnectionConfig config = new ConnectionConfig();
		int myReiliableChannelId  = config.AddChannel(QosType.Reliable);
		int myUnreliableChannelId = config.AddChannel(QosType.Unreliable);
		HostTopology topology = new HostTopology(config, 10);
		hostId = NetworkTransport.AddWebsocketHost(topology,8888,"128.237.191.178");
	//	NetworkWriter nwriter;

	
		Debug.Log("hostId:"+hostId);

		}
	
	// Update is called once per frame
	void Update()
	{
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[1024]; 
		int bufferSize = 1024;
		int dataSize;
		byte error;
		int myhostId;

		NetworkEventType recData = NetworkTransport.Receive(out myhostId, out connectionId, out channelId, 
			recBuffer, bufferSize, out dataSize, out error);
		
		switch (recData)
		{
		case NetworkEventType.Nothing:
			break;
		case NetworkEventType.ConnectEvent:
			string current_ip;
			UnityEngine.Networking.Types.NetworkID nwid;
			UnityEngine.Networking.Types.NodeID nodeid;
			int c_port;
			NetworkTransport.GetConnectionInfo(hostId,connectionId,out current_ip,out c_port,out nwid,out nodeid,out error);
			Debug.Log(current_ip);
			break;
		case NetworkEventType.DataEvent:       
			break;
		case NetworkEventType.DisconnectEvent: 
			break;
		}
	}
}
