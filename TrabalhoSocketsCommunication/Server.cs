using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsCommunication
{
    public  class Server
    {
        private GameBoard _gameBoard;
        private TcpListener _tcpListener;

        private Server()
        {
            _gameBoard = new GameBoard();
            _gameBoard.Initialize();
        }

        private static Server _singleton;
        public static Server Singleton
        {
            get
            {
                return _singleton ?? (_singleton = new Server());
            }
        }

        public void Intialize()
        {
            new Thread(() =>
            {
                _tcpListener = new TcpListener(new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes()), 1025);
                _tcpListener.Start();

                var clientSocket = _tcpListener.AcceptTcpClient();
                var buffer = new byte[clientSocket.ReceiveBufferSize];
                var stream = clientSocket.GetStream();
                var reader = new BinaryReader(stream);
                var writter = new BinaryWriter(stream);
                Request lastRequest;

                do
                {
                    reader.Read(buffer, 0, clientSocket.ReceiveBufferSize);

                    lastRequest = SerializationHelper.ByteArrayToObject<Request>(buffer);
                    lastRequest.ReplyServerValue = this.HandleRequest(lastRequest);

                    writter.Write(SerializationHelper.ObjectToByteArray(lastRequest));

                } while (lastRequest.Type != eRequestType.CloseSocket);

                clientSocket.Close();

            }).Start();
        }

        private object HandleRequest(Request request)
        {
            if (request.Type == eRequestType.MoveElement)
            {
                return HandleMoveElementRequest(request);
            }
            else if (request.Type == eRequestType.GetCapturedElementsAfterLastMovement)
            {
                return HandleGetCapturedElementsAfterLastMovementRequest(request);
            }
            else if (request.Type == eRequestType.RemoveCapturedElementsAfterLastMovement)
            {
                return HandleRemoveCapturedElementsAfterLastMovementRequest(request);
            }
            else if(request.Type == eRequestType.GetUpdatedGameBoard)
            {
                return _gameBoard;
            }
            else if(request.Type == eRequestType.GetGameStatus)
            {
                return this.HandleGetGameStatusRequest(request);
            }
            else if(request.Type == eRequestType.CanMoveElement)
            {
                return HandleCanMoveRequest(request);
            }
            else if(request.Type == eRequestType.ResetGameBoard)
            {
                _gameBoard.Reset();
            }

            return null;
        }

        private object HandleCanMoveRequest(Request request)
        {
            var moveElementTransferObject = (MoveElementTransferObject)request.ClientParameterValue;
            return _gameBoard.CanMoveTo(moveElementTransferObject.GameBoardElement, moveElementTransferObject.TargetR, moveElementTransferObject.TargetC);
        }

        private object HandleRemoveCapturedElementsAfterLastMovementRequest(Request request)
        {
            var lastMovedElement = (IGameBoardElement)request.ClientParameterValue;
            _gameBoard.RemoveCapturedElementsAfterLastMovement(lastMovedElement);
            return null;
        }

        private object HandleGetCapturedElementsAfterLastMovementRequest(Request request)
        {
            var lastMovedElement = (IGameBoardElement)request.ClientParameterValue;
            return _gameBoard.GetElementsToBeCapturedAfterLastMovement(lastMovedElement);
        }

        private object HandleMoveElementRequest(Request request)
        {
            var moveElementTransferObject = (MoveElementTransferObject)request.ClientParameterValue;
            _gameBoard.MoveTo(moveElementTransferObject.GameBoardElement, moveElementTransferObject.TargetR, moveElementTransferObject.TargetC);

            return null;
        }

        private object HandleGetGameStatusRequest(Request request)
        {
            return _gameBoard.GetGameStatus();
        }
    }
}
