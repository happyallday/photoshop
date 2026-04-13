namespace PhotoshopApp.UI.ViewModels;

using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoshopApp.Core.Filters;
using PhotoshopApp.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class FiltersViewModel : ViewModelBase
{
    private readonly FilterManager _filterManager;
    private readonly Action _applyFilterAction;
    
    [ObservableProperty]
    private string _selectedFilterName = "";
    
    [ObservableProperty]
    private BitmapSource? _filterPreview;
    
    [ObservableProperty]
    private bool _isFilterEnabled = true;
    
    [ObservableProperty]
    private string _filterStatus = "Ready";
    
    public ObservableCollection<FilterCategory> FilterCategories { get; } = new();
    
    public ICommand ApplyFilterCommand { get; }
    public ICommand PreviewFilterCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    
    public FiltersViewModel(FilterManager filterManager, Action applyFilterAction)
    {
        _filterManager = filterManager;
        _applyFilterAction = applyFilterAction;
        
        ApplyFilterCommand = new RelayCommand(ExecuteApplyFilter, CanExecuteApplyFilter);
        PreviewFilterCommand = new RelayCommand(ExecutePreviewFilter, CanExecutePreviewFilter);
        ResetFiltersCommand = new RelayCommand(ExecuteResetFilters);
        
        PopulateFilters();
    }
    
    private void PopulateFilters()
    {
        FilterCategories.Clear();
        
        var filtersByCategory = _filterManager.GetFiltersByCategory();
        
        foreach (var category in filtersByCategory)
        {
            var categoryViewModel = new FilterCategory
            {
                Name = category.Key,
                IsExpanded = category.Key == "Blur", // Expand blur category by default
                Filters = new ObservableCollection<FilterItem>()
            };
            
            foreach (var filterName in category.Value)
            {
                var filter = _filterManager.GetFilter(filterName);
                if (filter != null)
                {
                    categoryViewModel.Filters.Add(new FilterItem
                    {
                        Name = filterName,
                        Description = filter.Description,
                        Command = new RelayCommand(() => SelectFilter(filterName))
                    });
                }
            }
            
            FilterCategories.Add(categoryViewModel);
        }
    }
    
    private void SelectFilter(string filterName)
    {
        SelectedFilterName = filterName;
        IsFilterEnabled = true;
        FilterStatus = $"Selected: {filterName}";
        
        // Trigger preview if image is available
        ExecutePreviewFilter();
    }
    
    private void ExecuteApplyFilter()
    {
        if (!string.IsNullOrEmpty(SelectedFilterName))
        {
            FilterStatus = $"Applying: {SelectedFilterName}...";
            _applyFilterAction?.Invoke();
            FilterStatus = $"Applied: {SelectedFilterName}";
        }
    }
    
    private void ExecutePreviewFilter()
    {
        // This would be called when image is available
        FilterStatus = $"Previewing: {SelectedFilterName}";
    }
    
    private void ExecuteResetFilters()
    {
        SelectedFilterName = "";
        FilterPreview = null;
        FilterStatus = "Ready";
    }
    
    private bool CanExecuteApplyFilter()
    {
        return !string.IsNullOrEmpty(SelectedFilterName) && IsFilterEnabled;
    }
    
    private bool CanExecutePreviewFilter()
    {
        return !string.IsNullOrEmpty(SelectedFilterName) && IsFilterEnabled;
    }
    
    public void UpdateFilterPreview(Image<Rgba32> sourceImage)
    {
        if (!string.IsNullOrEmpty(SelectedFilterName) && sourceImage != null)
        {
            var preview = _filterManager.GeneratePreview(SelectedFilterName, sourceImage);
            if (preview != null)
            {
                FilterPreview = BitmapConverter.ToBitmapSource(preview);
            }
        }
    }
    
    public Image<Rgba32>? ApplyCurrentFilter(Image<Rgba32> sourceImage)
    {
        if (string.IsNullOrEmpty(SelectedFilterName)) return null;
        
        return _filterManager.ApplyFilter(SelectedFilterName, sourceImage);
    }
}

public class FilterCategory
{
    public string Name { get; set; } = "";
    public bool IsExpanded { get; set; } = false;
    public ObservableCollection<FilterItem> Filters { get; set; } = new();
}

public class FilterItem
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ICommand Command { get; set; } = null!;
    public bool IsSelected { get; set; }
}