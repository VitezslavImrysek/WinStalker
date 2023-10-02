using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WinStalker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var dispatcher = Dispatcher;
            var dpi = VisualTreeHelper.GetDpi(this);

            var callback = new DispatcherOperationCallback(OnSet);

            Task.Run(async () =>
            {
                var data = new Data();

                while (true)
                {
                    var modifiers = System.Windows.Forms.Control.ModifierKeys;
                    if (((modifiers & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control)
                        && ((modifiers & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift))
                    {
                        await Task.Delay(2);
                    }
                    else
                    {
                        var screenPosition = System.Windows.Forms.Control.MousePosition;

                        data.X = (int)((screenPosition.X + 20) / dpi.DpiScaleX);
                        data.Y = (int)((screenPosition.Y + 20) / dpi.DpiScaleY);

                        await dispatcher.BeginInvoke(DispatcherPriority.Normal, callback, data);

                        await Task.Delay(1);
                    }
                }
            });
        }

        private object OnSet(object arg)
        {
            var data = arg as Data;

            Left = data.X;
            Top = data.Y;

            return null;
        }

        private class Data
        {
            public int X;
            public int Y;
        }
    }
}
