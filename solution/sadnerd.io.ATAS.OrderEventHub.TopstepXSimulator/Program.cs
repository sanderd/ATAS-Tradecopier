// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Hello, World!");

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/topstepxhub")
    .Build();

connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0, 5) * 1000);
    await connection.StartAsync();
};

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    
});

await connection.StartAsync();