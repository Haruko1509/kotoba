using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;
using Microsoft.Win32;

namespace KotobaOdyssey.UI.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    private readonly INovelRepository _novelRepository;
    private readonly INovelImportService _novelImportService;
    private readonly INavigationService _navigationService;

    private List<Novel> _allNovels = new();

    public ObservableCollection<Novel> FilteredNovels { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedJlptFilter = "Tất cả JLPT";

    [ObservableProperty]
    private string _selectedSourceFilter = "Tất cả"; // Tất cả, Truyện mẫu (Demo), Truyện của tôi

    [ObservableProperty]
    private bool _favoritesOnly;

    [ObservableProperty]
    private string _selectedSortOption = "Đọc gần đây";

    [ObservableProperty]
    private int _totalNovelsCount;

    [ObservableProperty]
    private int _demoNovelsCount;

    [ObservableProperty]
    private int _userNovelsCount;

    // Modal Import State
    [ObservableProperty]
    private bool _isImportModalOpen;

    [ObservableProperty]
    private string _importType = "JSON"; // "JSON" or "TXT"

    [ObservableProperty]
    private string _importFilePath = string.Empty;

    [ObservableProperty]
    private string _importTitle = string.Empty;

    [ObservableProperty]
    private string _importAuthor = string.Empty;

    [ObservableProperty]
    private JlptLevel _importJlptLevel = JlptLevel.N4;

    [ObservableProperty]
    private string _importGenre = "Fantasy";

    [ObservableProperty]
    private string _importErrorMessage = string.Empty;

    public ObservableCollection<string> ImportValidationErrors { get; } = new();

    public LibraryViewModel(
        INovelRepository novelRepository,
        INovelImportService novelImportService,
        INavigationService navigationService)
    {
        _novelRepository = novelRepository;
        _novelImportService = novelImportService;
        _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
        await LoadNovelsAsync();
    }

    [RelayCommand]
    public async Task LoadNovelsAsync()
    {
        IsBusy = true;
        StatusText = "Đang tải thư viện truyện...";

        try
        {
            var list = await _novelRepository.GetAllAsync();
            _allNovels = list.ToList();

            TotalNovelsCount = _allNovels.Count;
            DemoNovelsCount = _allNovels.Count(n => n.IsDemoData);
            UserNovelsCount = _allNovels.Count(n => !n.IsDemoData);

            ApplyFilters();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilters();
    partial void OnSelectedJlptFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedSourceFilterChanged(string value) => ApplyFilters();
    partial void OnFavoritesOnlyChanged(bool value) => ApplyFilters();
    partial void OnSelectedSortOptionChanged(string value) => ApplyFilters();

    private void ApplyFilters()
    {
        IEnumerable<Novel> query = _allNovels;

        // 1. Search Query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLower();
            query = query.Where(n =>
                n.Title.ToLower().Contains(q) ||
                n.JapaneseTitle.ToLower().Contains(q) ||
                n.Author.ToLower().Contains(q) ||
                n.Genre.ToLower().Contains(q));
        }

        // 2. JLPT Filter
        if (SelectedJlptFilter != "Tất cả JLPT" && !string.IsNullOrWhiteSpace(SelectedJlptFilter))
        {
            query = query.Where(n => n.JlptLevel.ToString() == SelectedJlptFilter);
        }

        // 3. Source Filter
        if (SelectedSourceFilter == "Truyện mẫu (Demo)")
        {
            query = query.Where(n => n.IsDemoData);
        }
        else if (SelectedSourceFilter == "Truyện của tôi")
        {
            query = query.Where(n => !n.IsDemoData);
        }

        // 4. Favorites Only
        if (FavoritesOnly)
        {
            query = query.Where(n => n.IsFavorite);
        }

        // 5. Sorting
        query = SelectedSortOption switch
        {
            "Tên truyện (A-Z)" => query.OrderBy(n => n.Title),
            "Cấp độ JLPT" => query.OrderBy(n => n.JlptLevel),
            "Mới thêm nhất" => query.OrderByDescending(n => n.CreatedAt),
            _ => query.OrderByDescending(n => n.LastReadAt ?? n.CreatedAt)
        };

        FilteredNovels.Clear();
        foreach (var item in query)
        {
            FilteredNovels.Add(item);
        }
    }

    [RelayCommand]
    public async Task ToggleFavorite(Novel? novel)
    {
        if (novel == null) return;
        novel.IsFavorite = !novel.IsFavorite;
        await _novelRepository.UpdateAsync(novel);
        ApplyFilters();
    }

    [RelayCommand]
    public void OpenNovel(Novel? novel)
    {
        if (novel != null)
        {
            _navigationService.NavigateTo("Reader", novel);
        }
    }

    [RelayCommand]
    public async Task DeleteNovel(Novel? novel)
    {
        if (novel == null) return;

        // Delete from database
        await _novelRepository.DeleteAsync(novel.Id);
        await LoadNovelsAsync();
    }

    // Modal Import Actions
    [RelayCommand]
    public void OpenImportModal(string type = "JSON")
    {
        ImportType = type;
        ImportFilePath = string.Empty;
        ImportTitle = string.Empty;
        ImportAuthor = string.Empty;
        ImportGenre = "General";
        ImportJlptLevel = JlptLevel.N4;
        ImportErrorMessage = string.Empty;
        ImportValidationErrors.Clear();
        IsImportModalOpen = true;
    }

    [RelayCommand]
    public void CloseImportModal()
    {
        IsImportModalOpen = false;
        ImportErrorMessage = string.Empty;
        ImportValidationErrors.Clear();
    }

    [RelayCommand]
    public void BrowseFile()
    {
        var dialog = new OpenFileDialog();
        if (ImportType == "JSON")
        {
            dialog.Filter = "Tập tin JSON (*.json)|*.json|Tất cả tập tin (*.*)|*.*";
            dialog.Title = "Chọn file tiểu thuyết JSON để nhập";
        }
        else
        {
            dialog.Filter = "Tập tin văn bản (*.txt)|*.txt|Tất cả tập tin (*.*)|*.*";
            dialog.Title = "Chọn file văn bản TXT để nhập";
        }

        if (dialog.ShowDialog() == true)
        {
            ImportFilePath = dialog.FileName;
            if (string.IsNullOrWhiteSpace(ImportTitle))
            {
                ImportTitle = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }

    [RelayCommand]
    public async Task ConfirmImport()
    {
        ImportErrorMessage = string.Empty;
        ImportValidationErrors.Clear();

        if (string.IsNullOrWhiteSpace(ImportFilePath) || !File.Exists(ImportFilePath))
        {
            ImportErrorMessage = "Vui lòng chọn tập tin hợp lệ trước khi nhập.";
            return;
        }

        IsBusy = true;
        StatusText = "Đang xử lý nhập khẩu dữ liệu tiểu thuyết...";

        try
        {
            string content = await File.ReadAllTextAsync(ImportFilePath);

            var result = ImportType == "JSON"
                ? await _novelImportService.ImportFromJsonAsync(content)
                : await _novelImportService.ImportFromTxtAsync(content, ImportTitle, ImportAuthor, ImportJlptLevel, ImportGenre);

            if (result.IsSuccess)
            {
                CloseImportModal();
                await LoadNovelsAsync();

                // Automatically navigate to read the newly imported novel!
                if (result.ImportedNovel != null)
                {
                    _navigationService.NavigateTo("Reader", result.ImportedNovel);
                }
            }
            else
            {
                ImportErrorMessage = result.ErrorMessage ?? "Nhập file thất bại.";
                foreach (var err in result.ValidationErrors)
                {
                    ImportValidationErrors.Add(err);
                }
            }
        }
        catch (Exception ex)
        {
            ImportErrorMessage = $"Đã xảy ra lỗi khi đọc tập tin: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
