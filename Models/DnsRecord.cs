using CloudflareDnsManager.ViewModels;

namespace CloudflareDnsManager.Models;

public class DnsRecord : ViewModelBase
{
    public string Id { get; set; } = "";

    public string Type { get; set; } = "";

    public string Name { get; set; } = "";

    public string Content { get; set; } = "";

    public int Ttl { get; set; }

    public bool Proxied { get; set; }

    private int _priority;
    public int Priority
    {
        get => _priority;
        set { _priority = value; OnPropertyChanged(); }
    }

    private bool _isModified;
    public bool IsModified
    {
        get => _isModified;
        set
        {
            _isModified = value;
            OnPropertyChanged();
        }
    }
}

