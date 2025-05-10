using KCNVrmModTool.Hardware;

namespace KCNVrmModTool.VRM
{
    public abstract class AbstractVrmController : IVrmController
    {
        protected readonly MCP2221Device Device;
        protected readonly I2CommunicationHelper I2CHelper;
        protected const uint DefaultSpeed = 400000;

        protected AbstractVrmController(MCP2221Device device)
        {
            Device = device;
            I2CHelper = new I2CommunicationHelper(device);
        }

        public abstract bool DetectDevice(byte address);
        public abstract int SetIccMaxToMaximum(byte address);

        protected byte GetI2CAddress(byte address)
        {
            return Convert.ToByte(address << 1);
        }

        protected void LogReadSuccess(string page, string cmd, byte[] data, int length)
        {
            string dataString = "";
            for (int i = 0; i < length; i++)
            {
                dataString += data[i].ToString("X2");
            }
            Console.WriteLine($"ReadBlock({page},{cmd},{length})={dataString}\n");
        }

        protected void LogReadError(string page, string cmd, int length)
        {
            Console.WriteLine($"ReadBlock({page},{cmd},{length})=Error\n");
        }

        protected void LogWriteSuccess(string page, string cmd, string data)
        {
            Console.WriteLine($"WriteBlock({page},{cmd},{data})=OK");
        }

        protected void LogWriteError(string page, string cmd, string data)
        {
            Console.WriteLine($"WriteBlock({page},{cmd},{data})=Error");
        }

        protected void ExecuteStoreCommand(int delayMs = 1000)
        {
            Thread.Sleep(delayMs);
        }
    }
}
