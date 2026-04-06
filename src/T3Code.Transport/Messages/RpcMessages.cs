using System.Text.Json;
using System.Text.Json.Serialization;

namespace T3Code.Transport.Messages;

public sealed class RpcRequest
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

public sealed class RpcResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("error")]
    public RpcError? Error { get; set; }
}

public sealed class RpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

public sealed class RpcPush
{
    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

public sealed class RpcMessage
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("error")]
    public RpcError? Error { get; set; }

    public bool IsResponse => Id.HasValue && (Result.HasValue || Error != null);
    public bool IsPush => !Id.HasValue && Method != null;
}
