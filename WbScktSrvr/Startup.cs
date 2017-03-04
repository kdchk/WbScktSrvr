using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace WbScktSrvr
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                var http = (HttpContext)context;

                if (http.WebSockets.IsWebSocketRequest)
                {
                    Console.WriteLine("Console says: Client connected");

                    WebSocket webSocket = await http.WebSockets.AcceptWebSocketAsync();

                    while (webSocket.State == WebSocketState.Open)
                    {
                        var token = CancellationToken.None;
                        var buffer = new ArraySegment<Byte>(new Byte[8192]);

                        WebSocketReceiveResult result = null;

                        using (var ms = new MemoryStream())
                        {
                            do
                            {
                                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                                ms.Write(buffer.Array, buffer.Offset, result.Count);
                            }
                            while (!result.EndOfMessage);

                            ms.Seek(0, SeekOrigin.Begin);

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                using (var reader = new StreamReader(ms, Encoding.UTF8))
                                {
                                    var message = reader.ReadToEnd();
                                    Console.WriteLine("MESSAGE: " + message);
                                    
                                    var type = WebSocketMessageType.Text;
                                    var data = Encoding.UTF8.GetBytes("Echo from server: " + message);
                                    buffer = new ArraySegment<Byte>(data);
                                    await webSocket.SendAsync(buffer, type, true, token);
                                }
                            }
                        }
                    }

                }
            });
        }
    }
}
