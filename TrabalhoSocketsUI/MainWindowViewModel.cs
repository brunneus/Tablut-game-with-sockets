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
        BackgroundWorker _worker = new BackgroundWorker();

        public MainWindowViewModel()
        {
            _client = new Client();
            _client.InitializeConnection();
            _worker.WorkerSupportsCancellation = true;

            Elements = new GameBoardElementWrapper[81];
            WhiteElementsCaptured = new ObservableCollection<GameBoardElementWrapper>();
            BlackElementsCaptured = new ObservableCollection<GameBoardElementWrapper>();

            NewGame();

            _team = _client.SelectTeam();
            _isMyTimeToPlay = Team == _client.GetTeamPlaying();

            WaitOtherPlayerPlays();
        }

        public GameBoardElementWrapper[] Elements { get; set; }
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
                    _closeConnectionCommand = new RelayCommand(() =>
                    {
                        _worker.CancelAsync();
                        _worker.Dispose();
                        Client.CloseConnection();
                    });

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

                if (value)
                {
                    ReloadLoadElements();
                    UpdateGameStatusMessage();
                    UpdateListOfCapturedElements();
                }

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
                if (Client.CanMoveTo(selectedWrapper.Element, clickedWrapper.R, clickedWrapper.C))
                {
                    Client.MoveElement(selectedWrapper.Element, clickedWrapper.R, clickedWrapper.C);
                    ReloadLoadElements();
                    UpdateGameStatusMessage();
                    UpdateListOfCapturedElements();
                    IsMyTimeToPlay = false;
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
            var index = 0;
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    Elements[index++] = new GameBoardElementWrapper(null) { C = j, R = i };

            WhiteElementsCaptured.Clear();
            BlackElementsCaptured.Clear();
            GameEnded = false;
            GameStatusMessage = string.Empty;

            ReloadLoadElements();
        }

        private void WaitOtherPlayerPlays()
        {
            _worker.DoWork += (s, e) =>
            {
                while (!_worker.CancellationPending)
                {
                    while (!IsMyTimeToPlay)
                    {
                        if (!IsMyTimeToPlay && !_worker.CancellationPending)
                            IsMyTimeToPlay = Team == _client.GetTeamPlaying();
                    }
                }
            };

            _worker.RunWorkerAsync();
        }

        private void UpdateListOfCapturedElements()
        {
            var elementsCaptured = Client.GetUpdatedGameBoard().CapturedElements;

            BlackElementsCaptured.ClearOnUI();
            WhiteElementsCaptured.ClearOnUI();

            foreach (var element in elementsCaptured)
            {
                if (element.Team == eTeam.Black)
                    BlackElementsCaptured.AddOnUI(new GameBoardElementWrapper(element));
                else
                    WhiteElementsCaptured.AddOnUI(new GameBoardElementWrapper(element));
            }
        }

        private void ReloadLoadElements()
        {
            var gameBoard = Client.GetUpdatedGameBoard();

            var index = 0;

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    Elements[index].IsSelected = false;
                    Elements[index++].Element = gameBoard.Board[i, j];
                }
        }

        private void UpdateGameStatusMessage()
        {
            var gameStatus = Client.GetGameStatus();

            GameEnded = gameStatus != eGameStatus.Running;

            if (gameStatus == eGameStatus.BlackTeamWithoutValidMovements)
            {
                GameStatusMessage = "Time ganhador: Branco (Peças pretas não possuem movimentos válidos)!";
            }
            else if (gameStatus == eGameStatus.WhiteTeamWithoutValidMovements)
            {
                GameStatusMessage = "Time ganhador: Preto (Peças brancas não possuem movimentos válidos)!";
            }
            else if (gameStatus == eGameStatus.KingSorroundByMercenaries)
            {
                GameStatusMessage = "Time ganhador: Preto (Rei esta cercado de mercenários)!";
            }
            else if (gameStatus == eGameStatus.KingArriveAtSomeSide)
            {
                GameStatusMessage = "Time ganhador: Branco (Rei chegou em um dos 4 lados)!";
            }
        }
    }
}
