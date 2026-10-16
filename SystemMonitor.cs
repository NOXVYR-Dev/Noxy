using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Noxy;
public static class SystemMonitor {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)] private struct MEMORYSTATUSEX { public uint dwLength; public uint dwMemoryLoad; public ulong ullTotalPhys,ullAvailPhys,ullTotalPageFile,ullAvailPageFile,ullTotalVirtual,ullAvailVirtual,ullAvailExtendedVirtual; }
 [DllImport("kernel32.dll",CharSet=CharSet.Auto,SetLastError=true)] private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX status);
 [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd,out uint processId);
 [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);
 [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
 [DllImport("user32.dll")] private static extern IntPtr GetShellWindow();
 [StructLayout(LayoutKind.Sequential)] private struct RECT {public int Left,Top,Right,Bottom;}
 public static bool Running(string name) {try{return Process.GetProcessesByName(name).Length>0;}catch{return false;}}
 public static bool ObsRunning => Running("obs64")||Running("obs32");
 public static bool FalloutRunning => Running("Fallout76");
 public static bool WowRunning => Running("Wow")||Running("WowClassic")||Running("Wow-64");
 public static string? ForegroundProcessName(){try{var window=GetForegroundWindow();if(window==IntPtr.Zero||window==GetDesktopWindow()||window==GetShellWindow())return null;GetWindowThreadProcessId(window,out var processId);if(processId==0)return null;using var process=Process.GetProcessById((int)processId);return process.ProcessName;}catch{return null;}}
 public static int? RamPercent(){var s=new MEMORYSTATUSEX{dwLength=(uint)Marshal.SizeOf<MEMORYSTATUSEX>()};return GlobalMemoryStatusEx(ref s)?(int)s.dwMemoryLoad:null;}
 public static bool ForegroundFullscreen(){try {var h=GetForegroundWindow();if(h==IntPtr.Zero||h==GetDesktopWindow()||h==GetShellWindow())return false;if(!GetWindowRect(h,out var r))return false;var screen=System.Windows.Forms.Screen.FromHandle(h).Bounds;return r.Left<=screen.Left&&r.Top<=screen.Top&&r.Right>=screen.Right&&r.Bottom>=screen.Bottom;}catch{return false;}}
}

