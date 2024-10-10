# Python Unity Simple Connect

> Use a super simple way to connect Python with Unity

## 前言

Unity和Python或多或少都有需要互相连接的场合，比如需要Python实现一些Unity内部的数据分析，甚至是用Python驱动Unity内的数字角色等，现有应该有几种常见方式：

1. 通过内存共享实现（应该是通过特定的DLL来打通）、技术要求高一些，自己也没具体了解过。
2. 通过[NuGet Gallery | pythonnet 3.0.4](https://www.nuget.org/packages/pythonnet)实现C#内部运行Python代码，案例有：[shiena/Unity-PythonNet (github.com)](https://github.com/shiena/Unity-PythonNet)，但是看起来案例和灵活度（如平台兼容性）较低。
3. 通过网络端口实现（TCP/UDP）两端的通讯，这样两者之间相互独立，灵活度高。

这里采用第三种方法，现通过非常简单的方式实现比较高效灵活的Python+Unity对接。
**这里将实现一个简单的TCP通讯: Python作为服务器，Unity作为客户端，Unity向Python发送方块的实时位置，Python接收并更新方块的位置给Unity。**

## 实现

整个项目其实就只需要两个脚本，一个[Python脚本](Assets/Scripts/TCPServer.py)作为服务器，一个[C#脚本](Assets/Scripts/MyTcpClient.cs)作为客户端。
运行Python脚本后，再启动Unity，Unity会自动自动开始发送信息。Python 服务器接收到信息后会处理并重新发送信息给Unity，Unity收到后在发送新的信息，从而进入一个死循环，Unity中的方块将会持续的被Python控制。该项目将会实现Python和Unity的持续通讯。

结果如图：


![1728551675717](image/README/1728551675717.gif)
