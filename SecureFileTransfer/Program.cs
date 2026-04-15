using SecureFileTransfer.src.client;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.host;
using SecureFileTransfer.src.setup;


Console.Clear();
// string basePath = AppContext.BaseDirectory;
// string path = Path.Combine(basePath, "data", ".data", "host.yaml");
string path = Path.Combine(Directory.GetCurrentDirectory(), "data", ".data", "host.yaml");

if (!File.Exists(path))
{
    new Initialize();
}

HostModel host = HostConfigManager.Load();

string menu =
"""
Secure File Transfer
1. View host info
2. Manage peers
3. Start host
4. Start client
5. Re-run setup
6. Exit
""";
Console.WriteLine(menu + "\n");
while (true)
{
    string? option = Console.ReadLine();
    if (option == null || option.Length > 1 || option.Length < 1 || !char.IsDigit(option[0]))
    {
        Console.WriteLine("You Must Give a Valid Input...\nPress any key to continue");
        Console.ReadKey();
        Console.Clear();
        Console.WriteLine(menu + "\n");
        continue;
    }
    int choice = int.Parse(option);
    switch (choice)
    {
        case 1:
            Console.Clear();
            host.PrintInfo();
            break;
        case 2:
            Console.Clear();
            ManagePeers mhp = new();
            host = mhp.ManageHostPeers(host);
            break;
        case 3:
            HostService service = new();
            service.StartHost(host);
            break;
        case 4:
            ClientService client = new();
            client.StartClient(host);
            break;
        case 5:
            Console.Clear();
            new Initialize();
            break;
        case 6:
        default:
            Console.Clear();
            HostConfigManager.Save(host);
            Environment.Exit(0);
            break;
    }
    Console.WriteLine("\nPress any key to continue");
    Console.ReadKey();
    Console.Clear();
    Console.WriteLine(menu + "\n\n");
}



