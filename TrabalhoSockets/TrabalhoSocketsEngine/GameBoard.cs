using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsEngine
{
    [Serializable]
    public class GameBoard
    {
        public GameBoard()
        {
            
        }

        private IGameBoardElement[,] _board;
        public IGameBoardElement[,] Board
        {
            get
            {
                return _board;
            }
        }

        public void Print()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Board[i, j] is King)
                        Console.Write("K\t");
                    else if (Board[i, j] is Mercenary)
                        Console.Write("M\t");
                    else if (Board[i, j] is Bodyguard)
                        Console.Write("G\t");
                    else
                        Console.Write("X\t");
                }

                Console.WriteLine("\n");
            }
        }

        public void MoveFromTo(int sourceR, int sourceC, int targetR, int targetC)
        {
            var element = _board[sourceR, sourceC];
            this.MoveTo(element, targetR, targetC);
        }

        public void MoveTo(IGameBoardElement elementToMove, int r, int c)
        {
            if (CanMoveTo(elementToMove, r, c))
            {
                _board[elementToMove.R, elementToMove.C] = null;
                elementToMove.C = c;
                elementToMove.R = r;
                _board[r, c] = elementToMove;
            }
        }

        public bool CanMoveTo(IGameBoardElement elementToMove, int r, int c)
        {
            if (elementToMove == null)
                return false;

            if (r == 4 && c == 4)
                return false;

            if (r < 0 || r > 8 || c < 0 || c > 8)
                return false;

            if (elementToMove.R != r && elementToMove.C != c)
                return false;

            if (elementToMove.R != r)
            {
                var steps = Math.Abs(elementToMove.R - r);
                var sign = Math.Sign(r - elementToMove.R);
                var currentRow = elementToMove.R;

                while (steps != 0)
                {
                    var elementOfNextPosition = Board[currentRow += 1 * sign, elementToMove.C];

                    if (elementOfNextPosition != null)
                        return false;

                    steps--;
                }
            }
            else
            {
                var steps = Math.Abs(elementToMove.C - c);
                var sign = Math.Sign(c - elementToMove.C);
                var currentColumn = elementToMove.C;

                while (steps != 0)
                {
                    var elementOfNextPosition = Board[elementToMove.R, currentColumn += 1 * sign];

                    if (elementOfNextPosition != null)
                        return false;

                    steps--;
                }
            }

            return true;
        }

        public void RemoveRangeOfElements(IEnumerable<IGameBoardElement> elements)
        {
            foreach (var element in elements)
                this.RemoveElement(element);
        }

        public void RemoveElement(IGameBoardElement element)
        {
            _board[element.R, element.C] = null;
        }

        public void RemoveElement(int r, int c)
        {
            _board[r, c] = null;
        }

        public bool KingIsSorroundByMercenaries()
        {
            var king = _board.OfType<King>().First();
            var mercenariesAround = this.GetElementsAroundOf(king).OfType<Mercenary>();

            if (mercenariesAround.Count() <= 2)
                return false;

            if (mercenariesAround.Count() == 4)
                return true;

            var kingIsOnLeftOfThrone = king.C == 3 && king.R == 4;
            var kingIsOnRightOfThrone = king.C == 5 && king.R == 4;
            var kingIsOnBottomOfThrone = king.C == 4 && king.R == 5;
            var kingIsOnTopOfThrone = king.C == 4 && king.R == 3;

            return kingIsOnBottomOfThrone || kingIsOnLeftOfThrone || kingIsOnRightOfThrone || kingIsOnTopOfThrone;
        }

        public bool KingArriveInSomeSide()
        {
            var king = _board.OfType<King>().First();

            return king.R == 0 || king.R == 8 || king.C == 0 || king.C == 8;
        }

        public void RemoveCapturedElementsAfterLastMovement(IGameBoardElement lastMovedElement)
        {
            this.RemoveRangeOfElements(this.GetElementsToBeCapturedAfterLastMovement(lastMovedElement));
        }

        public eGameStatus GetGameStatus()
        {
            if (KingIsSorroundByMercenaries())
                return eGameStatus.KingSorroundByMercenaries;

            if (KingArriveInSomeSide())
                return eGameStatus.KingArriveAtSomeSide;

            if (!TeamHasSomeValidMovements(eTeam.Black))
                return eGameStatus.BlackTeamWithoutValidMovements;

            if (!TeamHasSomeValidMovements(eTeam.White))
                return eGameStatus.WhiteTeamWithoutValidMovements;

            return eGameStatus.Running;
        }

        public IEnumerable<IGameBoardElement> GetElementsToBeCapturedAfterLastMovement(IGameBoardElement lastMovedElement)
        {
            if (lastMovedElement == null || !lastMovedElement.CanCapture)
                return Enumerable.Empty<IGameBoardElement>();

            var capturedElements = new List<IGameBoardElement>();
            var elementsArroundFromAnotherTeam = GetElementsAroundOf(lastMovedElement)
                                                    .Where(t => t != null &&
                                                           t.Team != lastMovedElement.Team &&
                                                           t.IsCatchable);

            foreach (var elementDifferent in elementsArroundFromAnotherTeam)
            {
                if (CanBeCaptured(elementDifferent))
                {
                    capturedElements.Add(_board[elementDifferent.R, elementDifferent.C]);
                }
            }

            return capturedElements;
        }

        public bool TeamHasSomeValidMovements(eTeam team)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var element = _board[i, j];

                    if (element == null || element.Team != team)
                        continue;

                    var canMoveToRight = CanMoveTo(element, element.R, element.C + 1);
                    var canMoveToLeft = CanMoveTo(element, element.R, element.C - 1);
                    var canMoveToBottom = CanMoveTo(element, element.R + 1, element.C);
                    var canMoveToTop = CanMoveTo(element, element.R - 1, element.C);

                    if (canMoveToBottom || canMoveToLeft || canMoveToRight || canMoveToTop)
                        return true;
                }
            }

            return false;
        }

        public void Initialize()
        {
            this.LoadInitialState();
        }

        public void Reset()
        {
            LoadInitialState();
        }

        public IGameBoardElement ElementAt(int r, int c)
        {
            return _board[r, c];
        }

        private bool CanBeCaptured(IGameBoardElement element)
        {
            var elementsArroundFromAnotherTeam = GetElementsAroundOf(element)
                                                        .Where(t => t != null &&
                                                               t.Team != element.Team &&
                                                               t.CanCapture);

            if (elementsArroundFromAnotherTeam.Count() < 2) return false;

            var elementOnTop = elementsArroundFromAnotherTeam.FirstOrDefault(e => e.R == element.R + 1);
            var elementOnBottom = elementsArroundFromAnotherTeam.FirstOrDefault(e => e.R == element.R - 1);
            var elementOnLeft = elementsArroundFromAnotherTeam.FirstOrDefault(e => e.C == element.C - 1);
            var elementOnRight = elementsArroundFromAnotherTeam.FirstOrDefault(e => e.C == element.C + 1);

            return (elementOnTop != null && elementOnBottom != null) ||
                   (elementOnLeft != null && elementOnRight != null);
        }

        private IGameBoardElement[] GetElementsAroundOf(IGameBoardElement element)
        {
            var elementsAround = new IGameBoardElement[4];
            elementsAround[0] = element.C == 0 ? null : Board[element.R, element.C - 1];
            elementsAround[1] = element.R == 0 ? null : Board[element.R - 1, element.C];
            elementsAround[2] = element.C == 8 ? null : Board[element.R, element.C + 1];
            elementsAround[3] = element.R == 8 ? null : Board[element.R + 1, element.C];
            return elementsAround;
        }

        private void LoadInitialState()
        {
            _board = new IGameBoardElement[9, 9];
            this.LoadBodyguards();
            this.LoadKing();
            this.LoadMercenaries();
        }

        private void LoadMercenaries()
        {
            for (int i = 3; i < 6; i++)
            {
                Board[0, i] = new Mercenary() { R = 0, C = i };
                Board[i, 0] = new Mercenary() { R = i, C = 0 };
                Board[8, i] = new Mercenary() { R = 8, C = i };
                Board[i, 8] = new Mercenary() { R = i, C = 8 };

                if (i == 4)
                {
                    Board[1, i] = new Mercenary() { R = 1, C = i };
                    Board[i, 1] = new Mercenary() { R = i, C = 1 };
                    Board[7, i] = new Mercenary() { R = 7, C = i };
                    Board[i, 7] = new Mercenary() { R = i, C = 7 };
                }
            }
        }

        private void LoadKing()
        {
            Board[4, 4] = new King() { R = 4, C = 4 };
        }

        private void LoadBodyguards()
        {
            for (int i = 2; i < 7; i++)
            {
                Board[i, 4] = new Bodyguard() { R = i, C = 4 };
                Board[4, i] = new Bodyguard() { R = 4, C = i };
            }
        }
    }
}
