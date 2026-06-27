using Akka.Actor;
using FootballLineups.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FootballLineups.Server
{
    public class WebServer
    {
        private readonly HttpListener _listener = new();
        private readonly IActorRef _coordinator;
        private static readonly object _consoleLock = new();

        public WebServer(string prefix, IActorRef coordinator)
        {
            _coordinator = coordinator;
            _listener.Prefixes.Add(prefix);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Log("[Server] Web server pokrenut na http://localhost:5000/");

            while (true)
            {
                var context = await _listener.GetContextAsync();
                _ = ProcessRequestAsync(context);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            Log($"[Server] Zahtev primljen: {request.HttpMethod} {request.Url}");

            try
            {
                //url format: /lineup?fixtureId=123
                if (request.Url!.AbsolutePath == "/lineup")
                {
                    var query = request.QueryString;
                    var idStr = query["fixtureId"];

                    if(int.TryParse(idStr, out int fixtureId) && fixtureId > 0)
                    {
                        var result = await _coordinator.Ask<LineupResponse>(
                            new GetLineup(fixtureId),
                            TimeSpan.FromSeconds(5)
                        );

                        string body;
                        if(result.Found)
                        {
                            body = $"Utakmica: {result.Name}\n\n";
                            body += string.Join("\n", result.Players.Select(p => p.ToString()));
                            Log($"[Server] Odgovor poslan za utakmicu {fixtureId}" +
                                              $"- {result.Players.Count} igraca");
                        }
                        else
                        {
                            body = $"Utakmica {fixtureId} nije pronadjena ili jos nema podataka.";
                            Log($"[Server] Utakmica {fixtureId} nije pronadjena.");
                        }

                        await WriteResponseAsync(response, body, 200);
                    }
                    else
                    {
                        await WriteResponseAsync(response, "Neispravan fixtureId.", 400);
                        Log("[Server] Greska: neispravan fixtureId.");
                    }
                }
                else
                {
                    await WriteResponseAsync(response, "Endpoint nije pronadjen.", 404);
                    Log($"[Server] Greska 404: {request.Url.AbsolutePath}");
                }
            }
            catch (Exception ex)
            {
                Log($"[Server] Greska pri obradi zahteva: {ex.Message}");
                await WriteResponseAsync(response, $"Greska: {ex.Message}", 500);
            }
        }

        private async Task WriteResponseAsync(HttpListenerResponse response, string body, int statusCode)
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/plain; charset=utf-8";
            var buffer = System.Text.Encoding.UTF8.GetBytes(body);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
        }

        private static void Log(string message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine(message);
            }
        }
    }
}
