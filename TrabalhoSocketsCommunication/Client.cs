using System.IO;
using System.Net;
using System.Net.Sockets;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsCommunication
{
    public class Client
    {
        TcpClient _client;

        public void InitializeConnection()
        {
            _client = new TcpClient();
            _client.Connect(new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes()), 1025);
        }

        public void MoveElement(IGameBoardElement element, int r, int c)
        {
            var request = new Request()
            {
                Type = eRequestType.MoveElement,
                ClientParameterValue = new MoveElementTransferObject() { GameBoardElement = element, TargetR = r, TargetC = c }
            };

            SendRequestToServer<object>(request);
        }

        public eTeam SelectTeam()
        {
            var request = new Request()
            {
                Type = eRequestType.PickTeam,
            };

            return SendRequestToServer<eTeam>(request);
        }

        public eTeam GetTeamPlaying()
        {
            var request = new Request()
            {
                Type = eRequestType.GetTeamPlaying,
            };

            return SendRequestToServer<eTeam>(request);
        }

        public GameBoard GetUpdatedGameBoard()
        {
            var request = new Request()
            {
                Type = eRequestType.GetUpdatedGameBoard,
            };

            var reply = SendRequestToServer<GameBoard>(request);

            return reply;
        }

        public eGameStatus GetGameStatus()
        {
            var request = new Request()
            {
                Type = eRequestType.GetGameStatus
            };

            return SendRequestToServer<eGameStatus>(request);
        }

        public bool CanMoveTo(IGameBoardElement element, int r, int c)
        {
            var request = new Request()
            {
                Type = eRequestType.CanMoveElement,
                ClientParameterValue = new MoveElementTransferObject() { GameBoardElement = element, TargetR = r, TargetC = c }
            };

            return SendRequestToServer<bool>(request);
        }

        public void CloseConnection()
        {
            var request = new Request()
            {
                Type = eRequestType.CloseSocket,
            };

            var writter = new BinaryWriter(_client.GetStream());
            writter.Write(SerializationHelper.ObjectToByteArray(request));
        }

        private T SendRequestToServer<T>(Request request)
        {
            var buffer = new byte[_client.ReceiveBufferSize];

            var reader = new BinaryReader(_client.GetStream());
            var writter = new BinaryWriter(_client.GetStream());

            writter.Write(SerializationHelper.ObjectToByteArray(request));
            reader.Read(buffer, 0, _client.ReceiveBufferSize);

            return (T)SerializationHelper.ByteArrayToObject<Request>(buffer).ReplyServerValue;
        }
    }
}
