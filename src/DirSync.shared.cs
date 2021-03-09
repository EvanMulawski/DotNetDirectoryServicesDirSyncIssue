using System;
using System.DirectoryServices;

public sealed class DirSync : IDisposable
{
    private DirectoryEntry _directoryEntry;

    public DirSync(string path, string username, string password)
    {
        _directoryEntry = new DirectoryEntry(path, username, password, AuthenticationTypes.Secure | AuthenticationTypes.Sealing | AuthenticationTypes.ServerBind | AuthenticationTypes.Signing);
    }

    public DirSync(string path) : this(path, null, null)
    {

    }

    public Results Run()
    {
        _ = _directoryEntry.NativeObject;

        using var searcher = new DirectorySearcher(_directoryEntry, "(sn=Smith)")
        {
            SizeLimit = int.MaxValue,
            DirectorySynchronization = new DirectorySynchronization(DirectorySynchronizationOptions.ObjectSecurity)
        };

        using var results = searcher.FindAll();

        /*
         * enumeration is required `searcher.DirectorySynchronization.GetDirectorySynchronizationCookie()` to work on netcoreapp3.1
         * otherwise, it throws COMException (0x80005008)
         */
        _ = results.Count;

        /*
         * on net5.0, `searcher.DirectorySynchronization.GetDirectorySynchronizationCookie()` does not throw
         * but returns an invalid, 0-byte dirsync cookie
         */
        var cookie = searcher.DirectorySynchronization.GetDirectorySynchronizationCookie();

        return new Results(cookie?.Length > 0);
    }

    public void Dispose()
    {
        _directoryEntry?.Dispose();
        _directoryEntry = null;
    }

    public class Results
    {
        public Results(bool receivedCookie)
        {
            ReceivedCookie = receivedCookie;
        }

        public bool ReceivedCookie { get; }

        public void LogSummary(Action<string> logger)
        {
            logger($"Cookie received: {(ReceivedCookie ? "PASS" : "FAIL")}");
        }
    }
}
