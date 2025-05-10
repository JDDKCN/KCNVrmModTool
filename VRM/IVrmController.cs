namespace KCNVrmModTool.VRM
{
    public interface IVrmController
    {
        bool DetectDevice(byte address);
        int SetIccMaxToMaximum(byte address);
    }
}
