using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FrontendRunner : IHostedService
{
    private readonly IWebHostEnvironment _env;

    public FrontendRunner(IWebHostEnvironment env)
    {
        _env = env;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_env.IsDevelopment())
        {
            Console.WriteLine(" ******* Starting Frontend... ********************");
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/k npm start",
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp"),
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
