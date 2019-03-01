using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

public class Server : MonoBehaviour
{
	public string ipAddress = "127.0.0.1";
	public int port = 54010;
	public float waitingMassagesFrequency = 5f;
	public string responseMsg = "Close";

	private TcpListener _tcpListener = null;
	public List<TcpClient> _clients = new List<TcpClient>();
	private TcpClient m_client = null;
	private NetworkStream m_netStream = null;

	private byte[] _buffer = new byte[1024];
	private int _bytesReceived = 0;
	private string _msgReceived = string.Empty;
	private IEnumerator _ClientComCoroutine = null;

	public Button btnStart = null;
	public MessageSystem msgSystem = null;
	public TMPro.TextMeshProUGUI txtClientsInfo = null;

	private Queue<SingleMsg> myMsgQ = new Queue<SingleMsg>();

	public void StartServer(string ipAddress = "", int port = 80) {
		_tcpListener = new TcpListener(ParseIpAddress(ipAddress), port);
		_tcpListener.Start();
		msgSystem.NewMessage("[SERVER] server started");
		_tcpListener.BeginAcceptTcpClient(ClientConnected, null);
	}

	private void Start() {
		btnStart.onClick.AddListener(() => { StartServer(ipAddress, port); });
		// _ClientComCoroutine = this.ClientCommunication();
		// StartCoroutine(_ClientComCoroutine);
	}

	private void Update() {
		if(myMsgQ.Count > 0) {
			msgSystem.NewMessage(myMsgQ.Dequeue().ToString());
		}
		if(m_client != null && _ClientComCoroutine == null) {
		 	_ClientComCoroutine = this.ClientCommunication();
		 	StartCoroutine(_ClientComCoroutine);
		 }

		if(m_client != null && m_client.Connected) {
			txtClientsInfo.text = "ON " + m_client.Available;
			
		} else {
			txtClientsInfo.text = "OFF";
		}
	}

	private IPAddress ParseIpAddress(string ipAddressToParse) {
		return IPAddress.Parse(ipAddress); //TODO: do trparse
	}

	private void ClientConnected(IAsyncResult res) {
		m_client = _tcpListener.EndAcceptTcpClient(res);
		_clients.Add(m_client);
		string ip = ((IPEndPoint)m_client.Client.RemoteEndPoint).Address.ToString();
		myMsgQ.Enqueue(new SingleMsg("CLIENT", "new client connected " + ip));
	

		//msgSystem.NewMessage("[CLIENT] NEW CLIENT CONNECTED");
        //set the client reference
       
		//_clients.Add(_tcpListener.EndAcceptTcpClient(res));
    }



	private IEnumerator ClientCommunication() {

		_bytesReceived = 0; //TODO: in method reset this
		_buffer = new byte[1024];

		m_netStream = m_client.GetStream();
				//m_netStream = _clients[0].GetStream();

		do
		{
			// byte[] bfer = new byte[m_client.ReceiveBufferSize];
			// int readbytes = m_netStream.Read(bfer, 0, m_client.ReceiveBufferSize);
			// string data = Encoding.ASCII.GetString(bfer, 0, readbytes);
			// msgSystem.NewMessage("inc data: " + data);
			m_netStream.BeginRead(_buffer, 0, _buffer.Length, MessageReceived, m_netStream);
			if(_msgReceived.Length > 0) {
				myMsgQ.Enqueue(new SingleMsg("msg", _msgReceived));
			 	_msgReceived = string.Empty;
			}

			// if(_bytesReceived > 0) {
			// 	NewMessage("[MSG]: " + _msgReceived);
			// 	_msgReceived = string.Empty;
				
			// }
			yield return null;

		} while (_bytesReceived >= 0 && m_netStream != null);
		CloseConnection();
	}

	private void MessageReceived(IAsyncResult result) {
		if(result.IsCompleted && m_client.Connected) {
			_bytesReceived = m_netStream.EndRead(result);
			_msgReceived = Encoding.ASCII.GetString(_buffer, 0, _bytesReceived);
		}
	}

	private void CloseConnection() {
		msgSystem.NewMessage("[SERVER] Close connection from client");
		StopCoroutine(_ClientComCoroutine);
		_ClientComCoroutine = null;
		m_client.Close();
		m_client = null;

		_tcpListener.BeginAcceptTcpClient(ClientConnected, null);
	}

	public void CloseServer() {
		msgSystem.NewMessage("[SERVER] SERVER CLOSED!");
		if(m_client != null) {
			m_netStream.Close();
			m_netStream = null;
			m_client.Close();
			m_client = null;
		}
		if(_tcpListener != null) {
			_tcpListener.Stop();
			_tcpListener = null;
		}
	}
}

public class SingleMsg {
	public string msgTag;
	public string message;

	public SingleMsg(string tag, string msg) {
		msgTag = tag;
		message = msg;
	}

	public override string ToString() {
		return "[" + msgTag + "] " + message; 
	}
}