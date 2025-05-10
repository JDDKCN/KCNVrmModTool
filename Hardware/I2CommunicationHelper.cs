namespace KCNVrmModTool.Hardware
{
    public class I2CommunicationHelper
    {
        private readonly MCP2221Device _device;
        private const int MaxRetries = 4;

        public I2CommunicationHelper(MCP2221Device device)
        {
            _device = device;
        }

        public int WriteBlock(
            byte smbAddress,
            byte[] smbDataToSend,
            uint numberOfBytesToWrite,
            uint smbSpeed,
            int smbPage)
        {
            int result = 0;
            int retryCount = 0;
            byte[] pageCommand = new byte[4];

            do
            {
                if (retryCount > 0)
                    Console.WriteLine($" try #{retryCount}");

                retryCount++;

                if (smbPage >= 0)
                {
                    pageCommand[0] = 0;
                    pageCommand[1] = Convert.ToByte(smbPage);
                    result = _device.WriteBlock(smbAddress, pageCommand, 2, smbSpeed, 0);
                }

                if (result == 0)
                    result = _device.WriteBlock(smbAddress, smbDataToSend, numberOfBytesToWrite, smbSpeed, 0);
            }
            while (result != 0 && retryCount < MaxRetries);

            return result;
        }

        public int ReadBlock(
            byte smbAddress,
            byte[] smbDataToRead,
            uint numberOfBytesToRead,
            uint smbSpeed,
            int smbPage,
            byte readRegIndex)
        {
            int result = 0;
            int retryCount = 0;
            byte[] pageCommand = new byte[4];

            do
            {
                if (retryCount > 0)
                    Console.WriteLine($" retry#{retryCount}");

                retryCount++;

                if (smbPage >= 0)
                {
                    pageCommand[0] = 0;
                    pageCommand[1] = Convert.ToByte(smbPage);
                    result = _device.WriteBlock(smbAddress, pageCommand, 2, smbSpeed, 0);
                }

                if (result == 0)
                    result = _device.ReadBlock(smbAddress, smbDataToRead, numberOfBytesToRead, smbSpeed, 0, readRegIndex);
            }
            while (result != 0 && retryCount < MaxRetries);

            return result;
        }

        public int ReadBlockScan(
            byte smbAddress,
            byte[] smbDataToRead,
            uint numberOfBytesToRead,
            uint smbSpeed,
            int smbPage,
            byte readRegIndex)
        {
            int result = 0;
            int retryCount = 0;
            byte[] pageCommand = new byte[4];

            do
            {
                retryCount++;

                if (smbPage >= 0)
                {
                    pageCommand[0] = 0;
                    pageCommand[1] = Convert.ToByte(smbPage);
                    result = _device.WriteBlock(smbAddress, pageCommand, 2, smbSpeed, 0);
                }

                if (result == 0)
                    result = _device.ReadBlock(smbAddress, smbDataToRead, numberOfBytesToRead, smbSpeed, 0, readRegIndex);
            }
            while (result != 0 && retryCount < 2);

            return result;
        }
    }
}
