using Microsoft.EntityFrameworkCore;
using TaxtapulStroyBot.Entities;
using TaxtapulStroyBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using (var context = new AppDbContext())
{
    if (context.Database.CanConnect())
    {
        Console.WriteLine("✅ Database bilan ulanish o‘rnatildi!");
    }
    else
    {
        Console.WriteLine("❌ Database bilan ulanishda xatolik bor!");
    }
}


var botClient = new TelegramBotClient("8128993205:AAFJcSCnLA7yoUxH06WVbRRYbyI_QkH1olc");
ProductService productService = new ProductService();
Dictionary<long, Product> tempProducts = new();
Dictionary<long, Product> updatingProducts = new Dictionary<long, Product>();
Dictionary<long, string> updatingStage = new();

Console.WriteLine("Bot ishga tushdi...");
var appDbContext = new AppDbContext();
appDbContext.Database.Migrate();


botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
await Task.Delay(-1); // Bot doimiy ishlashi uchun 


async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is { } message)
    {
        long chatId = message.Chat.Id;
        string userText = message.Text;

        if (userText == "/admin")
        {
            await RequestPhoneNumber(botClient, chatId, message.Chat.Type);
        }
        else if (message.Contact is { } contact)
        {
            await CheckAdminAccess(botClient, chatId, contact.PhoneNumber);
        }
        else if (userText == "📦 Tovarlar")
        {
            await ShowProducts(botClient, chatId);
        }
        else if (userText == "➕ Tovar qo'shish")
        {
            await StartProductAdding(botClient, chatId);
        }
        else if (userText == "✏️ Tovar yangilash")
        {
            await StartProductUpdating(botClient, chatId);
        }
        else if (tempProducts.ContainsKey(chatId))
        {
            await ContinueProductAdding(botClient, chatId, userText);
        }
        else if (updatingProducts.ContainsKey(chatId))
        {
            await ContinueProductUpdating(botClient, chatId, userText);
        }
        else
        {
            string userCode = message.Text;

            var product = appDbContext.Products.FirstOrDefault(p => p.code == userCode);

            if (product != null)
            {
                string info = $"Код: {product.code}\n" +
                              $"Малумотлари: {product.Description}\n" +
                              $"Эни калинлиги: {product.Thickness}\n" +
                              $"Узунлиги: {product.Length}\n" +
                              $"Почка мт: {product.PackLength}\n" +
                              $"Нархи: {product.Price}\n";
                await botClient.SendTextMessageAsync(message.Chat.Id, info);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "❌ Bunday kod topilmadi. Iltimos, to‘g‘ri kod kiriting.");
            }
        }
    }
}


Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"Xatolik: {exception.Message}");
    return Task.CompletedTask;
}


async Task RequestPhoneNumber(ITelegramBotClient botClient, long chatId, ChatType chatType)
{
    if (chatType != ChatType.Private)
    {
        await botClient.SendTextMessageAsync(chatId, "❌ Telefon raqamini faqat shaxsiy suhbatda so‘rash mumkin.");
        return;
    }

    var requestPhoneKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton("📞 Telefon raqamni yuborish") { RequestContact = true }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true,
    };

    await botClient.SendTextMessageAsync(chatId, "Iltimos, telefon raqamingizni yuboring 📲", replyMarkup: requestPhoneKeyboard);
}


async Task CheckAdminAccess(ITelegramBotClient botClient, long chatId, string userPhone)
{
    if (AdminNumbers.AdminPhones.Contains(userPhone))
    {
        await botClient.SendTextMessageAsync(chatId, text: "\"✅ Siz admin sifatida tizimga kirdingiz!\"",
            replyMarkup: AdminMenu());
    }
    else
    {
        await botClient.SendTextMessageAsync(chatId, text: "❌ Kechirasiz, siz admin emassiz.");
    }
}


IReplyMarkup AdminMenu()
{
    return new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[]
            {
                "📦 Tovarlar","➕ Tovar qo'shish"
            },
        })
    {
        ResizeKeyboard = true
    };
}

async Task ShowProducts(ITelegramBotClient botClient, long chatId)
{
    if (appDbContext.Products is null)
    {
        await botClient.SendTextMessageAsync(chatId, "📭 Mahsulotlar hali qo'shilmagan.");
        return;
    }

    string response = "📦 **Mavjud mahsulotlar:**\n";
    foreach (var product in appDbContext.Products)
    {
        response += $"🔹 **{product.Name}** | 🏷 Kod: {product.code} | 💰 {product.Price} so'm\nMa'lumot: {product.Description}";
    }

    await botClient.SendTextMessageAsync(chatId, response);
}

async Task StartProductAdding(ITelegramBotClient botClient, long chatId)
{
    tempProducts[chatId] = new Product();
    await botClient.SendTextMessageAsync(chatId, "📝 Yangi mahsulot qo'shish.\n📌 Mahsulot kodini kiriting:");
}

