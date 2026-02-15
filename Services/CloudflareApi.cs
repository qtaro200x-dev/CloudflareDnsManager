using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CloudflareDnsManager.Models;

namespace CloudflareDnsManager.Services;

public class CloudflareApi
{
    private readonly HttpClient _client;

    public CloudflareApi()
    {
        _client = new HttpClient();
    }

    public void SetToken(string apiToken)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiToken);
    }

    public async Task<List<DnsRecord>> GetDnsRecordsAsync(string zoneId)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records";

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var list = new List<DnsRecord>();

        if (root.TryGetProperty("result", out var result))
        {
            foreach (var record in result.EnumerateArray())
            {
                var dns = new DnsRecord
                {
                    Id      = record.GetProperty("id").GetString() ?? "",
                    Type    = record.GetProperty("type").GetString() ?? "",
                    Name    = record.GetProperty("name").GetString() ?? "",
                    Content = record.GetProperty("content").GetString() ?? "",
                    Ttl     = record.GetProperty("ttl").GetInt32(),
                    Proxied = record.TryGetProperty("proxied", out var proxied)
                                && proxied.GetBoolean(),
                    IsModified = false
                };

                list.Add(dns);
            }
        }

        return list;
    }

    public async Task UpdateDnsRecordAsync(string zoneId, DnsRecord record)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{record.Id}";

        var payload = new
        {
            type    = record.Type,
            name    = record.Name,
            content = record.Content,
            ttl     = record.Ttl,
            proxied = record.Proxied
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
    }
}
