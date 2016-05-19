using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsEngine
{
    public interface IGameBoardElement
    {
        int C { get; set; }
        int R { get; set; }
        eTeam Team { get; }
        bool IsCatchable { get; }
        bool CanCapture { get; }
    }
}
