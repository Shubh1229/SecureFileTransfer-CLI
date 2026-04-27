# SecureFileTransfer CLI

Tired of emailing yourself files or using other roundabout means that limit file sizes you can transfer? Well, here is a cross-platform command-line tool (GUI in being worked on) built in C# (.NET) for securely transferring files between machines on a local network (IPv6 is also being worked on so you can transfer files from different networks too).

## Overview

SecureFileTransfer allows two computers to connect directly and send files between each other. It keeps track of connections, stores known peers, and logs transfer history. The project is designed with a modular structure so features like encryption and a GUI can be added later without major changes.

## Features

- Peer-to-peer file transfer over TCP
- Cross-platform support (macOS, Linux, Windows)
- Encrypted file transfer using ECDH and AES-GCM
- Multi-file transfer support
- Persistent peer list
- Transfer history logging

## How It Works

1. One machine starts in host mode and listens for connections
2. Another machine starts in client mode and connects to the host
3. A handshake and key exchange are performed
4. Files are transferred securely using encrypted chunks
5. Transfer results are logged locally

## Project Structure

SecureFileTransfer/
├── src/
│   ├── client/
│   ├── host/
│   ├── protocols/
│   ├── security/
│   ├── setup/
│   ├── logging/
│   └── data_structures/

## Data Storage

Application data is stored locally on each machine:

- Windows: %AppData%/SecureFileTransfer
- macOS/Linux: ~/.securefiletransfer

This includes:

- host configuration
- peer list
- transfer logs
- debug logs

## Running the CLI

If running from source:

dotnet run

If using a published build:

./sft (macOS/Linux)
sft.exe (Windows)

## Installation (Optional)

macOS / Linux:

chmod +x sft
sudo mv sft /usr/local/bin/sft

Windows:

Add the folder containing sft.exe to your system PATH.

## Networking

- Uses TCP sockets
- Default port: 5000
- Works over local network (IPv4)

## Development Status

Current version: CLI Alpha 1.0

Completed:

- Core CLI functionality
- Host/client communication
- Encrypted file transfer
- Logging and configuration system

Planned:

- GUI application
- Resume interrupted transfers
- Improved peer management
- IPv6 connection for file transfers over different wifi connections

## Notes

- All data and logs are stored locally
- No external services are used

## Author

Arihant Singh