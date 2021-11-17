using System;
using System.Collections.Generic;
using System.Text;

namespace TestServer
{
    class ServerSend
    {

        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            GameServer.clients[_toClient].tcp.SendData(_packet);    
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            GameServer.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>
        /// 모두에게 데이터 보내기
        /// </summary>
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= GameServer.MaxPlayers; i++)
            {
                GameServer.clients[i].tcp.SendData(_packet);
            }
        }

        /// <summary>
        /// 특정 클라이언트 제외하고 데이터 보내기
        /// </summary>
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= GameServer.MaxPlayers; i++)
            {
                if(i != _exceptClient)
                {
                    GameServer.clients[i].tcp.SendData(_packet);
                }
            }
        }

        /// <summary>
        /// 모두에게 데이터 보내기
        /// </summary>
        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= GameServer.MaxPlayers; i++)
            {
                GameServer.clients[i].udp.SendData(_packet);
            }
        }

        /// <summary>
        /// 특정 클라이언트 제외하고 데이터 보내기
        /// </summary>
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= GameServer.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    GameServer.clients[i].udp.SendData(_packet);
                }
            }
        }



        #region Packets
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void UDPTest(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.udpTest))
            {
                _packet.Write("A test packet for UDP");

                SendUDPData(_toClient, _packet);
            }
        }

        public static void SpawnPlayer(int _toClient, Player _player)
        {
            // Player정보는 중요하므로 한번 보낼때 안전한 TCP로 통신
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_toClient , _packet);

            }
        }

        /// <summary>
        /// 플레이어 포지션을 전체에게 송신
        /// </summary>
        /// <param name="_player"></param>
        public static void PlayerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);

                SendUDPDataToAll(_packet);
            }
        }

        /// <summary>
        /// 플레이어 회전값을 전체에게 송신
        /// </summary>
        /// <param name="_player"></param>
        public static void PlayerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                SendUDPDataToAll(_player.id, _packet);
            }
        }

        #endregion


    }
}
