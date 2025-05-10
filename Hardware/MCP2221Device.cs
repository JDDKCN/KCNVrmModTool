namespace KCNVrmModTool.Hardware
{
    public class MCP2221Device
    {
        public bool IsConnected { get; private set; }

        public bool Initialize()
        {
            try
            {
                // 加载DLL并获取函数指针
                if (!NativeDllLoader.LoadDllFromResources())
                {
                    Console.WriteLine("无法加载驱动Dll。");
                    return false;
                }

                // 使用加载的函数
                NativeDllLoader.DllInit();
                IsConnected = NativeDllLoader.GetConnectionStatus();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化出错：{ex.Message}\n");
                return false;
            }
        }

        public void SelectDevice(int deviceIndex)
        {
            NativeDllLoader.SelectDev(deviceIndex);
        }

        public int WriteBlock(byte address, byte[] data, uint length, uint speed, byte usesPec)
        {
            return NativeDllLoader.SmbWriteBlock(address, data, length, speed, usesPec);
        }

        public int ReadBlock(byte address, byte[] data, uint length, uint speed, byte usesPec, byte regIndex)
        {
            return NativeDllLoader.SmbReadBlock(address, data, length, speed, usesPec, regIndex);
        }
    }
}