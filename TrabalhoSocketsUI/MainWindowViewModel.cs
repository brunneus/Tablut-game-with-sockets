using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabalhoSocketsCommunication;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsUI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private eTeam _team;
        private BackgroundWorker _worker = new BackgroundWorker();

        public MainWindowViewModel()
        {
            //Server.Instance.Intialize();
            
            _client = new Client();
            _client.InitializeConnection();

            Team = _client.GiveTeam();
            IsMyTimeToPlay = Team == _client.GetTeamPlaying();

            Elements = new ObservableCollection<GameBoardElementWrapper>();
            WhiteElementsCaptured = new ObservableCollection<GameBoardElementWrapper>();
            BlackElementsCaptured = new ObservableCollection<GameBoardElementWrapper>();

            _worker = new BackgroundWorker();
            //_worker.DoWork += _worker_DoWork;
            //_worker.RunWorkerAsync();

            NewGame();
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                IsMyTimeToPlay = Team == _client.GetTeamPlaying();
                ReloadLoadElements();
            }
        }

        public ObservableCollection<GameBoardElementWrapper> Elements { get; set; }
        public ObservableCollection<GameBoardElementWrapper> WhiteElementsCaptured { get; set; }
        public ObservableCollection<GameBoardElementWrapper> BlackElementsCaptured { get; set; }

        private RelayCommand<GameBoardElementWrapper> _selectElementToMoveCommand;
        public RelayCommand<GameBoardElementWrapper> SelectElementToMoveCommand
        {
            get
            {
                if (_selectElementToMoveCommand == null)
                    _selectElementToMoveCommand = new RelayCommand<GameBoardElementWrapper>((param) => MoveElement(param));

                return _selectElementToMoveCommand;
            }
        }
        
        private RelayCommand _newGameCommand;
        public RelayCommand NewGameCommand
        {
            get
            {
                if (_newGameCommand == null)
                    _newGameCommand = new RelayCommand(NewGame);

                return _newGameCommand;
            }
        }

        private RelayCommand _closeConnectionCommand;
        public RelayCommand CloseConnectionCommand
        {
            get
            {
                if (_closeConnectionCommand == null)
                    _closeConnectionCommand = new RelayCommand(() => Client.CloseConnection());

                return _closeConnectionCommand;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private string _gameStatusMessage;
        public string GameStatusMessage
        {
            get
            {
                return _gameStatusMessage;
            }

            set
            {
                _gameStatusMessage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("GameStatusMessage"));
            }
        }

        private bool _gameEnded;
        public bool GameEnded
        {
            get
            {
                return _gameEnded;
            }

            set
            {
                _gameEnded = value;
                PropertyChanged(this, new PropertyChangedEventArgs("GameEnded"));
            }
        }

        private bool _isMyTimeToPlay;
        public bool IsMyTimeToPlay
        {
            get
            {
                return _isMyTimeToPlay;
            }
            set
            {
                _isMyTimeToPlay = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsMyTimeToPlay)));
            }
        }

        private Client _client;
        public Client Client
        {
            get
            {
                return _client;
            }

            set
            {
                _client = value;
            }
        }

        public eTeam Team
        {
            get
            {
                return _team;
            }

            set
            {
                _team = value;
            }
        }

        private void MoveElement(GameBoardElementWrapper clickedWrapper)
        {
            var selectedWrapper = this.Elements.FirstOrDefault(e => e.IsSelected);

            if (clickedWrapper.Element != null && clickedWrapper.Element.Team != Team)
            {
                if (selectedWrapper != null)
                    selectedWrapper.IsSelected = false;

                return;
            }

            if (clickedWrapper.Element == null && selectedWrapper == null)
                return;

            if (selectedWrapper == null)
            {
                clickedWrapper.IsSelected = true;
            }
            else
            {
                if (Client.SendCanMoveToRequest(selectedWrapper.Element, clickedWrapper.R, clickedWrapper.C))
                {
                    Client.SendRequestToServerToMoveElement(selectedWrapper.Element, clickedWrapper.R, clickedWrapper.C);

                    var lastMovedElement = Client.GetUpdatedGameBoard().ElementAt(clickedWrapper.R, clickedWrapper.C);
                    UpdateListOfCapturedElements(lastMovedElement);
                    Client.SendRemoveCapturedElementsAfterLastMovementRequest(lastMovedElement);

                    ReloadLoadElements();
                    UpdateGameStatusMessage();
                }
                else
                {
                    selectedWrapper.IsSelected = false;
                    selectedWrapper = null;
                }
            }
        }

        private void NewGame()
        {
            //_client.SendResetRequest();

            Elements.Clear();
            WhiteElementsCaptured.Clear();
            BlackElementsCaptured.Clear();
            GameEnded = false;
            GameStatusMessage = string.Empty;

            ReloadLoadElements();
        }

        private void WaitOtherPlayerPlays()
        {
            var updatedGameBoard = _client.GetUpdatedGameBoard();
            IsMyTimeToPlay = Team == _client.GetTeamPlaying();
        }

        private void UpdateListOfCapturedElements(IGameBoardElement lastMovedElement)
        {
            if (lastMovedElement == null)
                return;

            var blackTeamElementsCount = this.Elements.Where(e => e.Element != null && e.Element.Team == eTeam.Black);
            var whiteTeamElementsCount = this.Elements.Where(e => e.Element != null && e.Element.Team == eTeam.White);

            var elementsCaptured = Client.SendGetCapturedElementsAfterLastMovementRequest(lastMovedElement);
            var team = lastMovedElement.Team == eTeam.Black ? eTeam.White : eTeam.Black;

            foreach (var element in elementsCaptured)
            {
                var wrapper = this.Elements.First(e => e.R == element.R && e.C == element.C);

                if (team == eTeam.Black)
                    this.BlackElementsCaptured.Add(wrapper);
                else
                    this.WhiteElementsCaptured.Add(wrapper);
            }
        }

        private void ReloadLoadElements()
        {
            Elements.Clear();

            var gameBoard = Client.GetUpdatedGameBoard();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Elements.AddOnUI(new GameBoardElementWrapper(gameBoard.Board[i, j]) { C = j, R = i });
                }
            }
        }

        private void UpdateGameStatusMessage()
        {
            var gameStatus = Client.SendGameStatusMessageRequest();

            GameEnded = gameStatus != eGameStatus.Running;

            if (gameStatus == eGameStatus.BlackTeamWithoutValidMovements)
            {
                this.GameStatusMessage = "Time ganhador: Branco (Peças pretas não possuem movimentos válidos)!";
            }
            else if (gameStatus == eGameStatus.WhiteTeamWithoutValidMovements)
            {
                this.GameStatusMessage = "Time ganhador: Preto (Peças brancas não possuem movimentos válidos)!";
            }
            else if (gameStatus == eGameStatus.KingSorroundByMercenaries)
            {
                this.GameStatusMessage = "Time ganhador: Preto (Rei esta cercado de mercenários)!";
            }
            else if (gameStatus == eGameStatus.KingArriveAtSomeSide)
            {
                this.GameStatusMessage = "Time ganhador: Branco (Rei chegou em um dos 4 lados)!";
            }
        }
    }
}
