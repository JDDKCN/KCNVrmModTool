using KCNVrmModTool.Hardware;

namespace KCNVrmModTool.VRM
{
    public class PXE1610CController : AbstractVrmController
    {
        public PXE1610CController(MCP2221Device device) : base(device)
        {
        }

        public override bool DetectDevice(byte address)
        {
            byte i2cAddress = GetI2CAddress(address);
            byte[] cmdFDData = new byte[4];
            byte[] cmd1AData = new byte[4];
            byte[] cmd32Data = new byte[4];

            int result = I2CHelper.ReadBlock(i2cAddress, cmdFDData, 1, DefaultSpeed, 0, 253);
            if (result != 0 || cmdFDData[0] != 179) // 0xB3
                return false;

            result = I2CHelper.ReadBlock(i2cAddress, cmd1AData, 1, DefaultSpeed, 79, 26);
            if (result != 0 || cmd1AData[0] != 0)
                return false;

            result = I2CHelper.ReadBlock(i2cAddress, cmd32Data, 2, DefaultSpeed, 79, 50);
            if (result != 0 || cmd32Data[0] != 21 || cmd32Data[1] != 4)
                return false;

            return true;
        }

        public override int SetIccMaxToMaximum(byte address)
        {
            byte i2cAddress = GetI2CAddress(address);
            int result = 0;
            int currentIccMax = 0;
            byte[] data = new byte[8];
            byte[] cmdFDData = new byte[4];
            byte[] cmd1AData = new byte[4];
            byte[] cmd32Data = new byte[4];

            // Verify device type
            result = I2CHelper.ReadBlock(i2cAddress, cmdFDData, 1, DefaultSpeed, 0, 253);
            if (result == 0)
                LogReadSuccess("Page00", "cmdFD", cmdFDData, 1);
            else
                LogReadError("Page00", "cmdFD", 1);

            if (result == 0 && cmdFDData[0] == 179) // 0xB3
            {
                result = I2CHelper.ReadBlock(i2cAddress, cmd1AData, 1, DefaultSpeed, 79, 26);
                if (result == 0)
                    LogReadSuccess("Page4F", "cmd1A", cmd1AData, 1);
                else
                    LogReadError("Page4F", "cmd1A", 1);
            }

            if (result == 0 && cmdFDData[0] == 179 && cmd1AData[0] == 0)
            {
                result = I2CHelper.ReadBlock(i2cAddress, cmd32Data, 2, DefaultSpeed, 79, 50);
                if (result == 0)
                    LogReadSuccess("Page4F", "cmd32", cmd32Data, 2);
                else
                    LogReadError("Page4F", "cmd32", 2);
            }

            if (result != 0 || cmdFDData[0] != 179 || cmd1AData[0] != 0 || cmd32Data[0] != 21 || cmd32Data[1] != 4)
            {
                Console.WriteLine("PXE1610C not found\n");
                return -1;
            }

            Console.WriteLine("PXE1610C found: starting modd\n");

            // Enter programming mode
            data[0] = 39;
            data[1] = 124;
            data[2] = 179;
            result = I2CHelper.WriteBlock(i2cAddress, data, 3, DefaultSpeed, 63);
            if (result == 0)
                LogWriteSuccess("Page3F", "cmd27", "7C B3");
            else
                LogWriteError("Page3F", "cmd27", "7C B3");

            // Read current ICC_MAX
            if (result == 0)
            {
                Array.Clear(data, 0, data.Length);
                result = I2CHelper.ReadBlock(i2cAddress, data, 2, DefaultSpeed, 32, 115);
                currentIccMax = data[0];

                if (result == 0)
                    LogReadSuccess("Page20", "cmd73", data, 2);
                else
                    LogReadError("Page20", "cmd73", 2);
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
                data[0] = 115;
                data[1] = 255; // 0xFF
                data[2] = 0;
                result = I2CHelper.WriteBlock(i2cAddress, data, 3, DefaultSpeed, 32);

                if (result == 0)
                    LogWriteSuccess("Page20", "cmd73", "FF 00");
                else
                    LogWriteError("Page20", "cmd73", "FF 00");
            }

            // Verify ICC_MAX was set
            if (result == 0)
            {
                Array.Clear(data, 0, data.Length);
                result = I2CHelper.ReadBlock(i2cAddress, data, 2, DefaultSpeed, 32, 115);

                if (result == 0)
                    LogReadSuccess("Page20", "cmd73", data, 2);
                else
                    LogReadError("Page20", "cmd73", 2);
            }

            // Write verification data
            if (result == 0)
            {
                data[0] = 41;
                data[1] = 215;
                data[2] = 239;
                result = I2CHelper.WriteBlock(i2cAddress, data, 3, DefaultSpeed, 63);

                if (result == 0)
                    LogWriteSuccess("Page3F", "cmd29", "D7 EF");
                else
                    LogWriteError("Page3F", "cmd29", "D7 EF");
            }

            // Store command
            if (result == 0)
            {
                data[0] = 52;
                result = I2CHelper.WriteBlock(i2cAddress, data, 1, DefaultSpeed, 63);

                if (result == 0)
                    LogWriteSuccess("Page3F", "cmd34", "");
                else
                    LogWriteError("Page3F", "cmd34", "");

                ExecuteStoreCommand();
            }

            // Reset programming mode
            if (result == 0)
            {
                data[0] = 41;
                data[1] = 0;
                data[2] = 0;
                result = I2CHelper.WriteBlock(i2cAddress, data, 3, DefaultSpeed, 63);

                if (result == 0)
                    LogWriteSuccess("Page3F", "cmd29", "00 00");
                else
                    LogWriteError("Page3F", "cmd29", "00 00");
            }

            if (result != 0)
            {
                Console.WriteLine("PXE1610C modd error\n");
                return -1;
            }

            Console.WriteLine("PXE1610C modd successful\n");
            return 0;
        }
    }
}
