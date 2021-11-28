
namespace OSVersion.Lib.Linux
{
    internal enum Distribution
    {
        None = 0,
        Unknown = 1,
        RedHat = 2,
        RedHatEnterpriseLinux = 3,
        RHEL = 3,
        CentOS = 11,
        CentOSStream = 12,
        Fedora = 21,
        Debian = 31,
        Ubuntu = 41,
        SUSE = 51,
        OpenSUSE = 52,
        Photon = 61,
        Alpine = 71,
    }

    //  Android対応の予定は無いので、Androidは作りません
}
