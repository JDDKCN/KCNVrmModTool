using KCNVrmModTool.Hardware;

namespace KCNVrmModTool.Services
{
    public class VrmScannerService
    {
        private readonly MCP2221Device _device;
        private readonly I2CommunicationHelper _i2CHelper;
        private const uint DefaultSpeed = 400000;

        public VrmScannerService(MCP2221Device device)
        {
            _device = device;
            _i2CHelper = new I2CommunicationHelper(device);
        }

        public int ScanRange(byte startAddress, byte endAddress)
        {
            byte currentAddress = startAddress;

            while (currentAddress <= endAddress)
            {
                Console.WriteLine($"Scanning at addr: {currentAddress:X2}");
                ScanAddress(currentAddress);
                currentAddress++;
            }

            return 0;
        }

        private void ScanAddress(byte address)
        {
            byte i2cAddress = Convert.ToByte(address << 1);
            byte[] data = new byte[4];
            bool deviceFound = false;

            // Check if any device responds at this address
            if (_device.ReadBlock(i2cAddress, data, 1, DefaultSpeed, 0, 0) != 0)
                return;

            // Check for TPS53679 and TPS53678
            Array.Clear(data, 0, data.Length);
            if (_i2CHelper.ReadBlockScan(i2cAddress, data, 2, DefaultSpeed, 0, 173) == 0)
            {
                if (data[0] == 1 && data[1] == 121)
                {
                    Console.WriteLine($"Probably TPS53679 found at addr: {address:X2}");
                    deviceFound = true;
                }
                else if (data[0] == 1 && data[1] == 120)
                {
                    Console.WriteLine($"Probably TPS53678 found at addr: {address:X2}");
                    deviceFound = true;
                }
            }

            // Check for MP2955A
            if (!deviceFound)
            {
                Array.Clear(data, 0, data.Length);
                if (_i2CHelper.ReadBlockScan(i2cAddress, data, 2, DefaultSpeed, 0, 191) == 0 &&
                    data[0] == 85 && data[1] == 37)
                {
                    Console.WriteLine($"Probably MP2955A found at addr: {address:X2}");
                    deviceFound = true;
                }
            }

            // Check for PXE1610C family
            if (!deviceFound)
            {
                Array.Clear(data, 0, data.Length);
                if (_i2CHelper.ReadBlockScan(i2cAddress, data, 1, DefaultSpeed, 0, 253) == 0 && data[0] == 179)
                {
                    byte[] cmd1AData = new byte[4];
                    if (_i2CHelper.ReadBlockScan(i2cAddress, cmd1AData, 1, DefaultSpeed, 79, 26) == 0 && data[0] == 179 && cmd1AData[0] == 0)
                    {
                        Console.WriteLine($"Primarion family controller found at addr: {address:X2}");

                        byte[] cmd32Data = new byte[4];
                        if (_i2CHelper.ReadBlockScan(i2cAddress, cmd32Data, 2, DefaultSpeed, 79, 50) == 0 &&
                            data[0] == 179 && cmd1AData[0] == 0 && cmd32Data[0] == 21 && cmd32Data[1] == 4)
                        {
                            byte[] remainingData = new byte[4];
                            int result = _i2CHelper.ReadBlockScan(i2cAddress, remainingData, 2, DefaultSpeed, 80, 130);
                            byte remaining = (byte)((remainingData[1] << 2) | (remainingData[0] >> 6));

                            byte[] iccMaxData = new byte[4];
                            result = _i2CHelper.ReadBlockScan(i2cAddress, iccMaxData, 2, DefaultSpeed, 32, 115);
                            int iccMax = iccMaxData[0];

                            Console.WriteLine($"PXE1610C found at addr: {address:X2}");
                            Console.WriteLine($" remaining attempts= {remaining:X2}");
                            Console.WriteLine($" ICC_MAX= {iccMax:X2}");
                        }
                    }
                }
            }
        }
    }
}
