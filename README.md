# NosTale-PacketLogger

- supports filtering packet files that have following format: `[{hh:mm:ss}]\t[{Recv|Send}]\t{packet}`. One packet per line.
- supports injecting to running NosTale (may not work with older or some custom clients)
- supports capturing packets using npcap (install npcap to use this) of a running NosTale process (may not work with older or custom clients, may have problems capturing login packets)
- has filter profiles that may be saved and will be loaded on startup. That way you may set up filters quickly
- may connect to multiple sources at once or multiple times to one source and change filters
- sources are located in tabs that may be splitted or float (right click on tab title to see Float option)
- supports sending and receiving packets once or periodically

## What's next
- packet analyzer - group packets by header and define structure of packets. Export them to NosSmooth packet format
- packet capture for older and custom clients - currently encryptionkey is obtained from memory so if the memory changes, that's a problem
