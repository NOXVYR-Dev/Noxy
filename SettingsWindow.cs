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
    private readonly Action<int>? applyInstanceLimit;
    private readonly TextBlock updateStatus = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 8) };
    private readonly WpfButton installUpdate = new() { Content = "Jetzt aktualisieren", Height = 34, IsEnabled = false, Padding = new Thickness(12, 0, 12, 0) };
    private readonly WpfButton checkUpdates = new() { Content = "Jetzt prüfen", Height = 34, Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(12, 0, 12, 0) };

    public SettingsWindow(PetSettings settings, UpdateService updateService, Action<int>? applyInstanceLimit = null)
    {
        updates = updateService;
        this.applyInstanceLimit = applyInstanceLimit;
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
        panel.Children.Add(new TextBlock { Text = "Flummi-Wurf: Noxy mit der linken Maustaste schnell ziehen und loslassen. Er fliegt in Zugrichtung und prallt an den Rändern des aktuellen Bildschirms ab. Langsames Ziehen platziert ihn. Im Schlaf- und Ruhemodus bleibt der Wurf aus.", TextWrapping = TextWrapping.Wrap, Opacity = 0.85, Margin = new Thickness(0, 0, 0, 12) });
        var speech = AddCheck(panel, "Freche Sprüche aktiv", settings.EnableSpeech);
        var obs = AddCheck(panel, "Ruhemodus bei OBS", settings.QuietWhenObs);
        var full = AddCheck(panel, "Ruhemodus bei Vollbild", settings.QuietWhenFullscreen);
        var sensors = AddCheck(panel, "PC-Statusmeldungen aktiv", settings.EnableSensors);
        panel.Children.Add(new TextBlock { Text = "Etwa alle fünf Minuten: CPU-, GPU- und RAM-Auslastung. Die Messwerte bleiben auf diesem PC; GPU-Werte hängen vom installierten Windows-Treiber ab.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8, Margin = new Thickness(0, -4, 0, 10) });
        var appComments = AddCheck(panel, "Kommentare bei Programmwechsel", settings.EnableAppComments);
        panel.Children.Add(new TextBlock { Text = "Erkennt nur den Namen des aktiven Programms. Keine Fenstertitel oder Inhalte. Höchstens ein Kommentar alle 3 Minuten.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8, Margin = new Thickness(0, -4, 0, 10) });
        var weatherComments = AddCheck(panel, "Täglicher Wetter-Spruch (ungefähre Stadt per IP)", settings.EnableWeatherComments);
        panel.Children.Add(new TextBlock { Text = "Nur wenn aktiviert und höchstens einmal täglich: Deine öffentliche IP geht an ipwho.is zur ungefähren Stadtbestimmung. Die Koordinaten werden an Open-Meteo für Wetterdaten gesendet. Die Stadt kann ungenau sein. Wetterdaten: Open-Meteo (CC BY 4.0).", TextWrapping = TextWrapping.Wrap, Opacity = 0.8, Margin = new Thickness(0, -4, 0, 10) });
        panel.Children.Add(new TextBlock { Text = "Noxys auf dem Desktop", Foreground = BubbleAccent, FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Wähle bis zu drei Noxys. Die Änderung wird beim Speichern sofort übernommen.", TextWrapping = TextWrapping.Wrap, Opacity = 0.85, Margin = new Thickness(0, 0, 0, 5) });
        var maxInstances = new WpfComboBox { Height = 34, Margin = new Thickness(0, 0, 0, 10), Background = BubbleInput, Foreground = BubbleText, BorderBrush = BubbleBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 3, 6, 3), Template = CreateComboBoxTemplate(), ItemContainerStyle = movement.ItemContainerStyle };
        maxInstances.Items.Add("1 Noxy"); maxInstances.Items.Add("2 Noxys"); maxInstances.Items.Add("3 Noxys");
        maxInstances.SelectedIndex = Math.Clamp(settings.MaxInstances, 1, 3) - 1;
        panel.Children.Add(maxInstances);
        panel.Children.Add(new TextBlock { Text = "Farbe für welchen Noxy?", Foreground = BubbleText, Margin = new Thickness(0, 2, 0, 4) });
        var colorSlot = new WpfComboBox { Height = 34, Margin = new Thickness(0, 0, 0, 8), Background = BubbleInput, Foreground = BubbleText, BorderBrush = BubbleBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 3, 6, 3), Template = CreateComboBoxTemplate(), ItemContainerStyle = movement.ItemContainerStyle };
        colorSlot.Items.Add("Noxy 1"); colorSlot.Items.Add("Noxy 2"); colorSlot.Items.Add("Noxy 3"); colorSlot.SelectedIndex = 0;
        panel.Children.Add(colorSlot);
        var colorPreview = new Border { Height = 24, CornerRadius = new CornerRadius(5), BorderBrush = BubbleText, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(colorPreview);
        var rgbLabel = new TextBlock { Foreground = BubbleText, Margin = new Thickness(0, 0, 0, 4) };
        panel.Children.Add(rgbLabel);
        var red = AddColorSlider(panel, "Rot", BubbleAccent);
        var green = AddColorSlider(panel, "Grün", BubbleAccent);
        var blue = AddColorSlider(panel, "Blau", BubbleAccent);
        void ShowSelectedColor()
        {
            if (settings.NoxyColors.Count < 3) settings.NoxyColors = ["#FFFF425C", "#FF35D8C5", "#FFFFB52E"];
            var value = settings.NoxyColors[Math.Clamp(colorSlot.SelectedIndex, 0, 2)];
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
                red.Value = color.R; green.Value = color.G; blue.Value = color.B;
                colorPreview.Background = new SolidColorBrush(color);
                rgbLabel.Text = $"RGB  {color.R}, {color.G}, {color.B}  ·  #{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch { }
        }
        void UpdateColorPreview()
        {
            var color = System.Windows.Media.Color.FromRgb((byte)red.Value, (byte)green.Value, (byte)blue.Value);
            colorPreview.Background = new SolidColorBrush(color);
            rgbLabel.Text = $"RGB  {color.R}, {color.G}, {color.B}  ·  #{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        colorSlot.SelectionChanged += (_, _) => ShowSelectedColor();
        red.ValueChanged += (_, _) => UpdateColorPreview(); green.ValueChanged += (_, _) => UpdateColorPreview(); blue.ValueChanged += (_, _) => UpdateColorPreview();
        ShowSelectedColor();
        bool? startupState;
        try { startupState = WindowsStartup.IsEnabled(); }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException) { startupState = null; }
        var autostart = AddCheck(panel, "Beim Windows-Anmelden automatisch starten", startupState == true);
        autostart.IsEnabled = startupState.HasValue;
        if (!startupState.HasValue) autostart.ToolTip = "Windows erlaubt hier keinen Zugriff auf den Autostart. Die übrigen Einstellungen bleiben verfügbar.";

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
            settings.MaxInstances = Math.Clamp(maxInstances.SelectedIndex + 1, 1, 3);
            settings.NoxyColors[Math.Clamp(colorSlot.SelectedIndex, 0, 2)] = $"#FF{(byte)red.Value:X2}{(byte)green.Value:X2}{(byte)blue.Value:X2}";
            if (int.TryParse(interval.Text, out var seconds)) settings.IntervalSeconds = Math.Clamp(seconds, 15, 3600);
            try
            {
                if (startupState.HasValue && autostart.IsChecked != startupState)
                {
                    WindowsStartup.SetEnabled(autostart.IsChecked == true);
                    startupState = autostart.IsChecked == true;
                }
                settings.Save();
                updateStatus.Text = "Einstellungen gespeichert.";
                applyInstanceLimit?.Invoke(settings.MaxInstances);
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

    private static System.Windows.Controls.Slider AddColorSlider(System.Windows.Controls.Panel panel, string label, WpfBrush accent)
    {
        panel.Children.Add(new TextBlock { Text = label, Foreground = BubbleText, Margin = new Thickness(0, 0, 0, 1) });
        var slider = new System.Windows.Controls.Slider { Minimum = 0, Maximum = 255, TickFrequency = 1, IsSnapToTickEnabled = true, Height = 24, Margin = new Thickness(0, 0, 0, 4), Foreground = accent };
        panel.Children.Add(slider);
        return slider;
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
