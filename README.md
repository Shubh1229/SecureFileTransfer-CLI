# SecureFileTransfer CLI

A cross-platform CLI tool built in .NET (C#) for securely transferring files between computers over a local network.

---

## 🚀 Overview

SecureFileTransfer is a command-line application designed to allow two machines to:

- Establish a direct connection over IPv4/IPv6
- Transfer files between each other
- Track detailed logs of file transfers and connection sessions
- Manage known peers for quick access

The project is built from the ground up with a focus on:
- modular design
- clean architecture
- future extensibility (encryption, protocol design, etc.)

---

## 🎯 Goals

### Core Goals
- Build a cross-platform CLI tool (Mac, Windows, Linux)
- Enable peer-to-peer file transfer
- Store persistent configuration and peer data
- Implement structured logging

### Future Goals
- End-to-end encryption
- Custom transfer protocol
- IPv6-first support
- Automatic peer discovery
- Resume interrupted transfers

---

## 🧱 Project Structure

SecureFileTransfer/
├── Program.cs
├── data/
│   └── .data/
│       ├── host.yaml
│       └── transfer_logs.yaml
├── src/
│   ├── setup/
│   ├── host/
│   ├── client/
│   ├── logging/
│   └── data_structures/
├── bin/
├── obj/

---

## ⚙️ How It Works

On first launch, the application:
1. Detects host machine info
2. Creates data/.data/
3. Generates:
   - host.yaml
   - transfer_logs.yaml

---

## 🖥️ CLI Menu

Secure File Transfer
1. View host info
2. Manage peers
3. Start host
4. Start client
5. Re-run setup
6. Exit

---

## 📦 Data Storage

Location:
SecureFileTransfer/data/.data/

---

## 🛠️ Setup Instructions

Prerequisites:
- .NET 8+

Run:
dotnet run

---

## 🔐 Security (Planned)

- AES encryption
- Secure key exchange
- Authentication

---

## 🌐 Networking

- TCP sockets
- Default port: 5000
- IPv4 first, IPv6 later

---

## 🧪 Development Status

Completed:
- CLI
- Config system
- Logging

In Progress:
- Host/client networking

Planned:
- File transfer
- Encryption

---

## 📌 Notes

- .data folder is ignored in git
- Logs are local only

---

## 🧑‍💻 Author

Arihant Singh
