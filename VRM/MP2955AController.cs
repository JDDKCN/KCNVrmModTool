using KCNVrmModTool.Hardware;

namespace KCNVrmModTool.VRM
{
    public class MP2955AController : AbstractVrmController
    {
        public MP2955AController(MCP2221Device device) : base(device)
        {
        }

        public override bool DetectDevice(byte address)
        {
            byte i2cAddress = GetI2CAddress(address);
            byte[] idData = new byte[4];

            int result = I2CHelper.ReadBlock(i2cAddress, idData, 2, DefaultSpeed, 0, 191);
            if (result != 0 || idData[0] != 85 || idData[1] != 37) // 0x55, 0x25
                return false;

            return true;
        }

        public override int SetIccMaxToMaximum(byte address)
        {
            byte i2cAddress = GetI2CAddress(address);
            int result = 0;
            int currentIccMax = 0;
            byte[] data = new byte[8];
            byte[] idData = new byte[4];

            // Verify device type
            result = I2CHelper.ReadBlock(i2cAddress, idData, 2, DefaultSpeed, 0, 191);
            if (result == 0)
                LogReadSuccess("Page00", "cmdBF", idData, 2);
            else
                LogReadError("Page00", "cmdBF", 2);

            if (result != 0 || idData[0] != 85 || idData[1] != 37) // 0x55, 0x25
            {
                Console.WriteLine("MP2955A not found\n");
                return -1;
            }

            Console.WriteLine("MP2955A found: starting modd\n");

            // Read current ICC_MAX
            if (result == 0)
            {
                Array.Clear(data, 0, data.Length);
                result = I2CHelper.ReadBlock(i2cAddress, data, 1, DefaultSpeed, 0, 239);
                currentIccMax = data[0];

                if (result == 0)
                    LogReadSuccess("Page00", "cmdEF", data, 1);
                else
                    LogReadError("Page00", "cmdEF", 1);
            }

            // Skip if already at maximum
            if (currentIccMax == 255 && result == 0)
            {
                Console.WriteLine(" ICC_MAX already 255A - modification skipped.\n");
                return 0;
            }

            // Set ICC_MAX to maximum
            if (result == 0)
            {
                data[0] = 239;
                data[1] = 255; // 0xFF
                result = I2CHelper.WriteBlock(i2cAddress, data, 2, DefaultSpeed, 0);

                if (result == 0)
                    LogWriteSuccess("Page00", "cmdEF", "FF");
                else
                    LogWriteError("Page00", "cmdEF", "FF");
            }

            // Verify ICC_MAX was set
            if (result == 0)
            {
                Array.Clear(data, 0, data.Length);
                result = I2CHelper.ReadBlock(i2cAddress, data, 1, DefaultSpeed, 0, 239);

                if (result == 0)
                    LogReadSuccess("Page00", "cmdEF", data, 1);
                else
                    LogReadError("Page00", "cmdEF", 1);
            }

            // Store command
            if (result == 0)
            {
                data[0] = 21;
                result = I2CHelper.WriteBlock(i2cAddress, data, 1, DefaultSpeed, 0);

                if (result == 0)
                    LogWriteSuccess("Page00", "cmd15", "");
                else
                    LogWriteError("Page00", "cmd15", "");

                ExecuteStoreCommand();
            }

            if (result != 0)
            {
                Console.WriteLine("MP2955A modd error\n");
                return -1;
            }

            Console.WriteLine("MP2955A modd successful\n");
            return 0;
        }
    }
}
