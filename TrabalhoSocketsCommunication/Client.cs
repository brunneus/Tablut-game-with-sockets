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

        public GameBoard GetUpdatedGameBoard()
        {
            var request = new Request()
            {
                Type = eRequestType.GetUpdatedGameBoard,
            };

            var reply = SendRequestToServer(request);

            return (GameBoard)reply.ReplyServerValue;
        }

        public IEnumerable<IGameBoardElement> SendGetCapturedElementsAfterLastMovementRequest(IGameBoardElement lastMovedElement)
        {
            var request = new Request()
            {
                Type = eRequestType.GetCapturedElementsAfterLastMovement,
                ClientParameterValue = lastMovedElement
            };

            var reply = SendRequestToServer(request);
            return (IEnumerable<IGameBoardElement>)reply.ReplyServerValue;
        }

        private Request SendRequestToServer(Request request)
        {
            var stream = _client.GetStream();
            var reader = new BinaryReader(stream);
            var writter = new BinaryWriter(stream);

            writter.Write(SerializationHelper.ObjectToByteArray(request));

            var buffer = new byte[_client.ReceiveBufferSize];
            reader.Read(buffer, 0, _client.ReceiveBufferSize);

            return SerializationHelper.ByteArrayToObject<Request>(buffer);
        }

        public eGameStatus SendGameStatusMessageRequest()
        {
            var request = new Request()
            {
                Type = eRequestType.GetGameStatus
            };

            var reply = this.SendRequestToServer(request);

            return (eGameStatus)Enum.Parse(typeof(eGameStatus), reply.ReplyServerValue.ToString());
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

            return (bool)SendRequestToServer(request).ReplyServerValue;
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
