using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TestServer
{
    class GameServer
    {
        // 최대 접속 수
        public static int MaxPlayers { get; private set; }

        // 포트 넘버
        public static int Port { get; private set; }

        // 클라이언트 사전식으로 정렬
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        // 대리자 패킷핸들러 (패킷정보 토대로 해당 넘버 클라이언트에 전달?)
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        // 패킷핸들러 사전식 정렬
        public static Dictionary<int, PacketHandler> packetHandlers;

        // tcp 수신
        private static TcpListener tcpListener;
        // udp 수
        private static UdpClient udpListner;

        /// <summary>
        /// 최대 접속자 수 및 포트번호 지정, tcp udp설
        /// </summary>
        /// <param name="_maxPlayers"></param>
        /// <param name="_port"></param>
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server...");
            InitializeSeverData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback),null);


            udpListner = new UdpClient(Port);
            udpListner.BeginReceive(UDPReceiveCallback, null);
        }


        /// <summary>
        /// tcp통한 클라이언트 연결 최대 접속자수까지 빈공간에 클라이언트 연결
        /// </summary>
        /// <param name="_result"></param>
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback),null);

            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server Full");
        }


        /// <summary>
        /// udp 통한 패킷 데이터 수집
        /// </summary>
        /// <param name="_result"></param>
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListner.EndReceive(_result, ref _clientEndPoint);
                udpListner.BeginReceive(UDPReceiveCallback, null);

                if(_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    // 0이라는 id의 클라이언 인스턴스 검색 시도로 서버 충돌 발생할 수 있기떄문에 return
                    if(_clientId == 0)
                    {
                        return;
                    }

                    if(clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if(clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }

                }
            }
            catch(Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
            }
        }



        /// <summary>
        /// udp 데이터 보내기
        /// </summary>
        /// <param name="_clientEndPoint"></param>
        /// <param name="_packet"></param>
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if(_clientEndPoint != null)
                {
                    udpListner.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void InitializeSeverData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i,new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.udpTestReceived, ServerHandle.UDPTestReceived },
                {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement }
            };
            Console.WriteLine("Initialized packets");
        }

    }
        
}
