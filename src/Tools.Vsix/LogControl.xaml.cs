using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private DispatcherTimer _timer;
        private bool _synced;

        public LogControl()
        {
            InitializeComponent();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _timer.Tick += OnTimerTicker;
            _timer.Start();
        }

        public void Close()
        {
            _timer.Stop();
        }

        private void OnTimerTicker(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            lock (_stringBuilder)
            {
                if (!_synced)
                {
                    _textBox.Text = _stringBuilder.ToString();
                    _textBox.ScrollToEnd();
                    _synced = true;
                }
            }
        }

        public void Log(string str)
        {
            lock (_stringBuilder)
            {
                _stringBuilder.Append(str);
                _synced = false;
            }
        }

        public void Clear()
        {
            lock (_stringBuilder)
            {
                _stringBuilder.Clear();
                _synced = false;
            }
        }
    }
}
