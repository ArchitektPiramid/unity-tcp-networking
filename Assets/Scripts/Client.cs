using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;


public class Client : MonoBehaviour
{
	public string ipAddress = "127.0.0.1";
	public int port = 54010;

	private TcpClient _client;
	private NetworkStream _netStream = null;
	private byte[] _buffer = new byte[1024];
	private int _bytesReceived = 0;
	private string _receivedMessage = "";

	public Button btnStart = null;
	public Button btnDisconect = null;
	public MessageSystem msgSystem = null;

	private void Awake() {
		btnStart.onClick.AddListener(StartClient);
		btnDisconect.onClick.AddListener(CloseClient);
	}


	public void StartClient() {
		if(_client != null) {
			msgSystem.NewMessage("[CLIENT] Client is already running");
			return;
		}
		try {
			_client = new TcpClient();
			_client.Connect(ipAddress, port);
			msgSystem.NewMessage("[CLIENT] Client started");
		} catch (SocketException) {
			CloseClient();
			msgSystem.NewMessage("SOCKET error. there is no server to connect");
		}
	}

	private void Update() {
		if(!string.IsNullOrEmpty(_receivedMessage)) {
			msgSystem.NewMessage("MSG RECEIVED: " + _receivedMessage);
			_receivedMessage = "";
		}
		if(Input.GetKeyDown(KeyCode.M)) {
			SendMsgToServer("DUPAA");
		}
	}

	public void SendMsgToServer(string message) {
		if(!_client.Connected) { 
			return; 
		}
		_netStream = _client.GetStream();
		_netStream.BeginRead(_buffer, 0, _buffer.Length, MessageReceived, null);
		byte[] msg = Encoding.ASCII.GetBytes(message);

		_netStream.Write(msg, 0, msg.Length);
	}

	public void MessageReceived(IAsyncResult result) {
		if(result.IsCompleted && _client.Connected) {
			_bytesReceived = _netStream.EndRead(result);
			_receivedMessage = Encoding.ASCII.GetString(_buffer, 0, _bytesReceived);
		}
	}

	private void CloseClient() {
		if(_client.Connected) {
			_client.GetStream().Close();
			_client.Close();
			_client = null;
			msgSystem.NewMessage("Client closed");
		}
	}

}