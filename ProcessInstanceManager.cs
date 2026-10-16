namespace Noxy;

internal static class ProcessInstanceManager
{
    private static Mutex? ownedMutex;
    public static int Slot { get; private set; }

    public static bool TryAcquire(int maxInstances, out int slot)
    {
        slot = 0;
        for (var i = 1; i <= Math.Clamp(maxInstances, 1, 3); i++)
        {
            var candidate = new Mutex(false, $"Local\\Noxy.Instance.{i}");
            try
            {
                if (!candidate.WaitOne(0)) { candidate.Dispose(); continue; }
            }
            catch (AbandonedMutexException) { }
            ownedMutex = candidate;
            Slot = slot = i;
            return true;
        }
        return false;
    }

    public static void Release()
    {
        if (ownedMutex is null) return;
        try { ownedMutex.ReleaseMutex(); }
        finally { ownedMutex.Dispose(); ownedMutex = null; }
    }
}

