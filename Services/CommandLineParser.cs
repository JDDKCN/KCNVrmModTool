using KCNVrmModTool.Models;
using System.Globalization;

namespace KCNVrmModTool.Services
{
    public static class CommandLineParser
    {
        public static CommandOptions Parse(string[] args)
        {
            if (args.Length < 1)
                return null;

            var options = new CommandOptions();

            switch (args[0])
            {
                case "-scan":
                    options.Command = Command.Scan;
                    break;
                case "-MP2955A":
                    options.Command = Command.MP2955A;
                    break;
                case "-PXE1610C":
                    options.Command = Command.PXE1610C;
                    break;
                case "-TPS53679":
                    options.Command = Command.TPS53679;
                    break;
                case "-TPS53678":
                    options.Command = Command.TPS53678;
                    break;
                default:
                    Console.WriteLine("参数错误, 仅允许 -scan, -MP2955A, -PXE1610C, -TPS53679 或 -TPS53678。\n");
                    return null;
            }

            if (args.Length > 1)
            {
                if (!byte.TryParse(args[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte result1))
                {
                    Console.WriteLine("参数 1 解析错误。\n");
                    return null;
                }

                if (result1 > 0x7F)
                {
                    Console.WriteLine("参数 1 超出范围。\n");
                    return null;
                }

                options.Address1 = result1;

                if (args.Length > 2)
                {
                    if (!byte.TryParse(args[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte result2))
                    {
                        Console.WriteLine("参数 2 解析错误。\n");
                        return null;
                    }

                    if (result2 > 0x7F)
                    {
                        Console.WriteLine("参数 2 超出范围。\n");
                        return null;
                    }

                    options.Address2 = result2;
                }
            }

            return options;
        }
    }
}
