using System;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SteemDataScraper
{
    public class TelegramLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ITelegramBotClient _bot;
        private readonly ChatId _chatId;
        private bool _selfException;

        public TelegramLogger(string categoryName, Func<string, LogLevel, bool> filter, ITelegramBotClient bot, ChatId chatId)
        {
            _categoryName = categoryName;
            _filter = filter;
            _bot = bot;
            _chatId = chatId;
        }

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (_selfException)
            {
                _selfException = false;
                return;
            }

            _selfException = true;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
                return;

            if (exception != null)
            {
                message += "\n" + exception;
            }

            try
            {
                var msg = await _bot.SendTextMessageAsync(_chatId, $"{eventId.Id} {logLevel}: {message}", ParseMode.Default, true, true);

                _selfException = false;
            }
            catch
            {
                //todo nothing
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_chatId != null && (!string.IsNullOrEmpty(_chatId.Username) || _chatId.Identifier != 0) && (_filter == null || _filter(_categoryName, logLevel)));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
