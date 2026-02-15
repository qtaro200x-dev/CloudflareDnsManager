using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CloudflareDnsManager.Models;
using CloudflareDnsManager.Services;

namespace CloudflareDnsManager.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _apiToken = "";
    private string _zoneId = "";
    private bool _isBusy;

    public string ApiToken
    {
        get => _apiToken;
        set { _apiToken = value; OnPropertyChanged(); }
    }

    public string ZoneId
    {
        get => _zoneId;
        set { _zoneId = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ObservableCollection<DnsRecord> Records { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand ApplyCommand { get; }

    private readonly CloudflareApi _api = new();

    public MainViewModel()
    {
        LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
        ApplyCommand = new RelayCommand(async _ => await ApplyAsync(), _ => !IsBusy);
    }

    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiToken) || string.IsNullOrWhiteSpace(ZoneId))
        {
            MessageBox.Show("API Token と Zone ID を入力してください。");
            return;
        }

        try
        {
            IsBusy = true;
            _api.SetToken(ApiToken);

            Records.Clear();
            var list = await _api.GetDnsRecordsAsync(ZoneId);

            foreach (var r in list)
            {
                Records.Add(r);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"読み込みエラー: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplyAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiToken) || string.IsNullOrWhiteSpace(ZoneId))
        {
            MessageBox.Show("API Token と Zone ID を入力してください。");
            return;
        }

        try
        {
            IsBusy = true;
            _api.SetToken(ApiToken);

            foreach (var r in Records)
            {
                if (!r.IsModified) continue;

                await _api.UpdateDnsRecordAsync(ZoneId, r);
                r.IsModified = false;
            }

            MessageBox.Show("変更を反映しました。");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"反映エラー: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
