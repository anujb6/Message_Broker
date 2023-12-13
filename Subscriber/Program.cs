using Subscriber.DTO;
using System.Net.Http.Json;


Console.WriteLine("Press ESC to stop");

do
{
    HttpClient client = new HttpClient();
    Console.WriteLine("Listening....");
    while (!Console.KeyAvailable)
    {
        List<int> ackIds = await GetmessageAsync(client);

        Thread.Sleep(2000);
        if (ackIds.Count > 0)
            await AckMessagesAsync(client, ackIds);
    }
} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

static async Task<List<int>> GetmessageAsync(HttpClient client)
{
    List<int> ackIds = new();
    List<MessageReadDTO>? newMessages = new();

    try
    {
        newMessages = await client.GetFromJsonAsync<List<MessageReadDTO>>("https://localhost:7086/api/subscriptions/1/messages");
    }
    catch(Exception ex)
    {
        return ackIds;
    }

    foreach(MessageReadDTO msg in newMessages!)
    {
        Console.WriteLine($"{msg.Id} - {msg.TopicMessage} - {msg.MessageStatus}");
        ackIds.Add(msg.Id);
    }

    return ackIds;
}

static async Task AckMessagesAsync(HttpClient client, List<int> ackIds)
{
    var response = await client.PostAsJsonAsync("https://localhost:7086/api/subscriptions/1/messages", ackIds);
    var returnMessage = await response.Content.ReadAsStringAsync();

    Console.WriteLine(returnMessage);
}