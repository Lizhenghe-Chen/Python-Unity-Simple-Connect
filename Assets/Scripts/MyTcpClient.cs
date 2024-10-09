using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Events;
using Random = UnityEngine.Random; // 需要导入 Newtonsoft.Json 库

// 自定义的数据结构
[Serializable]
public class SendData
{
    public string command;
    public int value;
    public Vector3 position; // 包含 Unity 的 Vector3 类型
}

// 服务器响应类
[Serializable]
public class ServerResponse
{
    public string message;
    public string status;
    public Vector3 position;
}

//ServerResponse event 
public class ServerResponseEvent : UnityEvent<ServerResponse>
{
}

public class MyTcpClient : MonoBehaviour
{
    public int port = 65432;

    [SerializeField] private CubeMover cubeMover;
    private TcpClient _socketConnection;
    private NetworkStream _stream;
    private readonly ServerResponseEvent _serverResponseEvent = new();
    private int _retryCount;

    private void Start()
    {
        ConnectToServer();
        _serverResponseEvent.AddListener((response) =>
        {
            // 更新位置
            cubeMover.MoveCube(response.position);
            // 向服务器发送新的位置
            StartCoroutine(DelaySendCubePosition());
        });
    }

    private void Update()
    {
        // 按下空格时发送 Vector3 数据
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DelaySendCubePosition(0));
        }
    }

    private IEnumerator DelaySendCubePosition(float delay = .1f)
    {
        yield return new WaitForSeconds(delay);
        var data = new SendData
        {
            command = "SendPosition",
            value = Random.Range(0, 100),
            position = cubeMover.transform.position
        };
        SendToServer(data);
    }

    private void SendToServer(SendData dataToSend)
    {
        SendDataToServer(dataToSend);
    }


    // 连接服务器
    private void ConnectToServer()
    {
        try
        {
            _socketConnection = new TcpClient("127.0.0.1", port);
            _stream = _socketConnection.GetStream();
            Debug.Log("Connected to Python server");

            // 开始监听数据的协程
            StartCoroutine(ListenForData());
        }
        catch (Exception e)
        {
            _retryCount++;
            if (_retryCount < 5)
            {
                Debug.LogError("Error connecting to server: " + e.Message);
                Invoke(nameof(ConnectToServer), 1f);
            }
            else
            {
                Debug.LogError("Failed to connect to server after 5 retries.");
            }
        }
    }

    // 发送数据到服务器
    private void SendDataToServer(SendData sendData)
    {
        if (_socketConnection == null || !_socketConnection.Connected) return;

        try
        {
            if (_stream.CanWrite)
            {
                // 自定义序列化：将 Vector3 转换为一个包含 x, y, z 的 JSON 结构
                var dataToSend = new
                {
                    sendData.command,
                    sendData.value,
                    position = new { x = sendData.position.x, y = sendData.position.y, z = sendData.position.z }
                };

                // 将对象序列化为 JSON 字符串
                string jsonData = JsonConvert.SerializeObject(dataToSend);
                byte[] data = Encoding.ASCII.GetBytes(jsonData);
                _stream.Write(data, 0, data.Length);
                Debug.Log("<color=blue>Sent to server:</color> " + jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data: " + e.Message);
        }
    }

    // 协程方式接收服务器的数据
    private IEnumerator ListenForData()
    {
        byte[] data = new byte[1024];
        while (_socketConnection != null && _socketConnection.Connected)
        {
            if (_stream.CanRead)
            {
                // 开始异步读取数据
                IAsyncResult result = _stream.BeginRead(data, 0, data.Length, null, null);

                // 等待读取完成
                while (!result.IsCompleted)
                {
                    yield return null; // 每帧检查
                }

                int bytesRead = _stream.EndRead(result);
                if (bytesRead > 0)
                {
                    string jsonResponse = Encoding.ASCII.GetString(data, 0, bytesRead);
                    Debug.Log("Received from server: " + jsonResponse);

                    // 使用 JsonConvert 反序列化为 ServerResponse 对象
                    ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(jsonResponse);
                    Debug.Log("<color=green>Server response:</color> " + serverResponse.position);


                    _serverResponseEvent.Invoke(serverResponse);
                }
            }

            yield return null; // 每帧继续监听
        }
    }

    // 关闭连接
    private void OnDestroy()
    {
        _socketConnection?.Close();
    }
}