/*using TaxtapulStroyBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

 
        var botClient = new TelegramBotClient("8128993205:AAFJcSCnLA7yoUxH06WVbRRYbyI_QkH1olc");
ProductService productService = new ProductService();

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);

        Console.WriteLine("Bot ishga tushdi...");
        await Task.Delay(-1); // Bot doimiy ishlashi uchun 

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is { } message && message.Text is { } text)
    {
        var product = productService.Products.Find(p => p.code == text);
        if (product != null)
        {
            string info = $"{product.Name}\nNarxi: {product.Price}\nMa'lumot: {product.Description}";
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: info);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Mahsulot topilmadi.");
        }
    }
}

     Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"Xatolik: {exception.Message}");
    return Task.CompletedTask;
}
*/



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