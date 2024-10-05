import os
import json
import subprocess
from http.server import BaseHTTPRequestHandler, HTTPServer

moirai_cli_path = r"C:\Users\VIP\source\repos\Tanks3DServer\MoiraiAPI\moirai-cli.exe"
moirai_conf_path = r"C:\Users\VIP\AppData\Roaming\Moirai\moirai.conf"

class MoiraiApiServer(BaseHTTPRequestHandler):

    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        request_data = json.loads(post_data)

        try:
            method = request_data.get("method")
            parameters = request_data.get("params", [])

            result = self.execute_moirai_command(method, parameters)
            response = json.dumps({"result": result})

        except Exception as e:
            response = json.dumps({"error": str(e)})

        self._send_response(response)

    def execute_moirai_command(self, method, parameters):
        command = [moirai_cli_path, f"-conf={moirai_conf_path}", method]
        command.extend(parameters)
        print(method)
        process = subprocess.run(command, capture_output=True, text=True)
        stdout, stderr = process.communicate()

        if process.returncode != 0:
            raise Exception(f"Error executing command: {stderr.decode('utf-8')}")

        print(stdout.decode('utf-8'))

        return stdout.decode('utf-8')

    def _send_response(self, response):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        self.wfile.write(response.encode('utf-8'))


def run(server_class=HTTPServer, handler_class=MoiraiApiServer, port=5000):
    server_address = ('127.0.0.1', port)
    httpd = server_class(server_address, handler_class)
    print(f'Starting server on http://127.0.0.1:{port}...')
    httpd.serve_forever()


if __name__ == '__main__':
    run()