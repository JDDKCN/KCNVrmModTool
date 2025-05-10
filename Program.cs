using KCNVrmModTool.Hardware;
using KCNVrmModTool.Models;
using KCNVrmModTool.Services;
using KCNVrmModTool.VRM;
using System.Text;

namespace KCNVrmModTool
{
    internal class Program
    {
        private static readonly string Title = "KCNVrmModTool V1.0.0 - VRM修改程序";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.Title = Title;
            Console.WriteLine(Title);
            Console.WriteLine("https://github.com/JDDKCN/KCNVrmModTool");
            Console.WriteLine("Copyright 2023-2025 剧毒的KCN, All Rights Reserved.\n");

            var mcp2221Device = new MCP2221Device();
            bool isInitFinish = mcp2221Device.Initialize();

            if (!isInitFinish)
            {
                Console.WriteLine("初始化程序时出错，请按任意键退出程序。\n");
                Console.ReadKey();
                return;
            }

            if (!mcp2221Device.IsConnected)
            {
                Console.WriteLine("MCP2221a设备未连接。\n" +
                    "若您还没有安装MCP2221a设备驱动，访问此链接以下载: \n" +
                    "https://ww1.microchip.com/downloads/aemDocuments/documents/OTH/ProductDocuments/SoftwareLibraries/Firmware/MCP2221_Windows_Driver_2021-02-22.zip\n" +
                    "\n请按任意键退出程序。\n");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("MCP2221a设备已连接。\n");
            mcp2221Device.SelectDevice(0);

            var commandOptions = CommandLineParser.Parse(args);

            if (commandOptions == null)
            {
                Console.WriteLine("程序用法:");
                Console.WriteLine("  -scan [start_addr] [end_addr]  - 扫描 VRM 设备");
                Console.WriteLine("  -MP2955A [addr1] [addr2]       - 修改 MP2955A 设备");
                Console.WriteLine("  -PXE1610C [addr1] [addr2]      - 修改 PXE1610C 设备");
                Console.WriteLine("  -TPS53679 [addr1] [addr2]      - 修改 TPS53679 设备");
                Console.WriteLine("  -TPS53678 [addr1] [addr2]      - 修改 TPS53678 设备");
                Console.WriteLine("请按任意键退出程序。\n");
                Console.ReadKey();
                return;
            }

            ProcessCommand(commandOptions, mcp2221Device);

            Console.WriteLine("\n请按任意键退出程序。\n");
            Console.ReadKey();
        }

        private static void ProcessCommand(CommandOptions options, MCP2221Device device)
        {
            if (options.Command == Command.Scan && options.Address1 > 0 && options.Address2 > 0)
            {
                var scanner = new VrmScannerService(device);
                scanner.ScanRange(options.Address1, options.Address2);
                return;
            }

            IVrmController controller;

            switch (options.Command)
            {
                case Command.MP2955A:
                    controller = new MP2955AController(device);
                    break;
                case Command.PXE1610C:
                    controller = new PXE1610CController(device);
                    break;
                case Command.TPS53679:
                    controller = new TPS53679Controller(device);
                    break;
                case Command.TPS53678:
                    controller = new TPS53678Controller(device);
                    break;
                default:
                    Console.WriteLine("无效命令");
                    return;
            }

            if (options.Address1 > 0)
            {
                controller.SetIccMaxToMaximum(options.Address1);
            }

            if (options.Address2 > 0)
            {
                controller.SetIccMaxToMaximum(options.Address2);
            }
        }
    }
}
