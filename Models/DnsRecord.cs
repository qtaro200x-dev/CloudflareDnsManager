namespace CloudflareDnsManager.Models;

public class DnsRecord
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Content { get; set; } = "";
    public int Ttl { get; set; }
    public bool Proxied { get; set; }

    public bool IsModified { get; set; }
}

