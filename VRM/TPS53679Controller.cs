using KCNVrmModTool.Hardware;

namespace KCNVrmModTool.VRM
{
    public class TPS53679Controller : AbstractVrmController
    {
        public TPS53679Controller(MCP2221Device device) : base(device)
        {
        }

        public override bool DetectDevice(byte address)
        {
            byte i2cAddress = GetI2CAddress(address);
            byte[] idData = new byte[4];

            int result = I2CHelper.ReadBlock(i2cAddress, idData, 2, DefaultSpeed, 0, 173);
            if (result != 0 || idData[0] != 1 || idData[1] != 121) // 0x01, 0x79
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
            result = I2CHelper.ReadBlock(i2cAddress, idData, 2, DefaultSpeed, 0, 173);
            if (result == 0)
                LogReadSuccess("Page00", "cmdAD", idData, 2);
            else
                LogReadError("Page00", "cmdAD", 2);

            if (result != 0 || idData[0] != 1 || idData[1] != 121) // 0x01, 0x79
            {
                Console.WriteLine("TPS53679 not found\n");
                return -1;
            }

            Console.WriteLine("TPS53679 found: starting modd\n");

            // Read current ICC_MAX
            if (result == 0)
            {
                Array.Clear(data, 0, data.Length);
                result = I2CHelper.ReadBlock(i2cAddress, data, 2, DefaultSpeed, 0, 218);
                currentIccMax = data[0];

                if (result == 0)
                    LogReadSuccess("Page00", "cmdDA", data, 2);
                else
                    LogReadError("Page00", "cmdDA", 2);
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
                data[0] = 218;
                data[1] = 255; // 0xFF
                data[2] = 0;
                result = I2CHelper.WriteBlock(i2cAddress, data, 3, DefaultSpeed, 0);

                if (result == 0)
                    LogWriteSuccess("Page00", "cmdDA", "FF 00");
                else
                    LogWriteError("Page00", "cmdDA", "FF 00");
            }

            // Verify ICC_MAX was set
            if (result == 0)
            {
                Array.Clear(data, 0, data.Length);
                result = I2CHelper.ReadBlock(i2cAddress, data, 2, DefaultSpeed, 0, 218);

                if (result == 0)
                    LogReadSuccess("Page00", "cmdDA", data, 2);
                else
                    LogReadError("Page00", "cmdDA", 2);
            }

            // Store command
            if (result == 0)
            {
                data[0] = 17;
                result = I2CHelper.WriteBlock(i2cAddress, data, 1, DefaultSpeed, 0);

                if (result == 0)
                    LogWriteSuccess("Page00", "cmd11", "");
                else
                    LogWriteError("Page00", "cmd11", "");

                ExecuteStoreCommand();
            }

            if (result != 0)
            {
                Console.WriteLine("TPS53679 modd error\n");
                return -1;
            }

            Console.WriteLine("TPS53679 modd successful\n");
            return 0;
        }
    }
}
