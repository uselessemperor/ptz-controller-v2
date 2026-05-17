using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace PTZ_Controller_v2.Helpers;

public static class WindowTitleBarHelper
{
    public static void ApplyThemeTitleBar(Window window)
    {
        var resources = Application.Current.Resources;

        var background = GetColor(resources, "WindowTitleBarBackgroundColor");
        var foreground = GetColor(resources, "WindowTitleBarForegroundColor");
        var inactiveForeground = GetColor(resources, "WindowTitleBarInactiveForegroundColor");
        var hover = GetColor(resources, "WindowTitleBarButtonHoverColor");
        var pressed = GetColor(resources, "WindowTitleBarButtonPressedColor");
        var pressedForeground = GetColor(resources, "WindowTitleBarButtonPressedForegroundColor");

        var titleBar = window.AppWindow.TitleBar;

        titleBar.BackgroundColor = background;
        titleBar.ForegroundColor = foreground;

        titleBar.InactiveBackgroundColor = background;
        titleBar.InactiveForegroundColor = inactiveForeground;

        titleBar.ButtonBackgroundColor = background;
        titleBar.ButtonForegroundColor = foreground;

        titleBar.ButtonHoverBackgroundColor = hover;
        titleBar.ButtonHoverForegroundColor = foreground;

        titleBar.ButtonPressedBackgroundColor = pressed;
        titleBar.ButtonPressedForegroundColor = pressedForeground;

        titleBar.ButtonInactiveBackgroundColor = background;
        titleBar.ButtonInactiveForegroundColor = inactiveForeground;
    }

    private static Color GetColor(ResourceDictionary resources, string key)
    {
        if (resources.TryGetValue(key, out var value) && value is Color color)
        {
            return color;
        }

        return Colors.Black;
    }
}