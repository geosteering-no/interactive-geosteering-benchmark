import http.server
import socketserver
import logging
import simplejson
from simple_client import *


origin = 'http://game.geosteering.no'
my_cookies = log_me_in(base_url=origin)

class MyHandler(http.server.SimpleHTTPRequestHandler):

    # self.cookies
    def __init__(self, *args, directory=None, **kwargs):
        super().__init__(*args, directory=None, **kwargs)
        # self.my_cookies = None

    def _write_result(self, result):
        self.send_response(200)
        self.wfile.write(result)
        print(result)
        self.end_headers()


    def do_GET(self):
        logging.info(self.headers)
        print('Path: {}'.format(self.path))
        # if self.path == '' or self.path == '/':
        #     self.path = 'wwwroot/login.html'
        #     super().do_GET()
        if self.path.startswith('/geo'):
            print('Redirecting get to origin')
            print('Path: {}'.format(self.path))
            # if self.path.startswith('/geo/init'):
            if self.path == "/geo/evaluate":
                result = request_evaluation(origin, my_cookies)
                self._write_result(result)
                return
            if self.path == "/geo/userdata":
                result = get_data(origin, my_cookies)
                print("got result: {}".format(result))
                self._write_result(result)
                print("Wrote-up")
                return


            print('Should not come here! Get: {}'.format(self.path))
            self.send_response(404)
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
        content_length = int(self.headers['Content-Length'])  # <--- Gets the size of data
        post_data = str(self.rfile.read(content_length))  # <--- Gets the data itself
        # test_data = simplejson.loads(post_data)
        print('post_data {}'.format(post_data))
        # print('user_name {}'.format(test_data))
        if self.path.startswith('/geo'):
            if self.path.startswith('/geo'):
                print('Redirecting post to origin')
                if self.path.startswith('/geo/init'):
                    my_cookies = log_me_in(origin, post_data)
                    print('cookies obtained '.format(my_cookies))
                    self.send_response(301)
                    # new_path = '%s%s' % ('/index.html', self.path)
                    self.send_header('Location', '/index.html')
                    self.end_headers()


PORT = 10000
handler = socketserver.TCPServer(("", PORT), MyHandler)
print("serving at port {}".format(PORT))
handler.serve_forever()
