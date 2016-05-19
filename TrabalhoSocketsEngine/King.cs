using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsEngine
{
    [Serializable]
    public class King : IGameBoardElement
    {
        private int _c;
        private int _r;

        public int C
        {
            get
            {
                return _c;
            }

            set
            {
                _c = value;
            }
        }

        public bool CanCapture
        {
            get
            {
                return false;
            }
        }

        public bool IsCatchable
        {
            get
            {
                return false; 
            }
        }

        public int R
        {
            get
            {
                return _r;
            }

            set
            {
                _r = value;
            }
        }

        public eTeam Team
        {
            get
            {
                return eTeam.White;
            }
        }
    }
}
