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
        public MainWindowViewModel()
        {
            Server.Singleton.Intialize();

            _client = new Client();
            _client.InitializeConnection();

            Elements = new ObservableCollection<GameBoardElementWrapper>();
            WhiteElementsCaptured = new ObservableCollection<GameBoardElementWrapper>();
            BlackElementsCaptured = new ObservableCollection<GameBoardElementWrapper>();

            this.NewGame(null);
        }

        public ObservableCollection<GameBoardElementWrapper> Elements { get; set; }
        public ObservableCollection<GameBoardElementWrapper> WhiteElementsCaptured { get; set; }
        public ObservableCollection<GameBoardElementWrapper> BlackElementsCaptured { get; set; }

        private RelayCommand _selectElementToMoveCommand;
        public RelayCommand SelectElementToMoveCommand
        {
            get
            {
                if (_selectElementToMoveCommand == null)
                    _selectElementToMoveCommand = new RelayCommand((param) =>
                    {
                        var clickedWrapper = param as GameBoardElementWrapper;

                        if (_selectedWrapper == null && clickedWrapper.Element == null)
                            return;

                        if (clickedWrapper.Element != null && clickedWrapper.Element.Team != CurrentTeamPlay)
                        {
                            this.SelectedWrapper = null;
                            return;
                        }

                        if (SelectedWrapper != null)
                        {
                            if (Client.SendCanMoveToRequest(_selectedWrapper.Element, clickedWrapper.R, clickedWrapper.C))
                            {
                                Client.SendRequestToServerToMoveElement(SelectedWrapper.Element, clickedWrapper.R, clickedWrapper.C);

                                var lastMovedElement = Client.GetUpdatedGameBoard().ElementAt(clickedWrapper.R, clickedWrapper.C);
                                UpdateListOfCapturedElements(lastMovedElement);

                                Client.SendRemoveCapturedElementsAfterLastMovementRequest(lastMovedElement);
                                SelectedWrapper = null;

                                CurrentTeamPlay = CurrentTeamPlay == eTeam.Black ? eTeam.White : eTeam.Black;

                                ReloadLoadElements();
                                UpdateGameStatusMessage();
                            }
                        }
                        else
                        {
                            SelectedWrapper = clickedWrapper;
                            SelectedWrapper.IsSelected = true;
                        }
                    });

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
                    _closeConnectionCommand = new RelayCommand((param) => Client.CloseConnection());

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
        private GameBoardElementWrapper _selectedWrapper;
        public GameBoardElementWrapper SelectedWrapper
        {
            get
            {
                return _selectedWrapper;
            }

            set
            {
                _selectedWrapper = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedWrapper"));
            }
        }

        private eTeam _currentTeamPlay = eTeam.Black;
        public eTeam CurrentTeamPlay
        {
            get
            {
                return _currentTeamPlay;
            }
            private set
            {
                _currentTeamPlay = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentTeamPlay"));
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
        
        private void NewGame(object obj)
        {
            _client.SendResetRequest();

            Elements.Clear();
            WhiteElementsCaptured.Clear();
            BlackElementsCaptured.Clear();
            GameEnded = false;
            GameStatusMessage = string.Empty;
            SelectedWrapper = null;

            ReloadLoadElements();
        }

        private void UpdateListOfCapturedElements(IGameBoardElement lastMovedElement)
        {
            if (lastMovedElement == null)
                return;

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
            this.Elements.Clear();

            var gameBoard = Client.GetUpdatedGameBoard();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Elements.Add(new GameBoardElementWrapper(gameBoard.Board[i, j]) { C = j, R = i });
                }
            }
        }

        private void UpdateGameStatusMessage()
        {
            var gameStatus = Client.SendGameStatusMessageRequest();
            this.GameEnded = gameStatus != eGameStatus.Running;

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
