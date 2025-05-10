namespace KCNVrmModTool.Models
{
    public enum Command
    {
        None,
        Scan,
        MP2955A,
        PXE1610C,
        TPS53679,
        TPS53678
    }

    public class CommandOptions
    {
        public Command Command { get; set; }
        public byte Address1 { get; set; }
        public byte Address2 { get; set; }
    }
}
