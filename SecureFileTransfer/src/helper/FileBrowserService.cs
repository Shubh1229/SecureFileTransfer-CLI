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

                string[] directories;
                string[] files;

                try
                {
                    directories = Directory.GetDirectories(currentPath);
                    files = selectDirectoriesOnly ? Array.Empty<string>() : Directory.GetFiles(currentPath);
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
                if (directories.Length == 0)
                {
                    Console.WriteLine("  (none)");
                }
                else
                {
                    for (int i = 0; i < directories.Length; i++)
                    {
                        Console.WriteLine($"  D{i + 1}: {Path.GetFileName(directories[i])}");
                    }
                }

                if (!selectDirectoriesOnly)
                {
                    Console.WriteLine("\nFiles:");
                    if (files.Length == 0)
                    {
                        Console.WriteLine("  (none)");
                    }
                    else
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.WriteLine($"  F{i + 1}: {Path.GetFileName(files[i])}");
                        }
                    }
                }

                Console.WriteLine("\nCommands:");
                Console.WriteLine("  ..  -> go up");
                if (selectDirectoriesOnly)
                {
                    Console.WriteLine("  s   -> select this directory");
                }
                Console.WriteLine("  q   -> cancel");
                Console.Write("\nChoose: ");

                string input = (Console.ReadLine() ?? "").Trim();

                if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (input == "..")
                {
                    DirectoryInfo? parent = Directory.GetParent(currentPath);
                    if (parent != null)
                    {
                        currentPath = parent.FullName;
                    }
                    continue;
                }

                if (selectDirectoriesOnly && input.Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    return currentPath;
                }

                if (input.StartsWith("D", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(input[1..], out int dirIndex))
                {
                    dirIndex--;
                    if (dirIndex >= 0 && dirIndex < directories.Length)
                    {
                        currentPath = directories[dirIndex];
                    }
                    continue;
                }

                if (!selectDirectoriesOnly &&
                    input.StartsWith("F", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(input[1..], out int fileIndex))
                {
                    fileIndex--;
                    if (fileIndex >= 0 && fileIndex < files.Length)
                    {
                        return files[fileIndex];
                    }
                }
            }
        }
    }
}