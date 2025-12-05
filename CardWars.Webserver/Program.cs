var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/api", (object res) =>
{
	Console.WriteLine("Received");
	Console.WriteLine(res.GetType());
	Console.WriteLine(res);
});

app.UseFileServer();
app.Run();