using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsCommunication
{
    public enum eRequestType
    {
        MoveElement,
        CanMoveElement,
        GetUpdatedGameBoard,
        RemoveCapturedElementsAfterLastMovement,
        GetCapturedElementsAfterLastMovement,
        GetGameStatus,
        CloseSocket,
        ResetGameBoard
    }
}
