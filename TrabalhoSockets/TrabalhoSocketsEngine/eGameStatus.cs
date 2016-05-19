using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsEngine
{
    public enum eGameStatus
    {
        Running,
        WhiteTeamWithoutValidMovements,
        BlackTeamWithoutValidMovements,
        KingArriveAtSomeSide,
        KingSorroundByMercenaries
    }
}
