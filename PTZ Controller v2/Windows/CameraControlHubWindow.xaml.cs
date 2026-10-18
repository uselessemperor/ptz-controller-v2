using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PTZ_Controller_v2.Controls;
using PTZ_Controller_v2.Helpers;
using PTZ_Controller_v2.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTZ_Controller_v2;

public sealed partial class CameraControlHubWindow : Window
{
    private readonly AppServices _services;
    private readonly List<CameraControlView> _cameraViews = new();

    private CameraControlView? _activeView;

    public CameraControlHubWindow(AppServices services)
    {
        InitializeComponent();
        WindowTitleBarHelper.ApplyThemeTitleBar(this);

        _services = services;

        _ = LoadCamerasAsync();
    }

    private async Task LoadCamerasAsync()
    {
        try
        {
            CameraTabView.TabItems.Clear();
            _cameraViews.Clear();

            var cameras = await _services.CameraRepository.GetCamerasAsync();

            if (cameras.Count == 0)
            {
                CameraTabView.TabItems.Add(new TabViewItem
                {
                    Header = "No cameras",
                    IsClosable = false,
                    Content = new TextBlock
                    {
                        Text = "No cameras found. Add cameras from the main dashboard first.",
                        Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MutedTextBrush"],
                        FontSize = 18,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(24)
                    }
                });

                return;
            }

            foreach (var camera in cameras)
            {
                var view = new CameraControlView(camera, _services);
                _cameraViews.Add(view);

                var tab = new TabViewItem
                {
                    Header = camera.CameraName,
                    IsClosable = false,
                    Style = (Style)Application.Current.Resources["DarkTabViewItemStyle"],
                    Content = view,
                    Tag = view
                };

                CameraTabView.TabItems.Add(tab);
            }

            CameraTabView.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            CameraTabView.TabItems.Clear();

            CameraTabView.TabItems.Add(new TabViewItem
            {
                Header = "Error",
                IsClosable = false,
                Content = new TextBlock
                {
                    Text = $"Failed to load cameras: {ex.Message}",
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["DangerBrush"],
                    FontSize = 18,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(24)
                }
            });
        }
    }

    private async void CameraTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_activeView != null)
        {
            await _activeView.DeactivateAsync();
        }

        if (CameraTabView.SelectedItem is TabViewItem selectedTab &&
            selectedTab.Tag is CameraControlView selectedView)
        {
            _activeView = selectedView;
            await _activeView.ActivateAsync();
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (_activeView != null)
        {
            await _activeView.DeactivateAsync();
            _activeView = null;
        }

        foreach (var view in _cameraViews)
        {
            await view.DisposeAsync();
        }

        await LoadCamerasAsync();
    }

    private async void Window_Closed(object sender, WindowEventArgs args)
    {
        foreach (var view in _cameraViews)
        {
            await view.DisposeAsync();
        }
    }
}