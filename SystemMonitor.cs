using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Noxy;
public static class SystemMonitor {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)] private struct MEMORYSTATUSEX { public uint dwLength; public uint dwMemoryLoad; public ulong ullTotalPhys,ullAvailPhys,ullTotalPageFile,ullAvailPageFile,ullTotalVirtual,ullAvailVirtual,ullAvailExtendedVirtual; }
 [StructLayout(LayoutKind.Explicit)] private struct PDH_FMT_COUNTERVALUE { [FieldOffset(0)] public uint CStatus; [FieldOffset(8)] public double DoubleValue; }
 [StructLayout(LayoutKind.Sequential)] private struct PDH_FMT_COUNTERVALUE_ITEM { public IntPtr Name; public PDH_FMT_COUNTERVALUE Value; }
 [DllImport("kernel32.dll",CharSet=CharSet.Auto,SetLastError=true)] private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX status);
 [DllImport("kernel32.dll",SetLastError=true)] private static extern bool GetSystemTimes(out ulong idleTime,out ulong kernelTime,out ulong userTime);
 [DllImport("pdh.dll",EntryPoint="PdhOpenQueryW",CharSet=CharSet.Unicode)] private static extern uint PdhOpenQuery(IntPtr dataSource,UIntPtr userData,out IntPtr query);
 [DllImport("pdh.dll",EntryPoint="PdhAddEnglishCounterW",CharSet=CharSet.Unicode)] private static extern uint PdhAddEnglishCounter(IntPtr query,string counterPath,UIntPtr userData,out IntPtr counter);
 [DllImport("pdh.dll",EntryPoint="PdhCollectQueryData")] private static extern uint PdhCollectQueryData(IntPtr query);
 [DllImport("pdh.dll",EntryPoint="PdhGetFormattedCounterArrayW",CharSet=CharSet.Unicode)] private static extern uint PdhGetFormattedCounterArray(IntPtr counter,uint format,ref uint bufferSize,out uint itemCount,IntPtr items);
 [DllImport("pdh.dll",EntryPoint="PdhCloseQuery")] private static extern uint PdhCloseQuery(IntPtr query);
 [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd,out uint processId);
 [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);
 [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
 [DllImport("user32.dll")] private static extern IntPtr GetShellWindow();
 [StructLayout(LayoutKind.Sequential)] private struct RECT {public int Left,Top,Right,Bottom;}
 private static readonly object cpuLock=new(),gpuLock=new();
 private static bool hasCpuSample,gpuInitialized,gpuUnavailable;
 private static ulong previousIdle,previousKernel,previousUser;
 private static IntPtr gpuQuery,gpuCounter;
 private const uint PdhFormatDouble=0x00000200, PdhMoreData=0x800007D2;
 public static bool Running(string name) {try{return Process.GetProcessesByName(name).Length>0;}catch{return false;}}
 public static bool ObsRunning => Running("obs64")||Running("obs32");
 public static bool FalloutRunning => Running("Fallout76");
 public static bool WowRunning => Running("Wow")||Running("WowClassic")||Running("Wow-64");
 public static string? ForegroundProcessName(){try{var window=GetForegroundWindow();if(window==IntPtr.Zero||window==GetDesktopWindow()||window==GetShellWindow())return null;GetWindowThreadProcessId(window,out var processId);if(processId==0)return null;using var process=Process.GetProcessById((int)processId);return process.ProcessName;}catch{return null;}}
 public static int? RamPercent(){var s=new MEMORYSTATUSEX{dwLength=(uint)Marshal.SizeOf<MEMORYSTATUSEX>()};return GlobalMemoryStatusEx(ref s)?(int)s.dwMemoryLoad:null;}
 public static int? CpuPercent(){
  lock(cpuLock){
   if(!GetSystemTimes(out var idle,out var kernel,out var user))return null;
   if(!hasCpuSample){previousIdle=idle;previousKernel=kernel;previousUser=user;hasCpuSample=true;return null;}
   ulong idleDelta=idle-previousIdle,totalDelta=(kernel-previousKernel)+(user-previousUser);
   previousIdle=idle;previousKernel=kernel;previousUser=user;
   if(totalDelta==0||idleDelta>totalDelta)return null;
   return (int)Math.Clamp(Math.Round(100d*(totalDelta-idleDelta)/totalDelta),0,100);
  }
 }
 // Returns the busiest GPU engine reported by Windows, which is a useful overall
 // activity estimate without summing engines that can run in parallel.
 public static int? GpuPercent(){
  lock(gpuLock){
   try{
    if(gpuUnavailable)return null;
    if(!gpuInitialized){
     gpuInitialized=true;
     if(PdhOpenQuery(IntPtr.Zero,UIntPtr.Zero,out gpuQuery)!=0||PdhAddEnglishCounter(gpuQuery,@"\GPU Engine(*)\Utilization Percentage",UIntPtr.Zero,out gpuCounter)!=0){
      if(gpuQuery!=IntPtr.Zero)PdhCloseQuery(gpuQuery);
      gpuQuery=gpuCounter=IntPtr.Zero;gpuUnavailable=true;return null;
     }
    }
    if(PdhCollectQueryData(gpuQuery)!=0)return null;
    uint size=0,count=0;
    var status=PdhGetFormattedCounterArray(gpuCounter,PdhFormatDouble,ref size,out count,IntPtr.Zero);
    if(status!=PdhMoreData||size==0||size>4_194_304)return null;
    IntPtr buffer=Marshal.AllocHGlobal((int)size);
    try{
     if(PdhGetFormattedCounterArray(gpuCounter,PdhFormatDouble,ref size,out count,buffer)!=0)return null;
     int itemSize=Marshal.SizeOf<PDH_FMT_COUNTERVALUE_ITEM>();
     double maximum=double.NegativeInfinity;
     for(int i=0;i<count;i++){
      var item=Marshal.PtrToStructure<PDH_FMT_COUNTERVALUE_ITEM>(IntPtr.Add(buffer,i*itemSize));
      if((item.Value.CStatus is 0 or 1)&&double.IsFinite(item.Value.DoubleValue))maximum=Math.Max(maximum,item.Value.DoubleValue);
     }
     return double.IsFinite(maximum)?(int)Math.Clamp(Math.Round(maximum),0,100):null;
    }finally{Marshal.FreeHGlobal(buffer);}
   }catch(DllNotFoundException){gpuUnavailable=true;return null;}
   catch(EntryPointNotFoundException){gpuUnavailable=true;return null;}
   catch{ return null; }
  }
 }
 public static bool ForegroundFullscreen(){try {var h=GetForegroundWindow();if(h==IntPtr.Zero||h==GetDesktopWindow()||h==GetShellWindow())return false;if(!GetWindowRect(h,out var r))return false;var screen=System.Windows.Forms.Screen.FromHandle(h).Bounds;return r.Left<=screen.Left&&r.Top<=screen.Top&&r.Right>=screen.Right&&r.Bottom>=screen.Bottom;}catch{return false;}}
}
