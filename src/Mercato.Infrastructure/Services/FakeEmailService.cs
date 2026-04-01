using Mercato.Application.Common.Interfaces;

namespace Mercato.Infrastructure.Services;

public class FakeEmailService : IEmailService
{
    public Task SendAsync(string to, string subject, string body)
    {
        Console.WriteLine("===== EMAIL =====");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine(body);
        Console.WriteLine("=================");

        return Task.CompletedTask;
    }
}