using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfBrush = System.Windows.Media.Brush;

namespace Noxy;

public class SettingsWindow : Window
{
    private static readonly WpfBrush BubbleBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(23, 18, 27));
    private static readonly WpfBrush BubbleSurface = new SolidColorBrush(System.Windows.Media.Color.FromRgb(36, 33, 45));
    private static readonly WpfBrush BubbleInput = new SolidColorBrush(System.Windows.Media.Color.FromRgb(17, 19, 27));
    private static readonly WpfBrush BubbleBorder = new SolidColorBrush(System.Windows.Media.Color.FromRgb(184, 50, 67));
    private static readonly WpfBrush BubbleText = new SolidColorBrush(System.Windows.Media.Color.FromRgb(246, 233, 233));
    private static readonly WpfBrush BubbleAccent = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 103, 122));
    private readonly UpdateService updates;
    private readonly TextBlock updateStatus = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
    private readonly WpfButton installUpdate = new() { Content = "Jetzt aktualisieren", Height = 34, IsEnabled = false, Padding = new Thickness(12, 0, 12, 0) };
    private readonly WpfButton checkUpdates = new() { Content = "Jetzt prüfen", Height = 34, Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(12, 0, 12, 0) };

    public SettingsWindow(PetSettings settings, UpdateService updateService)
    {
        updates = updateService;
        Title = "Noxy | Einstellungen";
        Width = 470;
        Height = 730;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Foreground = BubbleText;

        var frame = new Border { Background = BubbleBackground, BorderBrush = BubbleBorder, BorderThickness = new Thickness(1.5), CornerRadius = new CornerRadius(15), Padding = new Thickness(12) };
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        frame.Child = layout;
        Content = frame;

        var titleBar = new Grid { Margin = new Thickness(3, 2, 3, 8), Cursor = System.Windows.Input.Cursors.SizeAll };
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var titleStack = new StackPanel { Orientation = WpfOrientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        titleStack.Children.Add(new TextBlock { Text = "NOXY", Foreground = BubbleAccent, FontWeight = FontWeights.Black, FontSize = 17, Margin = new Thickness(0, 0, 10, 0) });
        titleStack.Children.Add(new TextBlock { Text = $"Einstellungen  ·  {updates.CurrentVersion}", Foreground = BubbleText, FontSize = 15, VerticalAlignment = VerticalAlignment.Center });
        titleBar.Children.Add(titleStack);
        var close = new WpfButton { Content = "×", Width = 32, Height = 30, FontSize = 18, Padding = new Thickness(0), Margin = new Thickness(8, 0, 0, 0), ToolTip = "Schließen" };
        StyleButton(close);
        Grid.SetColumn(close, 1);
        titleBar.Children.Add(close);
        close.Click += (_, _) => Close();
        titleBar.MouseLeftButtonDown += (_, e) => { if (e.OriginalSource is not WpfButton && e.ButtonState == MouseButtonState.Pressed) DragMove(); };
        layout.Children.Add(titleBar);

        var panel = new StackPanel { Margin = new Thickness(8, 4, 8, 12) };
        var scroll = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetRow(scroll, 1);
        layout.Children.Add(scroll);
        panel.Children.Add(new TextBlock { Text = "Verhalten", Foreground = BubbleAccent, FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
        var movement = new WpfComboBox { Height = 34, Margin = new Thickness(0, 0, 0, 12), Background = BubbleInput, Foreground = BubbleText, BorderBrush = BubbleBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 3, 6, 3), Template = CreateComboBoxTemplate() };
        var movementItemStyle = new Style(typeof(ComboBoxItem));
        movementItemStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, BubbleSurface));
        movementItemStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, BubbleText));
        movementItemStyle.Setters.Add(new Setter(System.Windows.Controls.Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        var movementHover = new Trigger { Property = ComboBoxItem.IsHighlightedProperty, Value = true };
        movementHover.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, BubbleBorder));
        movementHover.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, System.Windows.Media.Brushes.White));
        movementItemStyle.Triggers.Add(movementHover);
        movement.ItemContainerStyle = movementItemStyle;
        movement.Items.Add("Herumlaufen");
        movement.Items.Add("Zappelig");
        movement.Items.Add("Statisch");
        movement.Items.Add("Schlafmodus");
        movement.SelectedIndex = settings.MovementMode switch { "zippy" => 1, "static" => 2, "sleep" => 3, _ => 0 };
        panel.Children.Add(movement);
        var speech = AddCheck(panel, "Freche Sprüche aktiv", settings.EnableSpeech);
        var obs = AddCheck(panel, "Ruhemodus bei OBS", settings.QuietWhenObs);
        var full = AddCheck(panel, "Ruhemodus bei Vollbild", settings.QuietWhenFullscreen);
        var sensors = AddCheck(panel, "RAM-Warnungen aktiv", settings.EnableSensors);
        var appComments = AddCheck(panel, "Kommentare bei Programmwechsel", settings.EnableAppComments);
        panel.Children.Add(new TextBlock { Text = "Erkennt nur den Namen des aktiven Programms. Keine Fenstertitel oder Inhalte. Höchstens ein Kommentar alle 3 Minuten.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8, Margin = new Thickness(0, -4, 0, 10) });
        var weatherComments = AddCheck(panel, "Täglicher Wetter-Spruch (ungefährer Standort über IP)", settings.EnableWeatherComments);
        panel.Children.Add(new TextBlock { Text = "Noxy ermittelt Stadt und Wetter einmal am Tag über das Internet. Der Standort ist IP-basiert und kann ungenau sein. Wetterdaten: Open-Meteo (CC BY 4.0).", TextWrapping = TextWrapping.Wrap, Opacity = 0.8, Margin = new Thickness(0, -4, 0, 10) });
        var autostart = AddCheck(panel, "Beim Windows-Anmelden automatisch starten", WindowsStartup.IsEnabled());

        panel.Children.Add(new TextBlock { Text = "Sprüche alle (Sekunden, mindestens 15):", Foreground = BubbleText, Margin = new Thickness(0, 6, 0, 0) });
        var interval = new WpfTextBox { Text = settings.IntervalSeconds.ToString(), Margin = new Thickness(0, 5, 0, 12), Padding = new Thickness(7, 5, 7, 5), Background = BubbleInput, Foreground = BubbleText, BorderBrush = BubbleBorder };
        panel.Children.Add(interval);

        var save = new WpfButton { Content = "Einstellungen speichern", Height = 36, Margin = new Thickness(0, 2, 0, 0), Padding = new Thickness(12, 0, 12, 0), Background = BubbleBorder, Foreground = System.Windows.Media.Brushes.White };
        panel.Children.Add(save);
        save.Click += (_, _) =>
        {
            settings.MovementMode = movement.SelectedIndex switch { 1 => "zippy", 2 => "static", 3 => "sleep", _ => "walk" };
            settings.EnableWalking = settings.MovementMode is "walk" or "zippy";
            settings.EnableSpeech = speech.IsChecked == true;
            settings.QuietWhenObs = obs.IsChecked == true;
            settings.QuietWhenFullscreen = full.IsChecked == true;
            settings.EnableSensors = sensors.IsChecked == true;
            settings.EnableAppComments = appComments.IsChecked == true;
            settings.EnableWeatherComments = weatherComments.IsChecked == true;
            if (int.TryParse(interval.Text, out var seconds)) settings.IntervalSeconds = Math.Clamp(seconds, 15, 3600);
            try
            {
                WindowsStartup.SetEnabled(autostart.IsChecked == true);
                settings.Save();
                updateStatus.Text = "Einstellungen gespeichert.";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this, $"Die Einstellungen konnten nicht gespeichert werden.\n\n{ex.Message}", "Noxy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        panel.Children.Add(new Separator { Margin = new Thickness(0, 18, 0, 12), Background = BubbleBorder });
        panel.Children.Add(new TextBlock { Text = "Updates von GitHub", FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = BubbleAccent });
        panel.Children.Add(updateStatus);
        panel.Children.Add(new TextBlock { Text = "Änderungen der neuesten Version", Foreground = BubbleText, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 6) });
        var changelog = new WpfTextBox
        {
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MinHeight = 74,
            MaxHeight = 130,
            Background = BubbleInput,
            Foreground = BubbleText,
            BorderBrush = BubbleBorder,
            Padding = new Thickness(8),
        };
        panel.Children.Add(changelog);
        var updateButtons = new StackPanel { Orientation = WpfOrientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        StyleButton(installUpdate);
        StyleButton(checkUpdates);
        updateButtons.Children.Add(installUpdate);
        updateButtons.Children.Add(checkUpdates);
        panel.Children.Add(updateButtons);

        updates.Changed += RefreshUpdateState;
        checkUpdates.Click += async (_, _) => await updates.CheckAsync();
        installUpdate.Click += async (_, _) => await updates.InstallUpdateAsync();
        void RefreshChangelog() => changelog.Text = updates.ChangeLog;
        updates.Changed += RefreshChangelog;
        Closed += (_, _) =>
        {
            updates.Changed -= RefreshUpdateState;
            updates.Changed -= RefreshChangelog;
        };
        RefreshUpdateState();
        RefreshChangelog();
    }

    private static void StyleButton(WpfButton button)
    {
        button.Background = BubbleSurface;
        button.Foreground = BubbleText;
        button.BorderBrush = BubbleBorder;
        button.BorderThickness = new Thickness(1);
        var template = new ControlTemplate(typeof(WpfButton));
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "ButtonBorder";
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(ContentPresenter.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        content.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
        border.AppendChild(content);
        template.VisualTree = border;
        var disabled = new Trigger { Property = System.Windows.Controls.Control.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 43, 53)), "ButtonBorder"));
        disabled.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 180, 190))));
        template.Triggers.Add(disabled);
        var hover = new Trigger { Property = System.Windows.Controls.Control.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(System.Windows.Media.Color.FromRgb(57, 48, 61)), "ButtonBorder"));
        template.Triggers.Add(hover);
        button.Template = template;
    }

    private static ControlTemplate CreateComboBoxTemplate()
    {
        const string xaml = @"
<ControlTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                 xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                 TargetType=""{x:Type ComboBox}"">
  <Grid>
    <Border x:Name=""ComboBorder"" Background=""{TemplateBinding Background}""
            BorderBrush=""{TemplateBinding BorderBrush}"" BorderThickness=""{TemplateBinding BorderThickness}"" CornerRadius=""5""></Border>
    <DockPanel LastChildFill=""True"" Margin=""1""><ToggleButton x:Name=""DropDownToggle"" DockPanel.Dock=""Right"" Width=""30"" Focusable=""False""
        Background=""Transparent"" Foreground=""#FFF6E9E9"" BorderThickness=""0"" Content=""⌄""
        IsChecked=""{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}""/>
      <ContentPresenter Margin=""7,0,4,0"" VerticalAlignment=""Center"" HorizontalAlignment=""Left""
        TextElement.Foreground=""{TemplateBinding Foreground}"" Content=""{TemplateBinding SelectionBoxItem}""
        ContentTemplate=""{TemplateBinding SelectionBoxItemTemplate}"" ContentStringFormat=""{TemplateBinding SelectionBoxItemStringFormat}""/>
    </DockPanel>
    <Popup x:Name=""Popup"" Placement=""Bottom"" PlacementTarget=""{Binding RelativeSource={RelativeSource TemplatedParent}}""
           AllowsTransparency=""True"" Focusable=""False"" IsOpen=""{TemplateBinding IsDropDownOpen}"">
      <Border Background=""#FF24212D"" BorderBrush=""#FFB83243"" BorderThickness=""1"" CornerRadius=""5"">
        <ScrollViewer CanContentScroll=""True"" MaxHeight=""220""><ItemsPresenter/></ScrollViewer>
      </Border>
    </Popup>
  </Grid>
  <ControlTemplate.Triggers>
    <Trigger Property=""IsDropDownOpen"" Value=""True""><Setter TargetName=""ComboBorder"" Property=""BorderBrush"" Value=""#FFFF677A""/></Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>";
        return (ControlTemplate)XamlReader.Parse(xaml);
    }

    private static WpfCheckBox AddCheck(System.Windows.Controls.Panel panel, string label, bool enabled)
    {
        var check = new WpfCheckBox
        {
            Content = label,
            Foreground = BubbleText,
            IsChecked = enabled,
            Margin = new Thickness(0, 0, 0, 10),
        };
        panel.Children.Add(check);
        return check;
    }

    private void RefreshUpdateState()
    {
        updateStatus.Text = updates.Status;
        installUpdate.IsEnabled = updates.UpdateAvailable && !updates.IsBusy;
        installUpdate.Background = updates.UpdateAvailable ? System.Windows.Media.Brushes.ForestGreen : BubbleSurface;
        installUpdate.Foreground = BubbleText;
        checkUpdates.IsEnabled = !updates.IsBusy;
    }
}

