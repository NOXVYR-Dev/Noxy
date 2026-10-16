using System.Runtime.InteropServices;

namespace Noxy;

internal static class PetEncounterService
{
    private delegate bool EnumWindowsCallback(IntPtr handle, IntPtr state);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr state);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr handle, System.Text.StringBuilder text, int maxCount);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr handle, out Rect rect);

    public static int FindOverlappingSlot(int ownSlot, IntPtr ownHandle)
    {
        if (ownHandle == IntPtr.Zero || !GetWindowRect(ownHandle, out var own)) return 0;
        var found = 0;
        EnumWindows((handle, _) =>
        {
            if (handle == ownHandle) return true;
            var title = new System.Text.StringBuilder(128);
            GetWindowText(handle, title, title.Capacity);
            var prefix = "Noxy #";
            if (!title.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(title.ToString()[prefix.Length..], out var otherSlot) || otherSlot == ownSlot ||
                !GetWindowRect(handle, out var other)) return true;
            var width = Math.Min(own.Right, other.Right) - Math.Max(own.Left, other.Left);
            var height = Math.Min(own.Bottom, other.Bottom) - Math.Max(own.Top, other.Top);
            if (width > 35 && height > 35) { found = otherSlot; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }
}

