using System.Windows;
using Point = System.Windows.Point;

namespace Noxy;

// All positions and speeds are in WPF device-independent pixels.
public sealed class PetMotion
{
    readonly Random random;
    double pause = .35, walkRemaining = 2.8, flightTime;
    int direction = 1;
    public bool Flying { get; private set; }
    public bool Moving { get; private set; }
    public double VelocityX { get; private set; }
    public double VelocityY { get; private set; }
    public int Direction => direction;
    public double Impact { get; private set; }

    public PetMotion(Random? random = null) => this.random = random ?? new Random();

    public void Stop()
    {
        Flying = Moving = false;
        VelocityX = VelocityY = 0;
        pause = .4;
    }

    public bool Throw(Vector velocity)
    {
        Stop();
        if (!double.IsFinite(velocity.X) || !double.IsFinite(velocity.Y) || velocity.Length < 260) return false;
        if (velocity.Length > 1800) velocity *= 1800 / velocity.Length;
        VelocityX = velocity.X;
        VelocityY = velocity.Y;
        if (Math.Abs(velocity.X) > 1) direction = Math.Sign(velocity.X);
        flightTime = 0;
        Flying = true;
        return true;
    }

    // Bounds describe legal WINDOW origins, so the window size must not be subtracted again.
    public Point Advance(Point position, Rect bounds, double elapsed, string mode, bool blocked)
    {
        Moving = false;
        if (!double.IsFinite(elapsed) || elapsed <= 0) return position;
        elapsed = Math.Min(elapsed, .1); // No teleport after a suspended/busy UI thread.
        Impact = Math.Max(0, Impact - elapsed * 5);
        if (blocked || mode == "sleep") { Stop(); return position; }
        position.X = Math.Clamp(position.X, bounds.Left, bounds.Right);
        position.Y = Math.Clamp(position.Y, bounds.Top, bounds.Bottom);

        if (Flying)
        {
            // Small substeps keep fast throws stable near corners and narrow work areas.
            int count = (int)Math.Ceiling(elapsed / .008);
            double dt = elapsed / count;
            for (int i = 0; i < count && Flying; i++)
            {
                flightTime += dt;
                VelocityY += 980 * dt;
                VelocityX *= Math.Exp(-.32 * dt);
                position.X += VelocityX * dt;
                position.Y += VelocityY * dt;
                if (position.X < bounds.Left || position.X > bounds.Right)
                {
                    position.X = Math.Clamp(position.X, bounds.Left, bounds.Right);
                    VelocityX *= -.74;
                    Impact = 1;
                }
                if (position.Y < bounds.Top || position.Y > bounds.Bottom)
                {
                    bool floor = position.Y > bounds.Bottom;
                    position.Y = Math.Clamp(position.Y, bounds.Top, bounds.Bottom);
                    VelocityY *= -.68;
                    Impact = 1;
                    if (floor) VelocityX *= .82;
                    if (floor && Math.Abs(VelocityY) < 110) Stop();
                }
                if (flightTime >= 5) Stop();
            }
            if (Math.Abs(VelocityX) > 1) direction = Math.Sign(VelocityX);
            return position;
        }

        if (mode is not ("walk" or "zippy")) return position;
        if (pause > 0) { pause -= elapsed; return position; }
        double speed = mode == "zippy" ? 140 : 58;
        double next = position.X + direction * speed * elapsed;
        if (next < bounds.Left) { next = bounds.Left; direction = 1; }
        if (next > bounds.Right) { next = bounds.Right; direction = -1; }
        Moving = Math.Abs(next - position.X) > .001;
        position.X = next;
        if ((walkRemaining -= elapsed) <= 0)
        {
            pause = mode == "zippy" ? .3 + random.NextDouble() * .6 : .6 + random.NextDouble() * 1.1;
            walkRemaining = 2.5 + random.NextDouble() * 2;
            if (random.Next(3) == 0) direction *= -1;
        }
        return position;
    }
}

public sealed class DragVelocity
{
    readonly Queue<(double Time, Point Position)> samples = new();
    public void Begin(Point position, double time) { samples.Clear(); Add(position, time); }
    public void Add(Point position, double time)
    {
        samples.Enqueue((time, position));
        while (samples.Count > 2 && time - samples.Peek().Time > .12) samples.Dequeue();
    }
    public Vector Release(double time)
    {
        if (samples.Count < 2) return new Vector();
        var first = samples.Peek();
        var last = samples.Last();
        // Holding still before releasing means placement, not a stale throw.
        if (time - last.Time > .1 || last.Time - first.Time < .015 || last.Time - first.Time > .2) return new Vector();
        return (last.Position - first.Position) / (last.Time - first.Time);
    }
}
