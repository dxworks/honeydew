using Honeydew.Logging;
using Konsole;

namespace Honeydew
{
    public class ProgressLoggerBar : IProgressLoggerBar
    {
        private readonly string _initialText;
        private readonly IProgressBar _progressBar;
        private int _maxLength;

        public ProgressLoggerBar(IProgressBar progressBar, string initialText)
        {
            _progressBar = progressBar;
            _initialText = initialText;
        }

        public void Start()
        {
            var text = $"{_initialText}";
            _maxLength = text.Length;
            _progressBar.Refresh(0, text);
        }

        public void Step(string text)
        {
            if (_maxLength < text.Length)
            {
                _maxLength = text.Length;
            }

            _progressBar.Next(text.PadRight(_maxLength));
        }

        public void Stop()
        {
            _progressBar.Refresh(_progressBar.Max, $"Done".PadRight(_maxLength));
        }
    }
}
