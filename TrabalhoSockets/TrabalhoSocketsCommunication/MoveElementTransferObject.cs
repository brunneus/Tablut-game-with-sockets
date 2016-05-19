using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsCommunication
{
    [Serializable]
    public class MoveElementTransferObject
    {
        public IGameBoardElement GameBoardElement { get; set; }
        public int TargetR { get; set; }
        public int TargetC { get; set; }
    }
}
