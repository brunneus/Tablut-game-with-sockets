using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsUI
{
    public class GameBoardElementWrapper : INotifyPropertyChanged
    {
        public GameBoardElementWrapper(IGameBoardElement element)
        {
            _element = element;
        }
        
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        private IGameBoardElement _element;
        public IGameBoardElement Element
        {
            get
            {
                return _element;
            }
            set
            {
                _element = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Element"));
            }
        }
        
        public int C { get; set; }

        public int R { get; set; }
    }
}
