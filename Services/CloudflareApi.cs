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

    // -------------------------------
    // 1. DNS レコード一覧取得 (GET)
    // -------------------------------
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
            Priority = record.TryGetProperty("priority", out var pri)
                        ? pri.GetInt32()
                        : 0,
            IsModified = false
        };


                list.Add(dns);
            }
        }

        return list;
    }

    // -------------------------------
    // 2. DNS レコード更新 (PUT)
    // -------------------------------
    public async Task UpdateDnsRecordAsync(string zoneId, DnsRecord record)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{record.Id}";

        var payload = new
        {
            type    = record.Type,
            name    = record.Name,
            content = record.Content,
            ttl     = record.Ttl,
            proxied = record.Proxied,
            priority = record.Type == "MX" ? record.Priority : (int?)null
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
    }

    // -------------------------------
    // 3. DNS レコード追加 (POST)
    // -------------------------------
    public async Task<DnsRecord> CreateDnsRecordAsync(string zoneId, DnsRecord record)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records";

        var payload = new
        {
            type    = record.Type,
            name    = record.Name,
            content = record.Content,
            ttl     = record.Ttl,
            proxied = record.Proxied,
            priority = record.Type == "MX" ? record.Priority : (int?)null
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var resJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(resJson);
        var root = doc.RootElement;

        var result = root.GetProperty("result");

        return new DnsRecord
        {
            Id      = result.GetProperty("id").GetString() ?? "",
            Type    = result.GetProperty("type").GetString() ?? "",
            Name    = result.GetProperty("name").GetString() ?? "",
            Content = result.GetProperty("content").GetString() ?? "",
            Ttl     = result.GetProperty("ttl").GetInt32(),
            Proxied = result.TryGetProperty("proxied", out var proxied)
                        && proxied.GetBoolean(),
            IsModified = false
        };
    }

    // -------------------------------
    // 4. DNS レコード削除 (DELETE)
    // -------------------------------
    public async Task DeleteDnsRecordAsync(string zoneId, string recordId)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordId}";
        var response = await _client.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }
}
