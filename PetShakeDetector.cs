using System.Windows;
using Point = System.Windows.Point;

namespace Noxy;

public sealed class PetShakeDetector
{
    const double StrokeLength = 22;
    const double InactivityResetSeconds = .3;
    const double ShakeWindowSeconds = .9;
    Point lastPosition, extremePosition;
    double lastTime, sequenceStart;
    int axis = -1, direction, reversals;

    public void Begin(Point position, double time)
    {
        lastPosition = extremePosition = position;
        lastTime = sequenceStart = time;
        axis = -1;
        direction = reversals = 0;
    }

    public bool Add(Point position, double time)
    {
        if (!double.IsFinite(time) || time <= lastTime || !double.IsFinite(position.X) || !double.IsFinite(position.Y)) return false;
        if (time - lastTime > InactivityResetSeconds)
        {
            Begin(position, time);
            return false;
        }

        double deltaX = position.X - lastPosition.X, deltaY = position.Y - lastPosition.Y;
        lastPosition = position;
        if (axis < 0)
        {
            if (Math.Max(Math.Abs(deltaX), Math.Abs(deltaY)) < 9) return false;
            axis = Math.Abs(deltaX) >= Math.Abs(deltaY) ? 0 : 1;
            direction = Math.Sign(axis == 0 ? deltaX : deltaY);
            extremePosition = position;
            sequenceStart = time;
            return false;
        }

        if (time - sequenceStart > ShakeWindowSeconds)
        {
            reversals = 0;
            sequenceStart = time;
        }

        double coordinate = axis == 0 ? position.X : position.Y;
        double extreme = axis == 0 ? extremePosition.X : extremePosition.Y;
        if (direction > 0)
        {
            if (coordinate > extreme) extremePosition = position;
            else if (extreme - coordinate >= StrokeLength) { direction = -1; extremePosition = position; reversals++; }
        }
        else
        {
            if (coordinate < extreme) extremePosition = position;
            else if (coordinate - extreme >= StrokeLength) { direction = 1; extremePosition = position; reversals++; }
        }
        if (reversals < 2) return false;
        Begin(position, time);
        return true;
    }
}

