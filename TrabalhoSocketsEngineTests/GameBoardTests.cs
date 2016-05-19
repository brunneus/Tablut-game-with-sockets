using System;
using NUnit.Framework;
using System.Linq;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsEngineTests
{
    [TestFixture]
    public class _gameBoardTests
    {
        private GameBoard _gameBoard;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            _gameBoard = new GameBoard();
            _gameBoard.Initialize();
        }

        [Test]
        [TestCase(3, 0, 3, 3, true)]
        [TestCase(3, 0, 0, 0, true)]
        [TestCase(3, 0, 3, 0, true)]
        [TestCase(5, 0, 8, 0, true)]
        [TestCase(3, 0, 8, 0, false)]
        [TestCase(3, 0, 3, 4, false)]
        [TestCase(3, 0, 3, 8, false)]
        [TestCase(3, 0, 4, 2, false)]
        [TestCase(4, 4, 0, 2, false)]
        [TestCase(4, 4, 5, 2, false)]
        [TestCase(4, 0, 3, 5, false)]
        [TestCase(1, 0, -3, 5, false)]
        [TestCase(0, 1, 3, 15, false)]
        [TestCase(0, 1, 3, -5, false)]
        [TestCase(0, 1, -3, 5, false)]
        public void CanMoveTo(int elementX, int elementY, int targetX, int targetY, bool expectedResult)
        {
            _gameBoard.Initialize();
            var elementToMove = _gameBoard.Board[elementX, elementY];

            Assert.AreEqual(_gameBoard.CanMoveTo(elementToMove, targetX, targetY), expectedResult);
        }

        [Test]
        [TestCase(3, 0, 0, 0)]
        [TestCase(3, 0, 1, 0)]
        [TestCase(3, 0, 2, 0)]
        [TestCase(4, 1, 8, 1)]
        public void MoveTo(int elementR, int elementC, int targetR, int targetC)
        {
            _gameBoard.Initialize();
            var elementToMove = _gameBoard.Board[elementR, elementC];
            _gameBoard.MoveTo(elementToMove, targetR, targetC);

            Assert.AreEqual(elementToMove.R, targetR);
            Assert.AreEqual(elementToMove.C, targetC);
            Assert.AreEqual(_gameBoard.Board[targetR, targetC], elementToMove);
            Assert.AreEqual(_gameBoard.Board[elementR, elementC], null);
        }

        [Test]
        [TestCase(3, 0)]
        [TestCase(3, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 1)]
        public void Remove(int elementR, int elementC)
        {
            
            _gameBoard.Initialize();
            var elementToRemove = _gameBoard.Board[elementR, elementC];
            _gameBoard.RemoveElement(elementToRemove);

            Assert.AreEqual(_gameBoard.Board[elementR, elementC], null);
        }

        [Test]
        public void CaptureIfPossibleCaseA()
        {
            
            _gameBoard.Initialize();
            var elementToMoveA = _gameBoard.Board[3, 0];
            var elementToMoveB = _gameBoard.Board[3, 8];
            _gameBoard.MoveTo(elementToMoveA, 3, 3);
            _gameBoard.MoveTo(elementToMoveB, 3, 5);

            var elementsCaptured = _gameBoard.GetElementsToBeCapturedAfterLastMovement(elementToMoveB);

            Assert.AreEqual(elementsCaptured.Count(), 1);
            Assert.AreEqual(elementsCaptured.ElementAt(0).R, 3);
            Assert.AreEqual(elementsCaptured.ElementAt(0).C, 4);
        }

        [Test]
        public void CaptureIfPossibleCaseB()
        {
            
            _gameBoard.Initialize();
            var elementToMoveA = _gameBoard.Board[0, 3];
            var elementToMoveB = _gameBoard.Board[8, 3];
            _gameBoard.MoveTo(elementToMoveA, 3, 3);
            _gameBoard.MoveTo(elementToMoveB, 5, 3);

            var elementsCaptured = _gameBoard.GetElementsToBeCapturedAfterLastMovement(elementToMoveB);

            Assert.AreEqual(elementsCaptured.Count(), 1);
            Assert.AreEqual(elementsCaptured.ElementAt(0).R, 4);
            Assert.AreEqual(elementsCaptured.ElementAt(0).C, 3);
        }

        [Test]
        public void KingIsSorroundByMercenariesCaseA()
        {
            
            _gameBoard.Initialize();
            _gameBoard.RemoveRangeOfElements(_gameBoard.Board.OfType<Bodyguard>());
            _gameBoard.MoveFromTo(4, 1, 4, 3);
            _gameBoard.MoveFromTo(1, 4, 3, 4);
            _gameBoard.MoveFromTo(7, 4, 5, 4);
            _gameBoard.MoveFromTo(4, 7, 4, 5);

            Assert.IsTrue(_gameBoard.KingIsSorroundByMercenaries());
        }

        [Test]
        public void KingIsSorroundByMercenariesCaseB()
        {
            
            _gameBoard.Initialize();
            _gameBoard.RemoveRangeOfElements(_gameBoard.Board.OfType<Bodyguard>());
            _gameBoard.MoveFromTo(3, 0, 3, 3);
            _gameBoard.MoveFromTo(4, 4, 3, 4);
            _gameBoard.MoveFromTo(1, 4, 2, 4);
            _gameBoard.MoveFromTo(3, 8, 3, 5);

            Assert.IsTrue(_gameBoard.KingIsSorroundByMercenaries());
        }

        [Test]
        public void TeamHasSomeValidMovementsCaseA()
        {
            
            _gameBoard.Initialize();
            _gameBoard.RemoveRangeOfElements(_gameBoard.Board.OfType<Bodyguard>());
            _gameBoard.MoveFromTo(3, 0, 3, 3);
            _gameBoard.MoveFromTo(4, 4, 3, 4);
            _gameBoard.MoveFromTo(1, 4, 2, 4);
            _gameBoard.MoveFromTo(3, 8, 3, 5);

            Assert.IsFalse(_gameBoard.TeamHasSomeValidMovements(eTeam.White));
        }

        [Test]
        public void TeamHasSomeValidMovementsCaseB()
        {
            
            _gameBoard.Initialize();
            _gameBoard.RemoveRangeOfElements(_gameBoard.Board.OfType<Bodyguard>());
            _gameBoard.RemoveRangeOfElements(_gameBoard.Board.OfType<King>());
            _gameBoard.Board[0, 0] = new Bodyguard();
            _gameBoard.Board[1, 0] = new Mercenary();
            _gameBoard.Board[0, 1] = new Mercenary();

            Assert.IsFalse(_gameBoard.TeamHasSomeValidMovements(eTeam.White));
        }
    }
}