async Task ContinueProductAdding(ITelegramBotClient botClient, long chatId, string text)
{
    var product = tempProducts[chatId];

    if (string.IsNullOrEmpty(product.code))
    {
        product.code = text;
        await botClient.SendTextMessageAsync(chatId, "📛 Mahsulot nomini kiriting:");
    }
    else if (string.IsNullOrEmpty(product.Name))
    {
        product.Name = text;
        await botClient.SendTextMessageAsync(chatId, "📏 Qalinligini kiriting:");
    }
    else if (string.IsNullOrEmpty(product.Thickness))
    {
        product.Thickness = text;
        await botClient.SendTextMessageAsync(chatId, "📏 Uzunligini kiriting:");
    }
    else if (string.IsNullOrEmpty(product.Length))
    {
        product.Length = text;
        await botClient.SendTextMessageAsync(chatId, "📦 Pachkadagi uzunligini kiriting:");
    }
    else if (string.IsNullOrEmpty(product.PackLength))
    {
        product.PackLength = text;
        await botClient.SendTextMessageAsync(chatId, "Mahsulot narxini yozing:");
    }
    else if (string.IsNullOrEmpty(product.Price))
    {
        product.Price = text;
        await botClient.SendTextMessageAsync(chatId, "ℹ️ Mahsulot haqida qisqa ma'lumot yozing:");
    }
    else
    {
        product.Description = text;
        appDbContext.Products.Add(product);
        appDbContext.SaveChanges();
        tempProducts.Remove(chatId);

        await botClient.SendTextMessageAsync(chatId, "✅ Mahsulot muvaffaqiyatli qo'shildi!", replyMarkup: AdminMenu());
    }
}

async Task StartProductUpdating(ITelegramBotClient botClient, long chatId)
{
    await botClient.SendTextMessageAsync(chatId, "📝 Yangilamoqchi bo'lgan mahsulot kodini kiriting:");
    updatingProducts[chatId] = new Product();
}

async Task ContinueProductUpdating(ITelegramBotClient botClient, long chatId, string text)
{
    if (updatingStage[chatId] == "code")
    {
        var product = appDbContext.Products.FirstOrDefault(p => p.code == text);
        if (product == null)
        {
            await botClient.SendTextMessageAsync(chatId, "❌ Bunday kodli mahsulot topilmadi. Iltimos, to‘g‘ri kod kiriting.");
            return;
        }
        updatingProducts[chatId] = product;
        updatingStage[chatId] = "name";
        await botClient.SendTextMessageAsync(chatId, $"📛 Yangi nomini kiriting (hozirgi: {product.Name}):");
    }
    else if (updatingStage[chatId] == "name")
    {
        updatingProducts[chatId].Name = text;
        updatingStage[chatId] = "price";
        await botClient.SendTextMessageAsync(chatId, "💰 Yangi narxni kiriting:");
    }
    else if (updatingStage[chatId] == "price")
    {
            updatingProducts[chatId].Price = text;
            updatingStage[chatId] = "thickness";
            await botClient.SendTextMessageAsync(chatId, "📏 Yangi qalinlikni kiriting:");
    }
    else if (updatingStage[chatId] == "thickness")
    {
        updatingProducts[chatId].Thickness = text;
        updatingStage[chatId] = "length";
        await botClient.SendTextMessageAsync(chatId, "📏 Yangi uzunlikni kiriting:");
    }
    else if (updatingStage[chatId] == "length")
    {
        updatingProducts[chatId].Length = text;
        updatingStage[chatId] = "packlength";
        await botClient.SendTextMessageAsync(chatId, "📦 Pachkadagi uzunlikni kiriting:");
    }
    else if (updatingStage[chatId] == "packlength")
    {
        updatingProducts[chatId].PackLength = text;
        updatingStage[chatId] = "description";
        await botClient.SendTextMessageAsync(chatId, "📜 Yangi ma'lumotni kiriting:");
    }
    else if (updatingStage[chatId] == "description")
    {
        updatingProducts[chatId].Description = text;
        await CompleteProductUpdating(botClient, chatId);
    }
}

async Task CompleteProductUpdating(ITelegramBotClient botClient, long chatId)
{
    var product = updatingProducts[chatId];
    appDbContext.Products.Update(product);
    appDbContext.SaveChanges();
    updatingProducts.Remove(chatId);
    updatingStage.Remove(chatId);
    await botClient.SendTextMessageAsync(chatId, "✅ Mahsulot yangilandi!");
}







/*
using TaxtapulStroyBot.Entities;
using TaxtapulStroyBot.Services;



ProductService productService = new();
bool isContinued = true;
void Menu()
{
    Console.WriteLine("Do you wanna add product");
    Console.WriteLine("Yes/No");
    var input = Console.ReadLine();
    input = input.ToLower();
    if (input.Contains('y'))
    {
        AskProductDetails();
    }
    else if (input == "n")
    {
        isContinued = false;
    }
}

while (isContinued)
{
    Menu();
}

void AskProductDetails()
{
    var product = new Product();

    Console.Write("Product code >>> ");
    product.code = Console.ReadLine();

    Console.Write("Product nomi >>> ");
    product.Name = Console.ReadLine()!;

    Console.Write("Product qalinligi >>>");
    product.Thickness = Console.ReadLine()!;

    Console.Write("Product uzunligi >>>");
    product.Length = Console.ReadLine()!;

    Console.Write("Product pachkasining uzunligi >>>");
    product.PackLength = Console.ReadLine()!;

    Console.Write("product narxi >>> ");
    product.Price = Console.ReadLine()!;

    Console.Write("Product haqida >>> ");
    product.Description = Console.ReadLine();
    product.Id++;
    productService.AddProduct(product);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Product was added successfully");
    Console.ForegroundColor = ConsoleColor.White;   
}
*/