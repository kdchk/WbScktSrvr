using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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

                    Console.WriteLine("Console says: Connected");
                    
                   
                    WebSocket webSocket = await http.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("1-WebSocket webSocket = await http.WebSockets.AcceptWebSocketAsync();");

                    while (webSocket.State == WebSocketState.Open)
                    {
                        var token = CancellationToken.None; Console.WriteLine("2-var token: " + token);

                        var buffer = new ArraySegment<Byte>(new Byte[20]); Console.WriteLine("3-var buffer: " + buffer); //initial value is 4096

                        //code below plays after "Send" in "Smart Websocket Client"
                        var received = await webSocket.ReceiveAsync(buffer, token); Console.WriteLine("4-var received: " + received);

                        switch (received.MessageType)
                        {
                            case WebSocketMessageType.Text:

                                Console.WriteLine("5-case WebSocketMessageType.Text: ");
                                var request = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);   Console.WriteLine("6-var request: " + request);
                                Console.WriteLine("MESSAGE: "+ request);
                                var type = WebSocketMessageType.Text;    Console.WriteLine("7-var type: " + type);
                                var data = Encoding.UTF8.GetBytes("Echo from server: " + request);   Console.WriteLine("8-var data: " + data);
                                buffer = new ArraySegment<Byte>(data); Console.WriteLine("9-buffer: " + buffer);
                                await webSocket.SendAsync(buffer, type, true, token);
                                Console.WriteLine("10-switch (received.MessageType)");
                                Console.WriteLine(" ");
                                break;
                        }
                    }

                }
            });
        }
    }
}
