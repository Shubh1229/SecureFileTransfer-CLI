using System.IO;

namespace SecureFileTransfer.src.helper
{
    public class FileBrowserService
    {
        public string? BrowseForFile(string startPath)
        {
            return Browse(startPath, selectDirectoriesOnly: false);
        }

        public string? BrowseForDirectory(string startPath)
        {
            return Browse(startPath, selectDirectoriesOnly: true);
        }

        private string? Browse(string startPath, bool selectDirectoriesOnly)
        {
            string currentPath = Directory.Exists(startPath)
                ? startPath
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Current path: {currentPath}\n");

                List<string> directories = new();
                List<string> files = new();

                try
                {
                    directories.AddRange(Directory.GetDirectories(currentPath));
                    if (!selectDirectoriesOnly)
                    {
                        files.AddRange(Directory.GetFiles(currentPath));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to access directory: {ex.Message}");
                    Console.WriteLine("\nPress any key to go up...");
                    Console.ReadKey();

                    DirectoryInfo? parent = Directory.GetParent(currentPath);
                    if (parent == null)
                    {
                        return null;
                    }

                    currentPath = parent.FullName;
                    continue;
                }

                Console.WriteLine("Directories:");
                Console.WriteLine("  ..");
                if (directories.Count == 0)
                {
                    Console.WriteLine("  (none)");
                }
                else
                {
                    for (int i = 0; i < directories.Count; i++)
                    {
                        Console.WriteLine($"  D{i + 1}: {Path.GetFileName(directories[i])}");
                    }
                }

                if (!selectDirectoriesOnly)
                {
                    Console.WriteLine("\nFiles:");
                    if (files.Count == 0)
                    {
                        Console.WriteLine("  (none)");
                    }
                    else
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            Console.WriteLine($"  F{i + 1}: {Path.GetFileName(files[i])}");
                        }
                    }
                }

                return "";
            }
        }
    }
}