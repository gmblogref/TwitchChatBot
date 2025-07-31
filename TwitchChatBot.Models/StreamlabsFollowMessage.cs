using System.Text.Json.Serialization;

public class StreamlabsFollowWrapper
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public List<StreamlabsFollowMessage>? Message { get; set; }
}

public class StreamlabsFollowMessage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}