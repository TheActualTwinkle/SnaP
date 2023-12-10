# **SnaP - poker game with [NSTU](https://en.nstu.ru/)-like graphics.**

"SnaP" -  **Shurely not a Poker**

The [rules](https://en.wikipedia.org/wiki/Texas_hold_%27em#Rules) of **Texas Hold 'em** as one of the most popular poker card game variants was used.

![JoinSample](GitImages/Menu.png)
<p align="center">
<b><i>Main Menu</i></b>
</p>

## Want to just Play? Here you go!
Visit GitHub [releases page](https://github.com/Twinkllle/PokerMultiplayer/releases) and download version for your OS. For nowadays we have only **Windows** and **Linux** standalone client and dedicated server support.

## SDT/UPnP
What the hell is **SDT**?
**SDT** is a SnaP Data Transfer system that allows us to communicate between the game *Host* and all users.

Example is simple: 
You click *host* ->
UPnP makes magic ->
Your ~~bank account~~ data was sent to to SDT Server ->
Other player can see your server in *Browse menu*

You can see status of **SDT** in the top-right corner
![[GitImages/SDT.png]]
## UPnP or Relay?
* ### Universal Plug and Play: 
	In **SnaP** we have implemented [UPnP](https://ru.wikipedia.org/wiki/UPnP)  system witch allows us to handle clients connection without the Port Forwarding.
	![IpPortInputFields](GitImages/UPnP.png)
<p align="center">
<b><i>Don`t forget to check if UPnP is enabled in router settings</i></b>
</p>

* ### Unity Relay
	In case if something went wrong with SDT/UPnP you can always use **Unity Relay**
## Dedicated server Setup Guide

	Annotation: SnaP Server default port is: 47924 (can be changed by -port argument)
1. You have to care about port forwarding (may be this [instruction](https://www.lifewire.com/how-to-port-forward-4163829) for Windows can be helpful).
2. When your port is forwarded you may start server.
* For Windows Dedicated Server run:
```bash
$ .\SnaP_Server.exe
```
* For Linux Dedicated Server build run:
```bash
$ sudo chmod +x ./SnaP_Server.x86_64
$ ./SnaP_Server.x86_64
```
If you forwarded different from *47924* port then you can add the `-port` argument when starting game.
```bash
$ ./SnaP_Server.x86_64 -port 12345
```

You will notified about the successful start of the server:
```bash 
=============================================================
STARTING AT: <LOCAL_IP:PORT>
=============================================================
Forwarding to public IP...
# ...Some...
# ...Unity...
# ...Logs here...
=============================================================
Successfully started at <PUBLIC_IP:PORT>
=============================================================
```
Or about start failure:
```bash
=============================================================
STARTING AT: <LOCAL_IP:PORT>
=============================================================
Failed to start at <LOCAL_IP:PORT>. Attempt 1/4
Failed to start at <LOCAL_IP:PORT>. Attempt 2/4
Failed to start at <LOCAL_IP:PORT>. Attempt 3/4
Failed to start at <LOCAL_IP:PORT>. Attempt 4/4
Connection timeout. Shuts-downing in 3000 milliseconds.
```
## **Services implemented:**
  * [Netcode for GameObjects](https://unity.com/products/netcode) 
    >This library is used to synchronize GameObject and game state data across all clients that connect in a networking session.
  * [UnityTransport](https://docs-multiplayer.unity3d.com/transport/current/about/index.html)
    >Unity Transport provides the `com.unity.transport` package, used to add multiplayer and network features to your project.  
  *  [UPnP](https://ru.wikipedia.org/wiki/UPnP) 
	>This set of protocols allows us to avoid Port Forwarding
  * [Unity Gaming Services (Relay)](https://unity.com/solutions/gaming-services)
    >These services make it easy for players to host and join games that are playable over the internet, without the need for port forwarding or out-of-game coordination.
  * [ParrelSync](https://github.com/VeriorPies/ParrelSync)
	>ParrelSync is a Unity editor extension that allows users to test multiplayer gameplay without building the project by having another Unity editor window opened and mirror the changes from the original project.
## **Requirements**:
 - Unity Version [**2021.3**](https://unity3d.com/get-unity/download) or newer
## **Resources:**
* [Holdem Combination Сalculator](https://github.com/ccqi/TexasHoldem)
* [Standalone File Browser](https://github.com/gkngkc/UnityStandaloneFileBrowser)
* [Open.NAT lib](https://github.com/lontivero/Open.NAT)
* [UI Scrollable Grid](https://assetstore.unity.com/packages/tools/gui/recyclable-scroll-rect-optimized-list-grid-view-178560)
* [UPnP Extentions](https://assetstore.unity.com/packages/tools/network/basic-upnp-220149)
* All Sprites and UI:
	* [VK](https://vk.com/id607494051)
	* [VK Public](https://vk.com/preved_medveddd)
## Feedback:
* Email: theactualtwinkle@gmail.com
* Telegram: https://t.me/TheActualTwinkle
