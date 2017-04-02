﻿using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetworkController : MonoBehaviour {

    private Thread m_ReceiveThread;
    private TcpListener m_TcpListener;
    private Socket m_ClientSocket;
    private bool m_IsShutdown = false;

    [SerializeField]
    private String m_ServerIP = "127.0.0.1";
    [SerializeField]
    private int m_Port = 8888;
    [SerializeField]
    private uint m_RecvBufferSize = 4;

    [SerializeField]
    private ObservationProducer m_ObservationProducer;

    void Start()
    {
        if (Application.isEditor) { 
            Application.runInBackground = true;
        }
        m_ReceiveThread = new Thread(new ThreadStart(receiveData));
        m_ReceiveThread.IsBackground = true;
        m_ReceiveThread.Start();
    }

    void Update()
    {
        m_ObservationProducer.GetObservation();
    }

    void OnApplicationQuit()
    {
        m_IsShutdown = true;
        m_TcpListener.Stop();
        m_ReceiveThread.Join();
    }

    private void receiveData()
    {
        m_TcpListener = new TcpListener(IPAddress.Parse(m_ServerIP), m_Port);
        m_TcpListener.Start();

        byte[] recvBuffer = new byte[m_RecvBufferSize];
        while (!m_IsShutdown)
        {
            Debug.Log("Waiting for client...");
            m_ClientSocket = m_TcpListener.AcceptSocket();
            Debug.Log("Connected.");

            while (!m_IsShutdown && IsSocketAlive(m_ClientSocket)) {
                m_ClientSocket.Receive(recvBuffer);
                
                int recvData = BitConverter.ToInt32(recvBuffer, 0);

                Debug.Log("Receive: " + recvData);
            }
             m_ClientSocket.Close();
            Debug.Log("Disconnected.");
        }
        m_TcpListener.Stop();
        Debug.Log("TCP Listener stopped.");
    }

    public static bool IsSocketAlive(Socket sock)
    {
        return !(sock.Poll(1, SelectMode.SelectRead) && sock.Available == 0);
    }
}