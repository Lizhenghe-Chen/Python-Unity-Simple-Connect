import socket
import json
import random

# 服务器设置
HOST = "127.0.0.1"
PORT = 65432
recieved_Position = {"x": 0, "y": 0, "z": 0}


def ProcessRawData(data):
    # 反序列化接收到的 JSON 数据
    received_data = json.loads(data.decode())

    # 检查是否包含 Vector3 数据
    if "position" in received_data:
        recieved_Position["x"] = received_data["position"]["x"]
        recieved_Position["y"] = received_data["position"]["y"]
        recieved_Position["z"] = received_data["position"]["z"]

    return data


def SendResponse(response_data):
    # 构建返回消息
    # add random number to x
    x = recieved_Position["x"] + random.randint(-1, 1)
    y = recieved_Position["y"]
    z = recieved_Position["z"] + random.randint(-1, 1)
    response_data = {
        "message": "Vector3 received",
        "position": {"x": x, "y": y, "z": z},
        "status": "ok",
    }
    # 序列化为 JSON 字符串并发送回客户端
    response_json = json.dumps(response_data)
    conn.sendall(response_json.encode())
    print(
        f"Received Vector3 from Unity: x={round(recieved_Position['x'], 2)}, y={round(recieved_Position['y'], 2)}, z={round(recieved_Position['z'], 2)} | "
        f"send back to Unity: x={round(x, 2)}, y={round(y, 2)}, z={round(z, 2)}"
    )


# 创建 TCP 服务器
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()
    print(f"Server started. Listening on {HOST}:{PORT}")

    conn, addr = s.accept()
    with conn:
        print(f"Connected by {addr}")
        while True:
            data = conn.recv(1024)
            if not data:
                break
            ProcessRawData(data)
            SendResponse(data)
