using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsCommunication
{
    public class Server
    {
        private GameBoard _gameBoard;
        private TcpListener _tcpListener;
        private eTeam _teamPlaying = eTeam.Black;
        private IEnumerable<eTeam> _availableTeams = new eTeam[] { eTeam.Black, eTeam.White };

        private static Server _instance;
        public static Server Instance
        {
            get
            {
                return _instance ?? (_instance = new Server());
            }
        }

        private Server()
        {
            _gameBoard = new GameBoard();
            _gameBoard.Initialize();
        }

        public void Intialize()
        {
            _tcpListener = new TcpListener(new IPAddress(IPAddress.Parse("127.0.0.1").GetAddressBytes()), 1025);
            _tcpListener.Start();
            StartAccept();
        }

        private void StartAccept()
        {
            _tcpListener.BeginAcceptTcpClient(AcceptedClient, _tcpListener);
        }

        private void AcceptedClient(IAsyncResult ar)
        {
            StartAccept();
            var client = _tcpListener.EndAcceptTcpClient(ar);
            HandleRequests(client);
        }

        private void HandleRequests(TcpClient client)
        {
            var streamClient = client.GetStream();
            var readerClient = new BinaryReader(streamClient);
            var writterClient = new BinaryWriter(streamClient);

            while (true)
            {
                var buffer = new byte[client.ReceiveBufferSize];
                readerClient.Read(buffer, 0, client.ReceiveBufferSize);

                var request = SerializationHelper.ByteArrayToObject<Request>(buffer);
                
                request.ReplyServerValue = GetReplyToRequest(request);
                writterClient.Write(SerializationHelper.ObjectToByteArray(request));
                
                if (request.Type == eRequestType.CloseSocket)
                    break;

                if (request.Type == eRequestType.MoveElement)
                    SwitchTeamPlaying();
            }
            
            client.GetStream().Close();
            client.Close();
        }

        private void SwitchTeamPlaying()
        {
            _teamPlaying = _teamPlaying == eTeam.Black ?
                                    eTeam.White :
                                    eTeam.Black;
        }

        private object GetReplyToRequest(Request request)
        {
            if (request.Type == eRequestType.MoveElement)
                return HandleMoveElementRequest(request);

            else if (request.Type == eRequestType.GetGameStatus)
                return _gameBoard.GetGameStatus();

            else if (request.Type == eRequestType.CanMoveElement)
                return HandleCanMoveRequest(request);

            else if (request.Type == eRequestType.GetUpdatedGameBoard)
                return _gameBoard;

            else if (request.Type == eRequestType.ResetGameBoard)
                _gameBoard.Reset();

            else if (request.Type == eRequestType.PickTeam)
                return HandlePickTeamRequest();
            
            else if (request.Type == eRequestType.GetTeamPlaying)
                return _teamPlaying;

            else if(request.Type == eRequestType.CloseSocket)
                return "Connection Closed";

            throw new NotSupportedException("Cannot handle the request...");
        }

        private object HandlePickTeamRequest()
        {
            if (!_availableTeams.Any())
                throw new NotSupportedException("Theres no more teams available");

            var pickedTeam = _availableTeams.First();
            _availableTeams = _availableTeams.Skip(1);
            return pickedTeam;
        }

        private object HandleCanMoveRequest(Request request)
        {
            var moveElementTransferObject = (MoveElementTransferObject)request.ClientParameterValue;

            return _gameBoard.CanMoveTo(
                moveElementTransferObject.GameBoardElement, 
                moveElementTransferObject.TargetR, 
                moveElementTransferObject.TargetC);
        }
        
        private object HandleMoveElementRequest(Request request)
        {
            var moveElementTransferObject = (MoveElementTransferObject)request.ClientParameterValue;

            _gameBoard.MoveTo(
                moveElementTransferObject.GameBoardElement, 
                moveElementTransferObject.TargetR, 
                moveElementTransferObject.TargetC);

            return _gameBoard;
        }
    }
}
