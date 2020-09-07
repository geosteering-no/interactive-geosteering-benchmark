import http.server
import socketserver
import logging
from simple_client import *

origin = 'http://game.geosteering.no'

class MyHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        logging.info(self.headers)
        print('Path: {}'.format(self.path))
        if self.path.startswith('/geo'):
            print('Redirecting')
            self.send_response(301)
            self.send_header('Location', 'http://game.geosteering.no')
            self.end_headers()
            #     == '/up':
            # self.send_response(200)
            # self.end_headers()
            # self.wfile.write(b'up')
        else:
            self.path = 'wwwroot/' + self.path
            super().do_GET()
        # return
        # print(self.path)
        # self.send_response(301)
        # new_path = '%s%s' % ('http://game.geosteering.no', self.path)
        # self.send_header('Location', new_path)
        # self.end_headers()

    def do_POST(self):
        if self.path.startswith('/geo'):
            print('posting in geo')
            self.path = 'wwwroot/index.html'
            super().do_GET()


PORT = 12000
handler = socketserver.TCPServer(("", PORT), MyHandler)
print("serving at port {}".format(PORT))
handler.serve_forever()
