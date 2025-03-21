

using Microsoft.AspNetCore.SignalR.Client;

string ServerUrl = "https://localhost:7228/SendNotification";

var connection = new HubConnectionBuilder()
    .WithUrl(ServerUrl)
    .Build();

// See all the events

connection.On<string>("ReceiveTaskUpdate", (message) =>
{
   Console.WriteLine($"message received: {message}");
});

try
{
    await connection.StartAsync();
    Console.WriteLine("connection started...");
}
catch (Exception ex)
{
    Console.WriteLine($"Ocurred an error during establish connection: {ex.Message}");
}

Console.ReadKey();
