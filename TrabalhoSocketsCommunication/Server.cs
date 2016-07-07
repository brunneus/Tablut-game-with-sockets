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
    public class Server
    {
        private GameBoard _gameBoard;
        private TcpListener _tcpListener;
        private eTeam _teamPlaying = eTeam.Black;
        private IEnumerable<eTeam> _availableTeams = new eTeam[] { eTeam.Black, eTeam.White };

        private Server()
        {
            _gameBoard = new GameBoard();
            _gameBoard.Initialize();
        }

        private static Server _instance;
        public static Server Instance
        {
            get
            {
                return _instance ?? (_instance = new Server());
            }
        }

        public void Intialize()
        {
            _tcpListener = new TcpListener(new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes()), 1025);
            _tcpListener.Start();
            StartAccept();
        }

        private void AcceptedClient(IAsyncResult ar)
        {
            StartAccept();
            var client = _tcpListener.EndAcceptTcpClient(ar);
            RecieveRequests(client);
        }

        private void StartAccept()
        {
            _tcpListener.BeginAcceptTcpClient(AcceptedClient, _tcpListener);
        }

        private void RecieveRequests(TcpClient client)
        {
            var streamClient = client.GetStream();
            var readerClient = new BinaryReader(streamClient);
            var writterClient = new BinaryWriter(streamClient);

            _connectedClientsStreamWritter.Add(writterClient);

            Request lastRequest;

            do
            {
                var buffer = new byte[client.ReceiveBufferSize];
                readerClient.Read(buffer, 0, client.ReceiveBufferSize);

                lastRequest = SerializationHelper.ByteArrayToObject<Request>(buffer);
                lastRequest.ReplyServerValue = HandleRequest(lastRequest);
                writterClient.Write(SerializationHelper.ObjectToByteArray(lastRequest));

                if (lastRequest.Type == eRequestType.MoveElement)
                {
                    _teamPlaying = _teamPlaying == eTeam.Black ?
                        eTeam.White : 
                        eTeam.Black;
                }

            } while (lastRequest.Type != eRequestType.CloseSocket);

            client.Close();
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
            else if (request.Type == eRequestType.GetGameStatus)
            {
                return HandleGetGameStatusRequest(request);
            }
            else if (request.Type == eRequestType.CanMoveElement)
            {
                return HandleCanMoveRequest(request);
            }
            else if (request.Type == eRequestType.GetUpdatedGameBoard)
            {
                return _gameBoard;
            }
            else if (request.Type == eRequestType.ResetGameBoard)
            {
                _gameBoard.Reset();
            }
            else if (request.Type == eRequestType.GiveTeam)
            {
                if (!_availableTeams.Any())
                    throw new NotSupportedException("Theres no more teams available");

                var pickedTeam = _availableTeams.First();
                _availableTeams = _availableTeams.Skip(1);
                return pickedTeam;
            }
            else if (request.Type == eRequestType.GetTeamPlaying)
            {
                return _teamPlaying;
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

            return _gameBoard;
        }

        private object HandleGetGameStatusRequest(Request request)
        {
            return _gameBoard.GetGameStatus();
        }
    }
}
