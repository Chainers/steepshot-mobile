using System;
using FFImageLoading.Helpers;

namespace Steepshot.iOS.Models
{
    public class EmptyLogger : IMiniLogger
    {
        public void Debug(string message)
        {
        }

        public void Error(string errorMessage)
        {
        }

        public void Error(string errorMessage, Exception ex)
        {
        }
    }
}
