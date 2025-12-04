using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace WindowsCleaner
{
    public class CleanerOptions
    {
        public bool DryRun { get; set; }
        public bool EmptyRecycleBin { get; set; }
        public bool IncludeSystemTemp { get; set; }
        public bool CleanChrome { get; set; }
        public bool CleanEdge { get; set; }
        public bool CleanWindowsUpdate { get; set; }
        public bool CleanThumbnails { get; set; }
        public bool CleanPrefetch { get; set; }
        public bool FlushDns { get; set; }
        public bool Verbose { get; set; }
    }

    public class CleanerResult
    {
        public int FilesDeleted { get; set; }
        public long BytesFreed { get; set; }
    }

    public class ReportItem
    {
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
    }

    public class CleanerReport
    {
        public System.Collections.Generic.List<ReportItem> Items { get; } = new System.Collections.Generic.List<ReportItem>();
        public long TotalBytes => Items.Sum(i => i.Size);
        public int Count => Items.Count;
    }

    public static class Cleaner
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        // Flags for SHEmptyRecycleBin
        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI = 0x00000002;
        private const uint SHERB_NOSOUND = 0x00000004;

        public static CleanerResult RunCleanup(CleanerOptions options, Action<string>? log = null)
        {
            var result = new CleanerResult();

            void Log(string s)
            {
                if (options.Verbose)
                    log?.Invoke(s);
            }

            // User temp
            try
            {
                string userTemp = Path.GetTempPath();
                Log($"Nettoyage du dossier temporaire utilisateur: {userTemp}");
                var r = DeleteDirectoryContents(userTemp, options.DryRun, Log);
                result.FilesDeleted += r.files;
                result.BytesFreed += r.bytes;
            }
            catch (Exception ex)
            {
                Log("Erreur nettoyage user temp: " + ex.Message);
            }

            // LocalAppData\Temp
            try
            {
                var localTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
                Log($"Nettoyage du dossier LocalAppData Temp: {localTemp}");
                var r = DeleteDirectoryContents(localTemp, options.DryRun, Log);
                result.FilesDeleted += r.files;
                result.BytesFreed += r.bytes;
            }
            catch (Exception ex)
            {
                Log("Erreur nettoyage LocalAppData Temp: " + ex.Message);
            }

            // System temp (optional)
            if (options.IncludeSystemTemp)
            {
                try
                {
                    var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    var systemTemp = Path.Combine(windows, "Temp");
                    Log($"Nettoyage du dossier Temp système: {systemTemp}");
                    var r = DeleteDirectoryContents(systemTemp, options.DryRun, Log);
                    result.FilesDeleted += r.files;
                    result.BytesFreed += r.bytes;
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage System Temp: " + ex.Message);
                }
            }

            // Chrome cache
            if (options.CleanChrome)
            {
                try
                {
                    var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var chromeCache = Path.Combine(local, "Google", "Chrome", "User Data", "Default", "Cache");
                    Log($"Nettoyage cache Chrome: {chromeCache}");
                    var r = DeleteDirectoryContents(chromeCache, options.DryRun, log ?? (s => { }));
                    result.FilesDeleted += r.files;
                    result.BytesFreed += r.bytes;
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage Chrome: " + ex.Message);
                }
            }

            // Edge cache
            if (options.CleanEdge)
            {
                try
                {
                    var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var edgeCache = Path.Combine(local, "Microsoft", "Edge", "User Data", "Default", "Cache");
                    Log($"Nettoyage cache Edge: {edgeCache}");
                    var r = DeleteDirectoryContents(edgeCache, options.DryRun, log ?? (s => { }));
                    result.FilesDeleted += r.files;
                    result.BytesFreed += r.bytes;
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage Edge: " + ex.Message);
                }
            }

            // Windows Update cache (SoftwareDistribution) - requires admin
            if (options.CleanWindowsUpdate)
            {
                try
                {
                    var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    var sd = Path.Combine(windir, "SoftwareDistribution", "Download");
                    Log($"Nettoyage SoftwareDistribution\\Download: {sd}");
                    var r = DeleteDirectoryContents(sd, options.DryRun, log ?? (s => { }));
                    result.FilesDeleted += r.files;
                    result.BytesFreed += r.bytes;
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage SoftwareDistribution: " + ex.Message);
                }
            }

            // Thumbnails
            if (options.CleanThumbnails)
            {
                try
                {
                    var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var thumbDir = Path.Combine(local, "Microsoft", "Windows", "Explorer");
                    Log($"Nettoyage vignettes: {thumbDir}");
                    // delete thumbcache_*.db files
                    var files = Directory.Exists(thumbDir) ? Directory.GetFiles(thumbDir, "thumbcache_*.db") : Array.Empty<string>();
                    foreach (var f in files)
                    {
                        try
                        {
                            var fi = new FileInfo(f);
                            if (!options.DryRun) File.Delete(f);
                            result.FilesDeleted++;
                            result.BytesFreed += fi.Length;
                            log?.Invoke($"Supprimé vignette: {f}");
                        }
                        catch (Exception ex)
                        {
                            log?.Invoke($"Impossible de supprimer vignette {f}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage vignettes: " + ex.Message);
                }
            }

            // Prefetch (requires admin)
            if (options.CleanPrefetch)
            {
                try
                {
                    var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    var prefetch = Path.Combine(windir, "Prefetch");
                    Log($"Nettoyage Prefetch: {prefetch}");
                    var r = DeleteDirectoryContents(prefetch, options.DryRun, log ?? (s => { }));
                    result.FilesDeleted += r.files;
                    result.BytesFreed += r.bytes;
                }
                catch (Exception ex)
                {
                    Log("Erreur nettoyage Prefetch: " + ex.Message);
                }
            }

            // Flush DNS
            if (options.FlushDns)
            {
                try
                {
                    Log("Exécution flush DNS...");
                    if (!options.DryRun)
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo("ipconfig", "/flushdns") { CreateNoWindow = true, UseShellExecute = false };
                        var p = System.Diagnostics.Process.Start(psi);
                        p?.WaitForExit(5000);
                    }
                    else
                    {
                        Log("(dry-run) ipconfig /flushdns non exécuté");
                    }
                }
                catch (Exception ex)
                {
                    Log("Erreur flush DNS: " + ex.Message);
                }
            }

            // Empty Recycle Bin
            if (options.EmptyRecycleBin)
            {
                try
                {
                    Log("Vidage de la Corbeille...");
                    if (!options.DryRun)
                    {
                        uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
                        SHEmptyRecycleBin(IntPtr.Zero, null, flags);
                    }
                    else
                    {
                        Log("(dry-run) Corbeille non vidée");
                    }
                }
                catch (Exception ex)
                {
                    Log("Erreur vidage Corbeille: " + ex.Message);
                }
            }

            return result;
        }

        public static CleanerReport GenerateReport(CleanerOptions options, Action<string>? progress = null)
        {
            var report = new CleanerReport();

            void P(string s) => progress?.Invoke(s);

            try
            {
                string userTemp = Path.GetTempPath();
                P($"Scan du dossier temporaire utilisateur: {userTemp}");
                ScanDirectory(userTemp, report, P);
            }
            catch (Exception ex) { P("Erreur scan user temp: " + ex.Message); }

            try
            {
                var localTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
                P($"Scan LocalAppData Temp: {localTemp}");
                ScanDirectory(localTemp, report, P);
            }
            catch (Exception ex) { P("Erreur scan LocalAppData Temp: " + ex.Message); }

            if (options.IncludeSystemTemp)
            {
                try
                {
                    var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    var systemTemp = Path.Combine(windows, "Temp");
                    P($"Scan Temp système: {systemTemp}");
                    ScanDirectory(systemTemp, report, P);
                }
                catch (Exception ex) { P("Erreur scan System Temp: " + ex.Message); }
            }

            if (options.CleanChrome)
            {
                try
                {
                    var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var chromeCache = Path.Combine(local, "Google", "Chrome", "User Data", "Default", "Cache");
                    P($"Scan cache Chrome: {chromeCache}");
                    ScanDirectory(chromeCache, report, P);
                }
                catch (Exception ex) { P("Erreur scan Chrome: " + ex.Message); }
            }

            if (options.CleanEdge)
            {
                try
                {
                    var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var edgeCache = Path.Combine(local, "Microsoft", "Edge", "User Data", "Default", "Cache");
                    P($"Scan cache Edge: {edgeCache}");
                    ScanDirectory(edgeCache, report, P);
                }
                catch (Exception ex) { P("Erreur scan Edge: " + ex.Message); }
            }

            if (options.CleanWindowsUpdate)
            {
                try
                {
                    var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    var sd = Path.Combine(windir, "SoftwareDistribution", "Download");
                    P($"Scan SoftwareDistribution\\Download: {sd}");
                    ScanDirectory(sd, report, P);
                }
                catch (Exception ex) { P("Erreur scan SoftwareDistribution: " + ex.Message); }
            }

            if (options.CleanThumbnails)
            {
                try
                {
                    var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var thumbDir = Path.Combine(local, "Microsoft", "Windows", "Explorer");
                    P($"Scan vignettes: {thumbDir}");
                    if (Directory.Exists(thumbDir))
                    {
                        foreach (var f in Directory.GetFiles(thumbDir, "thumbcache_*.db"))
                        {
                            try { var fi = new FileInfo(f); report.Items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false }); }
                            catch { }
                        }
                    }
                }
                catch (Exception ex) { P("Erreur scan vignettes: " + ex.Message); }
            }

            if (options.CleanPrefetch)
            {
                try
                {
                    var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    var prefetch = Path.Combine(windir, "Prefetch");
                    P($"Scan Prefetch: {prefetch}");
                    ScanDirectory(prefetch, report, P);
                }
                catch (Exception ex) { P("Erreur scan Prefetch: " + ex.Message); }
            }

            if (options.EmptyRecycleBin)
            {
                // We cannot enumerate recycle bin easily without COM; just note action
                P("Note: la Corbeille sera vidée (taille non estimée)");
            }

            return report;
        }

        private static void ScanDirectory(string path, CleanerReport report, Action<string>? progress)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            try
            {
                foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { var fi = new FileInfo(f); report.Items.Add(new ReportItem { Path = f, Size = fi.Length, IsDirectory = false }); }
                    catch { }
                }
                // Also add directories as items (size 0) for visibility
                foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    try { report.Items.Add(new ReportItem { Path = d, Size = 0, IsDirectory = true }); }
                    catch { }
                }
            }
            catch (Exception ex) { progress?.Invoke($"Erreur en scannant {path}: {ex.Message}"); }
        }

        private static (int files, long bytes) DeleteDirectoryContents(string path, bool dryRun, Action<string>? log)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return (0, 0);

            int deleted = 0;
            long bytes = 0;

            try
            {
                var entries = Directory.GetFileSystemEntries(path);
                foreach (var entry in entries)
                {
                    try
                    {
                        if (File.Exists(entry))
                        {
                            var fi = new FileInfo(entry);
                            // Attempt deletion with retries
                            var (ok, freed) = TryDeleteFileWithRetries(entry, dryRun, log);
                            if (ok)
                            {
                                bytes += freed;
                                deleted++;
                                log?.Invoke($"Supprimé: {entry}");
                            }
                        }
                        else if (Directory.Exists(entry))
                        {
                            // Attempt to delete directory recursively with retries
                            var (ok, freed) = TryDeleteDirectoryWithRetries(entry, dryRun, log);
                            if (ok)
                            {
                                deleted++;
                                bytes += freed; // best-effort (may be 0)
                                log?.Invoke($"Supprimé dossier: {entry}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Impossible de supprimer {entry}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Invoke($"Erreur en listant {path}: {ex.Message}");
            }

            return (deleted, bytes);
        }

        private static (bool deleted, long bytesFreed) TryDeleteFileWithRetries(string filePath, bool dryRun, Action<string>? log, int maxAttempts = 5)
        {
            if (!File.Exists(filePath)) return (false, 0);
            long size = 0;
            try
            {
                var fi = new FileInfo(filePath);
                size = fi.Length;
            }
            catch { }

            if (dryRun)
            {
                log?.Invoke($"(dry-run) Suppression planifiée: {filePath}");
                return (true, size);
            }

            int attempt = 0;
            var delay = 150;
            while (attempt < maxAttempts)
            {
                try
                {
                    File.Delete(filePath);
                    return (true, size);
                }
                catch (IOException ioEx)
                {
                    attempt++;
                    log?.Invoke($"Fichier verrouillé, tentative {attempt}/{maxAttempts}: {filePath} - {ioEx.Message}");
                    Thread.Sleep(delay);
                    delay *= 2;
                    continue;
                }
                catch (UnauthorizedAccessException ua)
                {
                    log?.Invoke($"Accès refusé à {filePath}: {ua.Message}");
                    return (false, 0);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Erreur suppression fichier {filePath}: {ex.Message}");
                    return (false, 0);
                }
            }

            log?.Invoke($"Échec suppression après {maxAttempts} tentatives: {filePath}");
            return (false, 0);
        }

        private static (bool deleted, long bytesFreed) TryDeleteDirectoryWithRetries(string dirPath, bool dryRun, Action<string>? log, int maxAttempts = 4)
        {
            if (!Directory.Exists(dirPath)) return (false, 0);

            long totalFreed = 0;
            try
            {
                // Try to calculate approximate size (sum of file lengths)
                foreach (var f in Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    try { totalFreed += new FileInfo(f).Length; } catch { }
                }
            }
            catch { }

            if (dryRun)
            {
                log?.Invoke($"(dry-run) Suppression planifiée du dossier: {dirPath}");
                return (true, totalFreed);
            }

            int attempt = 0;
            var delay = 200;
            while (attempt < maxAttempts)
            {
                try
                {
                    Directory.Delete(dirPath, true);
                    return (true, totalFreed);
                }
                catch (IOException ioEx)
                {
                    attempt++;
                    log?.Invoke($"Dossier verrouillé, tentative {attempt}/{maxAttempts}: {dirPath} - {ioEx.Message}");
                    Thread.Sleep(delay);
                    delay *= 2;
                    continue;
                }
                catch (UnauthorizedAccessException ua)
                {
                    log?.Invoke($"Accès refusé au dossier {dirPath}: {ua.Message}");
                    return (false, 0);
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Erreur suppression dossier {dirPath}: {ex.Message}");
                    return (false, 0);
                }
            }

            log?.Invoke($"Échec suppression dossier après {maxAttempts} tentatives: {dirPath}");
            return (false, 0);
        }
    }
}
