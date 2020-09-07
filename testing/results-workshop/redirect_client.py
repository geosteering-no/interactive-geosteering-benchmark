import http.server
import socketserver


class MyHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        print(self.path)
        self.send_response(301)
        new_path = '%s%s' % ('http://game.geosteering.no', self.path)
        self.send_header('Location', new_path)
        self.end_headers()


PORT = 7000
handler = socketserver.TCPServer(("", PORT), MyHandler)
print("serving at port {}".format(PORT))
handler.serve_forever()