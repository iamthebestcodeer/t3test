namespace T3Code.BackendHost;

public sealed class PortAllocator
{
    private readonly Random _random = new();

    public static int ReservePort(string host = "127.0.0.1")
    {
        using var listener = new System.Net.Sockets.TcpListener(
            System.Net.IPAddress.Parse(host), 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public int GetRandomEphemeralPort()
    {
        return _random.Next(49152, 65535);
    }
}
