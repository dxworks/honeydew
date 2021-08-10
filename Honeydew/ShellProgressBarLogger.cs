using HoneydewCore.Logging;
using ShellProgressBar;

namespace Honeydew
{
    public class ShellProgressBarLogger : IProgressLogger
    {
        private readonly string _initialText;
        private readonly IProgressBar _progressBar;

        public ShellProgressBarLogger(IProgressBar progressBar, string initialText)
        {
            _progressBar = progressBar;
            _initialText = initialText;
        }

        public void Start()
        {
            _progressBar.Message = $"Start {_initialText}";
        }

        public void Step(string text)
        {
            _progressBar.Tick(text);
        }

        public void Stop()
        {
            _progressBar.Message = $"Done {_initialText}";
            _progressBar.Dispose();
        }
    }
}
