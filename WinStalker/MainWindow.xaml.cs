using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using Control = System.Windows.Forms.Control;
using Keys = System.Windows.Forms.Keys;
using Point = System.Drawing.Point;
using PInvoke = Windows.Win32.PInvoke;

namespace WinStalker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RunMouseLoop();
        }

        private void RunMouseLoop()
        {
            _cts = new CancellationTokenSource();

            var dispatcher = Dispatcher;
            var dpi = VisualTreeHelper.GetDpi(this);

            Task.Run(() => OnMouseLoop(dispatcher, dpi, _cts.Token));
        }

        private async Task OnMouseLoop(Dispatcher dispatcher, DpiScale dpi, CancellationToken token)
        {
            const int DelaySleep = 16;
            const int DelayStandBy = 2;
            const int DelayKeyboard = 16;
            const int SleepTicks = 500 / DelayStandBy; // 500 ms divided by time for one tick

            var sleepTicks = SleepTicks;
            var mousePos = Point.Empty;

            var data = new WindowsPosition();
            var callback = new DispatcherOperationCallback(WindowPositionCallback);

            var manualReset = new ManualResetEvent(false);

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var modifiers = Control.ModifierKeys;
                if (((modifiers & Keys.Control) == Keys.Control) && ((modifiers & Keys.Shift) == Keys.Shift))
                {
                    sleepTicks = SleepTicks;
                    await Task.Delay(DelayKeyboard);
                }
                else
                {
                    PInvoke.GetCursorPos(out var mousePosNew);
                    if (mousePosNew == mousePos)
                    {
                        if (sleepTicks > 0)
                        {
                            sleepTicks--;

                            // Wait actively, Task.Delay has resolution of about 10-16ms.
                            manualReset.WaitOne(DelayStandBy);
                        }
                        else
                        {
                            await Task.Delay(DelaySleep);
                        }
                    }
                    else
                    {
                        sleepTicks = SleepTicks;
                        mousePos = mousePosNew;

                        data.X = (int)((mousePos.X + 20) / dpi.DpiScaleX);
                        data.Y = (int)((mousePos.Y + 20) / dpi.DpiScaleY);

                        await dispatcher.BeginInvoke(DispatcherPriority.Normal, callback, data);
                    }
                }
            }
        }

        private object WindowPositionCallback(object arg)
        {
            var data = arg as WindowsPosition;

            Left = data.X;
            Top = data.Y;

            return null;
        }

        private class WindowsPosition
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
