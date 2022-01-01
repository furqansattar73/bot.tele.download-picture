using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Net;

namespace imgDownloadBot
{
    class Program
    {

        private static TelegramBotClient? bot;

        public static async Task Main()
        {
            bot = new TelegramBotClient("5028651916:AAF-wYqzrST6fxhNmNgwNtVNDES4om1Jg-I");

            User me = await bot.GetMeAsync();
            Console.Title = me.Username ?? "img bot";

            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            bot.StartReceiving(updateHandler,
                               errorHandler,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }

        public static Task errorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        public static async Task updateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await errorHandler(botClient, exception, cancellationToken);
            }
        }

        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;


            var action = message.Text switch
            {
                "/start" => help(botClient, message),
                "/help" => help(botClient, message),
                _ => Other(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");




            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Enter the link of the image you want to download\n"+
                                                            "/start\n"+"/help"
                                                            );
            }

        }



        static async Task<Message> Other(ITelegramBotClient botClient, Message message)
        {

            string recivedURL = message.Text;


            try
            {
                return await botClient.SendPhotoAsync(
            chatId: message.Chat.Id,
            replyToMessageId: message.MessageId,
            photo: recivedURL,
            caption: "Here is your image! 🙏",
            replyMarkup: new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                    "Click to see it online",
                    recivedURL))
                );

            }
            catch
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "The link does not contain an image or is incorrect. Please try again."
                                                            );

            }


        }




    }
}
