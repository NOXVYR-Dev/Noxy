using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Noxy;

static class Program
{
    static int passed;
    static readonly List<string> results = new();
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly Rect Bounds = new(0, 0, 1000, 600);

    static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        passed++; results.Add("PASS: " + name); Console.WriteLine(results[^1]);
    }
    static Point Run(PetMotion m, Point p, Rect b, double seconds, string mode = "walk", double dt = 1.0 / 60)
    {
        for (int i = 0; i < (int)Math.Round(seconds / dt); i++) p = m.Advance(p, b, dt, mode, false);
        return p;
    }
    static T Field<T>(object o, string name) => (T)o.GetType().GetField(name, Private)!.GetValue(o)!;
    static void Frame(MainWindow w, double dt = 1.0 / 30) => typeof(MainWindow).GetMethod("AdvanceAnimation", Private)!.Invoke(w, new object[] { dt });
    static void Pump(double seconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start(); Dispatcher.PushFrame(frame);
    }
    static RenderTargetBitmap Snapshot(Window w)
    {
        w.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)w.Width, (int)w.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render((Visual)w.Content); return bitmap;
    }
    static void SavePreview(string path, IReadOnlyList<BitmapSource> frames)
    {
        var visual = new DrawingVisual();
        using (var d = visual.RenderOpen())
        {
            d.DrawRectangle(new SolidColorBrush(Color.FromRgb(37, 43, 56)), null, new Rect(0, 0, 810, 345));
            var labels = new[] { "Laufen", "Flummi-Wurf", "Schlafmodus" };
            for (int i = 0; i < frames.Count; i++)
            {
                d.DrawImage(frames[i], new Rect(i * 270, 30, 270, 300));
                d.DrawText(new FormattedText(labels[i], System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 18, Brushes.White, 1), new Point(i * 270 + 20, 12));
            }
        }
        var image = new RenderTargetBitmap(810, 345, 96, 96, PixelFormats.Pbgra32); image.Render(visual);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = File.Create(path); encoder.Save(stream);
    }

    [STAThread]
    static int Main(string[] args)
    {
        string output = Path.GetFullPath(args.FirstOrDefault() ?? "outputs"); Directory.CreateDirectory(output);
        try
        {
            var m = new PetMotion(new Random(17));
            var p = Run(m, new Point(100, 100), Bounds, 2);
            Check(p.X > 180 && p.Y == 100, "Autonomous walking starts without input");
            var fast = Run(new PetMotion(new Random(17)), new Point(100, 100), Bounds, 2, "zippy");
            Check(fast.X > p.X * 1.4, "Zippy mode runs visibly faster");
            var p30 = Run(new PetMotion(new Random(17)), new Point(100, 100), Bounds, 2, dt: 1.0 / 30);
            Check(Math.Abs(p30.X - p.X) < 3, "Walking speed remains stable at 30 and 60 Hz");
            foreach (var mode in new[] { "static", "sleep" })
                Check(Run(new PetMotion(), new Point(100, 100), Bounds, 2, mode) == new Point(100, 100), mode + " prevents autonomous motion");
            m = new PetMotion(); p = new Point(995, 100);
            bool edge = false, returned = false;
            for (int i = 0; i < 120; i++)
            {
                p = m.Advance(p, Bounds, 1.0 / 60, "walk", false);
                edge |= p.X >= 999; returned |= edge && p.X < 990;
            }
            Check(edge && returned, "Walking reaches the full legal right edge and turns back");
            var negative = new Rect(-1500, -500, 1000, 700);
            p = Run(new PetMotion(), new Point(-501, -100), negative, 2);
            Check(negative.Contains(p), "Monitor with negative desktop coordinates stays bounded");
            m = new PetMotion(); m.Throw(new Vector(1200, -400));
            p = m.Advance(new Point(998, 200), Bounds, .05, "walk", false);
            Check(m.VelocityX < 0 && Bounds.Contains(p), "Right-wall throw reverses horizontal direction");
            Check(m.Impact > 0, "Collision provides visible squash feedback");
            m = new PetMotion(); m.Throw(new Vector(-1200, -400));
            p = m.Advance(new Point(2, 200), Bounds, .05, "walk", false);
            Check(m.VelocityX > 0 && Bounds.Contains(p), "Left-wall throw reverses horizontal direction");
            m = new PetMotion(); m.Throw(new Vector(300, -1400));
            p = m.Advance(new Point(400, 2), Bounds, .02, "walk", false);
            Check(m.VelocityY > 0 && Bounds.Contains(p), "Ceiling throw bounces downward");
            m = new PetMotion(); m.Throw(new Vector(300, 1400));
            p = m.Advance(new Point(400, 598), Bounds, .02, "walk", false);
            Check(m.VelocityY < 0 && Bounds.Contains(p), "Floor throw bounces upward");
            Check(Math.Abs(m.VelocityY) < 1400, "Bounce loses energy");
            m = new PetMotion(); Check(!m.Throw(new Vector(100, 20)), "Slow dragging does not fling");
            Check(!m.Throw(new Vector(double.NaN, 20)), "Invalid velocity cannot enter physics");
            m.Throw(new Vector(9000, -9000));
            Check(new Vector(m.VelocityX, m.VelocityY).Length <= 1800.001, "Extreme throws have a bounded speed");
            m.Advance(new Point(100, 100), Bounds, .03, "walk", true);
            Check(!m.Flying, "Quiet mode interrupts flight immediately");
            m.Throw(new Vector(700, -500)); m.Advance(new Point(100, 100), Bounds, .03, "sleep", false);
            Check(!m.Flying, "Sleep mode interrupts flight");
            m.Throw(new Vector(700, -500)); m.Stop();
            Check(!m.Flying && m.VelocityX == 0 && m.VelocityY == 0, "Grabbing a flying pet stops its momentum");
            m.Throw(new Vector(700, -500));
            Check(m.Advance(new Point(100, 100), Bounds, 1.0 / 60, "static", false).X > 100, "Static mode still permits a deliberate user throw");
            var rng = new Random(42); bool stable = true, settled = true;
            for (int trial = 0; trial < 250; trial++)
            {
                var b = trial % 2 == 0 ? Bounds : negative;
                m = new PetMotion(new Random(trial));
                p = new Point(b.Left + rng.NextDouble() * b.Width, b.Top + rng.NextDouble() * b.Height);
                m.Throw(new Vector(rng.Next(-4000, 4001), rng.Next(-4000, 4001)));
                for (int i = 0; i < 360; i++)
                {
                    p = m.Advance(p, b, 1.0 / 60, "static", false);
                    stable &= double.IsFinite(p.X) && double.IsFinite(p.Y) && b.Contains(p);
                }
                settled &= !m.Flying;
            }
            Check(stable, "250 randomized throws stay inside monitor bounds, including corners");
            Check(settled, "All 250 randomized throws settle within six seconds");
            m = new PetMotion(); m.Throw(new Vector(500, -300));
            p = Run(m, new Point(0, 0), new Rect(0, 0, 0, 0), 6, "static");
            Check(p == new Point(0, 0) && !m.Flying, "Tiny display with zero available travel remains stable");
            m = new PetMotion(); m.Throw(new Vector(700, -300));
            p = m.Advance(new Point(500, 200), Bounds, 20, "walk", false);
            Check((p - new Point(500, 200)).Length < 190, "Long frame stall cannot teleport the pet");
            var drag = new DragVelocity(); drag.Begin(new Point(100, 100), 0);
            drag.Add(new Point(140, 80), .04); drag.Add(new Point(200, 50), .10);
            var velocity = drag.Release(.11);
            Check(velocity.X > 900 && velocity.Y < -400, "Drag samples retain release speed and pull direction");
            Check(drag.Release(.3).Length == 0, "Holding before release cancels a stale throw");
            drag.Begin(new Point(0, 0), 1); drag.Add(new Point(0, 100), 1.08);
            Check(Math.Abs(drag.Release(1.09).X) < .001 && drag.Release(1.09).Y > 1000, "Vertical throws retain their direction");
            drag.Begin(new Point(0, 0), 2); drag.Add(new Point(1, 1), 2.08);
            Check(!new PetMotion().Throw(drag.Release(2.09)), "Slow sample sequence remains a placement");

            var shake = new PetShakeDetector(); shake.Begin(new Point(100, 100), 0);
            Check(!shake.Add(new Point(140, 100), .08) && !shake.Add(new Point(110, 100), .16) && shake.Add(new Point(140, 100), .24),
                "Two quick direction reversals register a deliberate shake");
            shake.Begin(new Point(0, 0), 1);
            Check(!shake.Add(new Point(25, 0), 1.05) && !shake.Add(new Point(15, 0), 1.1) && !shake.Add(new Point(25, 0), 1.15),
                "Small cursor jitter does not register as a shake");

            // Run the actual compiled WPF window and its dispatcher, without touching user settings.
            var app = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            var settings = new PetSettings { QuietWhenObs = false, QuietWhenFullscreen = false, EnableSpeech = false, EnableWeatherComments = false };
            var window = new MainWindow(settings, 1);
            var closeHandler = (CancelEventHandler)Delegate.CreateDelegate(typeof(CancelEventHandler), window,
                typeof(MainWindow).GetMethod("OnClosing", Private | BindingFlags.DeclaredOnly)!);
            window.Closing -= closeHandler; // Tests must not replace the user's saved desktop position.
            window.Show(); Pump(.1);
            var area = (Rect)typeof(MainWindow).GetMethod("CurrentMonitorWorkArea", Private)!.Invoke(window, null)!;
            window.Left = area.Left + Math.Min(50, area.Width / 4); window.Top = area.Top + Math.Min(50, area.Height / 4);
            double initial = window.Left; Pump(1.4);
            Check(Math.Abs(window.Left - initial) > 30, "Actual Windows pet moves on the live dispatcher");
            Field<DispatcherTimer>(window, "movementTimer").Stop(); Field<DispatcherTimer>(window, "timer").Stop();
            var liveDrag = Field<DragVelocity>(window, "dragVelocity");
            var clock = Field<System.Diagnostics.Stopwatch>(window, "animationClock");
            liveDrag.Begin(new Point(window.Left, window.Top), clock.Elapsed.TotalSeconds);
            typeof(MainWindow).GetField("dragging", Private)!.SetValue(window, true);
            System.Threading.Thread.Sleep(35); window.Left += 45;
            typeof(MainWindow).GetMethod("EndDrag", Private)!.Invoke(window, new object[] { true });
            Check(Field<PetMotion>(window, "motion").Flying && Field<PetMotion>(window, "motion").VelocityX > 260,
                "Actual window LocationChanged samples feed the release-to-flight path");
            Field<PetMotion>(window, "motion").Stop();
            var shakeDetector = Field<PetShakeDetector>(window, "shakeDetector");
            var shakeStart = clock.Elapsed.TotalSeconds; var shakePosition = new Point(window.Left, window.Top);
            shakeDetector.Begin(shakePosition, shakeStart); liveDrag.Begin(shakePosition, shakeStart);
            typeof(MainWindow).GetField("shakeTriggered", Private)!.SetValue(window, false);
            typeof(MainWindow).GetField("dragging", Private)!.SetValue(window, true);
            System.Threading.Thread.Sleep(40); window.Left += 40;
            System.Threading.Thread.Sleep(40); window.Left -= 30;
            System.Threading.Thread.Sleep(40); window.Left += 30;
            typeof(MainWindow).GetMethod("EndDrag", Private)!.Invoke(window, new object[] { true });
            Check(!Field<PetMotion>(window, "motion").Flying && Field<double>(window, "angerRemaining") > 0,
                "A shaken pet gets angry instead of being flung on release");
            var firstShakePhrase = ((TextBlock)window.FindName("BubbleText")).Text;
            typeof(MainWindow).GetMethod("BecomeAngry", Private)!.Invoke(window, null);
            var secondShakePhrase = ((TextBlock)window.FindName("BubbleText")).Text;
            Check(firstShakePhrase != secondShakePhrase && firstShakePhrase.Length > 0 && secondShakePhrase.Length > 0,
                "Shake reactions use varied phrases without repeating the last one");
            Frame(window);
            Check(((System.Windows.Shapes.Path)window.FindName("LeftAngryBrow")).Visibility == Visibility.Visible && ((System.Windows.Shapes.Path)window.FindName("RightAngryBrow")).Visibility == Visibility.Visible,
                "The real WPF pet shows its angry eyebrows after a shake");
            Field<PetMotion>(window, "motion").Stop();
            for (int i = 0; i < 20; i++) Frame(window);
            var body = (Canvas)window.FindName("PetBody");
            var eye = (System.Windows.Shapes.Ellipse)window.FindName("LeftEye");
            Check(body.Children.Contains(eye) && body.Children.Contains((UIElement)window.FindName("Body")), "Body and facial features share the animated group");
            var animation = (TransformGroup)body.RenderTransform;
            double before = ((TranslateTransform)animation.Children[2]).Y; Frame(window, .07);
            Check(((TranslateTransform)animation.Children[2]).Y != before, "WPF character visibly animates between frames");
            var movingMotion = Field<PetMotion>(window, "motion");
            var leftEar = (System.Windows.Shapes.Path)window.FindName("LeftEar");
            var rightEar = (System.Windows.Shapes.Path)window.FindName("RightEar");
            for (int i = 0; i < 20 && !movingMotion.Moving; i++) Frame(window, .04);
            double leftEarBefore = ((RotateTransform)leftEar.RenderTransform).Angle;
            double rightEarBefore = ((RotateTransform)rightEar.RenderTransform).Angle;
            Frame(window, .07);
            Check(movingMotion.Moving && (Math.Abs(((RotateTransform)leftEar.RenderTransform).Angle - leftEarBefore) > .1 ||
                Math.Abs(((RotateTransform)rightEar.RenderTransform).Angle - rightEarBefore) > .1),
                "Both ears wobble with the actual walking animation");
            var images = new List<BitmapSource> { Snapshot(window) };
            var liveMotion = Field<PetMotion>(window, "motion"); liveMotion.Throw(new Vector(900, -500)); Frame(window, .07);
            Check(liveMotion.Flying && Math.Abs(((RotateTransform)animation.Children[1]).Angle) > 1, "WPF flight tilts the character visibly");
            images.Add(Snapshot(window));
            settings.MovementMode = "sleep"; Frame(window);
            Check(!liveMotion.Flying && eye.Height == 3 && ((TextBlock)window.FindName("SleepLabel")).IsVisible, "Sleep closes the eyes, shows Zs, and stops flight");
            images.Add(Snapshot(window)); SavePreview(Path.Combine(output, "Noxy-Alpha-Animation.png"), images);
            settings.MovementMode = "static"; Frame(window); var stationary = new Point(window.Left, window.Top);
            for (int i = 0; i < 60; i++) Frame(window);
            Check(new Point(window.Left, window.Top) == stationary, "Actual static window stays at its placed location");
            settings.MovementMode = "walk"; window.ContextMenu.IsOpen = true; Frame(window); stationary = new Point(window.Left, window.Top);
            for (int i = 0; i < 20; i++) Frame(window);
            Check(new Point(window.Left, window.Top) == stationary, "Context menu pauses the pet for interaction");
            window.ContextMenu.IsOpen = false;
            var options = new SettingsWindow(settings, Field<UpdateService>(window, "updates")) { Owner = window };
            options.Show(); Pump(.05); Check(options.IsVisible, "Existing settings window opens successfully"); options.Close();
            Check(!window.ContextMenu.Items.OfType<MenuItem>().Any(item => Equals(item.Header, "KI-Chat")),
                "The removed AI chat is no longer available from the pet menu");
            Check(SystemMonitor.RamPercent() is >= 0 and <= 100, "Existing RAM sensor returns a plausible Windows reading");
            _ = SystemMonitor.CpuPercent(); System.Threading.Thread.Sleep(150);
            Check(SystemMonitor.CpuPercent() is >= 0 and <= 100, "Windows CPU sensor returns a plausible load percentage");
            _ = SystemMonitor.GpuPercent(); System.Threading.Thread.Sleep(150);
            var gpu = SystemMonitor.GpuPercent();
            Check(gpu is null or >= 0 and <= 100, "GPU sensor returns a percentage or safely reports unavailable hardware");
            typeof(MainWindow).GetField("cpuPercent", Private)!.SetValue(window, 31);
            typeof(MainWindow).GetField("gpuPercent", Private)!.SetValue(window, 47);
            var status = (string)typeof(MainWindow).GetMethod("SystemUsageComment", Private)!.Invoke(window, null)!;
            Check(status.Contains("CPU 31 %") && status.Contains("GPU-Spitze 47 %") && status.Contains("RAM "), "PC status message combines CPU, GPU, and RAM readings");
            window.Close(); app.Shutdown();
            results.Add($"\n{passed} checks passed. Windows WPF integration included. No live weather API calls or user settings writes.");
            File.WriteAllLines(Path.Combine(output, "Noxy-Alpha-Testprotokoll.txt"), results);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex); results.Add(ex.ToString());
            File.WriteAllLines(Path.Combine(output, "Noxy-Alpha-Testprotokoll.txt"), results); return 1;
        }
    }
}

