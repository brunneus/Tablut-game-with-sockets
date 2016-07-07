using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsCommunication
{
    public class Client
    {
        TcpClient _client = new TcpClient();

        public void InitializeConnection()
        {
            _client.Connect(new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes()), 1025);
        }

        public void SendRequestToServerToMoveElement(IGameBoardElement element, int r, int c)
        {
            var request = new Request()
            {
                Type = eRequestType.MoveElement,
                ClientParameterValue = new MoveElementTransferObject() { GameBoardElement = element, TargetR = r, TargetC = c }
            };

            SendRequestToServer(request);
        }

        public eTeam GiveTeam()
        {
            var request = new Request()
            {
                Type = eRequestType.GiveTeam,
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

        public IEnumerable<IGameBoardElement> SendGetCapturedElementsAfterLastMovementRequest(IGameBoardElement lastMovedElement)
        {
            var request = new Request()
            {
                Type = eRequestType.GetCapturedElementsAfterLastMovement,
                ClientParameterValue = lastMovedElement
            };

            var reply = SendRequestToServer<IEnumerable<IGameBoardElement>>(request);
            return reply;
        }

        private void SendRequestToServer(Request request)
        {
            var reader = new BinaryReader(_client.GetStream());
            var buffer = new byte[_client.ReceiveBufferSize];
            var writter = new BinaryWriter(_client.GetStream());

            writter.Write(SerializationHelper.ObjectToByteArray(request));
            reader.Read(buffer, 0, _client.ReceiveBufferSize);
        }

        private T SendRequestToServer<T>(Request request)
        {
            var reader = new BinaryReader(_client.GetStream());
            var buffer = new byte[_client.ReceiveBufferSize];
            var writter = new BinaryWriter(_client.GetStream());

            writter.Write(SerializationHelper.ObjectToByteArray(request));
            reader.Read(buffer, 0, _client.ReceiveBufferSize);

            return (T)SerializationHelper.ByteArrayToObject<Request>(buffer).ReplyServerValue;
        }

        public eGameStatus SendGameStatusMessageRequest()
        {
            var request = new Request()
            {
                Type = eRequestType.GetGameStatus
            };

            return SendRequestToServer<eGameStatus>(request);
        }

        public void SendRemoveCapturedElementsAfterLastMovementRequest(IGameBoardElement lastMovedElement)
        {
            var request = new Request()
            {
                Type = eRequestType.RemoveCapturedElementsAfterLastMovement,
                ClientParameterValue = lastMovedElement
            };

            SendRequestToServer(request);
        }

        public bool SendCanMoveToRequest(IGameBoardElement element, int r, int c)
        {
            var request = new Request()
            {
                Type = eRequestType.CanMoveElement,
                ClientParameterValue = new MoveElementTransferObject() { GameBoardElement = element, TargetR = r, TargetC = c }
            };

            return SendRequestToServer<bool>(request);
        }

        public void SendResetRequest()
        {
            var request = new Request()
            {
                Type = eRequestType.ResetGameBoard,
            };

            SendRequestToServer(request);
        }

        public void CloseConnection()
        {
            var request = new Request()
            {
                Type = eRequestType.CloseSocket,
            };

            this.SendRequestToServer(request);
            _client.Close();
        }
    }
}
