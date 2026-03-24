import socket
import json
import time

def start_server(host='127.0.0.1', port=64574):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    
    try:
        server_socket.bind((host, port))
        server_socket.listen(1)
        print(f"Server listening on {host}:{port}")
    except Exception as e:
        print(f"Failed to bind: {e}")
        return

    try:
        while True:
            print("Waiting for connection...")
            conn, addr = server_socket.accept()
            print(f"Connected by {addr}")
            
            with conn:
                buffer = ""
                while True:
                    try:
                        data = conn.recv(4096)
                        if not data: break
                        
                        buffer += data.decode('utf-8', errors='ignore')
                        while '\n' in buffer:
                            line, buffer = buffer.split('\n', 1)
                            line = line.strip()
                            if not line: continue
                            
                            try:
                                json_data = json.loads(line)
                                
                                # 关键修复：确保解析结果是字典
                                if not isinstance(json_data, dict):
                                    continue
                                
                                print("-" * 30)
                                
                                health = json_data.get('health')
                                if health is not None:
                                    print(f"Health: {health:.4f}")
                                    
                                next_hit = json_data.get('nextHit')
                                if isinstance(next_hit, dict):
                                    # 安全获取所有字段
                                    o_type = next_hit.get('type', 'unknown')
                                    x = next_hit.get('x', 0)
                                    y = next_hit.get('y', 0)
                                    dt = next_hit.get('dt', 0)
                                    print(f"Next ({o_type}): Pos({x:.1f}, {y:.1f}) dt: {dt:.1f}ms")
                                    
                                mouse = json_data.get('mouse')
                                if isinstance(mouse, list) and len(mouse) >= 2:
                                    print(f"Mouse: ({mouse[0]:.1f}, {mouse[1]:.1f})")

                            except json.JSONDecodeError:
                                pass
                    except Exception as e:
                        print(f"Connection error: {e}")
                        break
    except KeyboardInterrupt:
        print("\nStopping...")
    finally:
        server_socket.close()

if __name__ == '__main__':
    start_server()
