using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoopCheckTool
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands), new InputGestureCollection()
        {
            new KeyGesture(Key.F4, ModifierKeys.Alt)
        });
        public static readonly RoutedUICommand Settings = new RoutedUICommand("Settings", "Settings", typeof(CustomCommands));
    }
}
