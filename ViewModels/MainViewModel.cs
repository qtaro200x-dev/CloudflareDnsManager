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

    private DnsRecord? _selectedRecord;

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

    public DnsRecord? SelectedRecord
    {
        get => _selectedRecord;
        set { _selectedRecord = value; OnPropertyChanged(); }
    }

    public ObservableCollection<DnsRecord> Records { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }

    private readonly CloudflareApi _api = new();

    public MainViewModel()
    {
        LoadCommand   = new RelayCommand(async _ => await LoadAsync(),   _ => !IsBusy);
        ApplyCommand  = new RelayCommand(async _ => await ApplyAsync(),  _ => !IsBusy);
        AddCommand    = new RelayCommand(async _ => await AddAsync(),    _ => !IsBusy);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy);

    }

    // -------------------------
    // DNS レコード読み込み
    // -------------------------
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

    // -------------------------
    // DNS レコード反映
    // -------------------------
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

    // -------------------------
    // DNS レコード追加
    // -------------------------
    private async Task AddAsync()
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

        var newRecord = new DnsRecord
        {
            Type    = "A",
            Name    = $"test-{Guid.NewGuid().ToString().Substring(0, 8)}.q-taro.org",
            Content = "1.2.3.4",
            Ttl     = 120,
            Proxied = false
        };

            var created = await _api.CreateDnsRecordAsync(ZoneId, newRecord);
            Records.Add(created);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"追加エラー: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -------------------------
    // DNS レコード削除
    // -------------------------
    private async Task DeleteAsync()
    {
        if (SelectedRecord == null)
        {
            MessageBox.Show("削除するレコードを選択してください。");
            return;
        }

        var confirm = MessageBox.Show(
            $"本当に削除しますか？\n{SelectedRecord.Type} {SelectedRecord.Name}",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            IsBusy = true;
            _api.SetToken(ApiToken);

            await _api.DeleteDnsRecordAsync(ZoneId, SelectedRecord.Id);
            Records.Remove(SelectedRecord);
            SelectedRecord = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"削除エラー: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
