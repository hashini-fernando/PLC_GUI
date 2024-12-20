using WebSocketSharp;
using WebSocketSharp.Server;

public class MyWebSocketService : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("Message received from client: " + e.Data);
    }

    public void SendUpdate(string message)
    {
        Send(message);
    }
}

public class WebSocketManager
{
    private WebSocketServer _server;

    public WebSocketManager(int port)
    {
        _server = new WebSocketServer($"ws://localhost:{port}");
        _server.AddWebSocketService<MyWebSocketService>("/updates");
    }

    public void Start()
    {
        _server.Start();
        Console.WriteLine("WebSocket server started.");
    }

    public void Stop()
    {
        _server.Stop();
        Console.WriteLine("WebSocket server stopped.");
    }

    // This method can be called from other classes to broadcast updates

 
        public void BroadcastUpdate(string message)
    {
        // Use the Sessions.Broadcast method to send messages to all connected clients
        _server.WebSocketServices["/updates"].Sessions.Broadcast(message);
    }
}
