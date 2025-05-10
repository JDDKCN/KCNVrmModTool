using System.Runtime.InteropServices;

namespace KCNVrmModTool.Hardware
{
    public static class NativeDllLoader
    {
        private static IntPtr _dllHandle = IntPtr.Zero;
        private static string _tempFileName = string.Empty;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        // 添加委托定义，与DLL中的函数签名匹配
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void DllInitDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool GetConnectionStatusDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate int SelectDevDelegate(int whichDevice);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate int SmbWriteBlockDelegate(
            byte smbAddress,
            byte[] smbDataToSend,
            uint numberOfBytesToWrite,
            uint smbSpeed,
            byte usesPEC);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate int SmbReadBlockDelegate(
            byte smbAddress,
            byte[] smbDataToRead,
            uint numberOfBytesToRead,
            uint smbSpeed,
            byte usesPEC,
            byte readRegIndex);

        // 存储函数指针；；
        public static DllInitDelegate DllInit;
        public static GetConnectionStatusDelegate GetConnectionStatus;
        public static SelectDevDelegate SelectDev;
        public static SmbWriteBlockDelegate SmbWriteBlock;
        public static SmbReadBlockDelegate SmbReadBlock;

        public static bool LoadDllFromResources()
        {
            if (_dllHandle != IntPtr.Zero)
                return true; // DLL已加载

            try
            {
                // 从资源中获取DLL数据
                byte[] dllData = Properties.Resources.MCP2221DLL_UM_x86;

                // 创建唯一临时文件路径
                _tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.dll");

                // 将DLL数据写入临时文件
                File.WriteAllBytes(_tempFileName, dllData);

                // 加载DLL
                _dllHandle = LoadLibrary(_tempFileName);
                if (_dllHandle == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Exception($"无法加载DLL: {_tempFileName}，错误代码: {error}");
                }

                // 获取函数指针并绑定到委托
                IntPtr pDllInit = GetProcAddress(_dllHandle, "DllInit");
                IntPtr pGetConnectionStatus = GetProcAddress(_dllHandle, "GetConnectionStatus");
                IntPtr pSelectDev = GetProcAddress(_dllHandle, "SelectDev");
                IntPtr pSmbWriteBlock = GetProcAddress(_dllHandle, "SmbWriteBlock");
                IntPtr pSmbReadBlock = GetProcAddress(_dllHandle, "SmbReadBlock");

                if (pDllInit == IntPtr.Zero || pGetConnectionStatus == IntPtr.Zero ||
                    pSelectDev == IntPtr.Zero || pSmbWriteBlock == IntPtr.Zero || pSmbReadBlock == IntPtr.Zero)
                {
                    throw new Exception("无法获取DLL中的函数指针");
                }

                // 创建委托
                DllInit = Marshal.GetDelegateForFunctionPointer<DllInitDelegate>(pDllInit);
                GetConnectionStatus = Marshal.GetDelegateForFunctionPointer<GetConnectionStatusDelegate>(pGetConnectionStatus);
                SelectDev = Marshal.GetDelegateForFunctionPointer<SelectDevDelegate>(pSelectDev);
                SmbWriteBlock = Marshal.GetDelegateForFunctionPointer<SmbWriteBlockDelegate>(pSmbWriteBlock);
                SmbReadBlock = Marshal.GetDelegateForFunctionPointer<SmbReadBlockDelegate>(pSmbReadBlock);

                // 设置程序退出时清理DLL
                AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanupDll();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载DLL资源时出错: {ex.Message}");
                CleanupDll(); // 清理任何部分创建的资源
                return false;
            }
        }

        private static void CleanupDll()
        {
            try
            {
                if (_dllHandle != IntPtr.Zero)
                {
                    FreeLibrary(_dllHandle);
                    _dllHandle = IntPtr.Zero;
                }

                if (!string.IsNullOrEmpty(_tempFileName) && File.Exists(_tempFileName))
                {
                    try
                    {
                        File.Delete(_tempFileName);
                        _tempFileName = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"无法删除临时DLL文件: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理DLL时出错: {ex.Message}");
            }
        }
    }
}