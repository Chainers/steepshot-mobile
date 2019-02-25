using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SteemDataScraper
{
    public class TelegramLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly ITelegramBotClient _botClient;
        private readonly ChatId _chatId;


        public TelegramLoggerProvider(IConfiguration configuration) : this(configuration, null) { }

        public TelegramLoggerProvider(IConfiguration configuration, Func<string, LogLevel, bool> filter)
        {
            _filter = filter;
            var bot = new TelegramBot();
            configuration.GetSection("TelegramBot").Bind(bot);
            _botClient = new TelegramBotClient(bot.Token);
            //var u = _botClient.GetUpdatesAsync().Result;
            _chatId = new ChatId(bot.Chanel);
        }


        public ILogger CreateLogger(string categoryName)
        {
            return new TelegramLogger(categoryName, _filter, _botClient, _chatId);
        }

        public void Dispose()
        {
        }

        private class TelegramBot
        {
            public string Token { get; set; }

            public long Chanel { get; set; }
        }
    }
}