using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace TrabalhoSocketsUI.Behaviors
{
    public class MouseEnterGameBoardControlBehavior : Behavior<GameBoardElementControl>
    {
        public MainWindowViewModel MainWindowViewModel
        {
            get { return (MainWindowViewModel)GetValue(MainWindowViewModelProperty); }
            set { SetValue(MainWindowViewModelProperty, value); }
        }

        public static readonly DependencyProperty MainWindowViewModelProperty =
            DependencyProperty.Register("MainWindowViewModel", typeof(MainWindowViewModel), typeof(MouseEnterGameBoardControlBehavior));

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            this.AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.AssociatedObject.Background = Brushes.Transparent;
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var wrapper = this.AssociatedObject.DataContext as GameBoardElementWrapper;
            var selectedWrapper = this.MainWindowViewModel.SelectedWrapper;
            
            if (selectedWrapper != null)
            {
                var gameBoard = MainWindowViewModel.Client.GetUpdatedGameBoard();
                if (gameBoard.CanMoveTo(selectedWrapper.Element, wrapper.R, wrapper.C))
                    AssociatedObject.Background = Brushes.Green;
                else
                    AssociatedObject.Background = Brushes.Red;
            }
            else
            {
                if (wrapper.Element == null || wrapper.Element.Team != MainWindowViewModel.CurrentTeamPlay)
                {
                    this.AssociatedObject.Background = Brushes.Transparent;
                }
                else
                {
                    this.AssociatedObject.Background = Brushes.LightBlue;
                }
            }
        }
    }
}
