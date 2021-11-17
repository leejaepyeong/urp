using System;
namespace TestServer
{
    class GameLogic
    {
        public static void Update()
        {
            foreach(Client _client in GameServer.clients.Values)
            {
                if(_client.player != null)
                {
                    _client.player.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
