using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsEngine
{
    [Serializable]
    public class Mercenary: IGameBoardElement
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
                return true;
            }
        }

        public bool IsCatchable
        {
            get
            {
                return true;
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
                return eTeam.Black;
            }
        }
    }
}
