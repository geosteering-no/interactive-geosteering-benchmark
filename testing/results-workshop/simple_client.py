from urllib.request import urlopen
import requests
from datetime import datetime


def get_data(base_url, cookies):
    add_url = "/geo/userdata"
    response = requests.get(base_url + add_url, cookies=cookies)
    json_result = response.json()
    return json_result


def commit_point(base_url, cookies, well_point):
    add_url = "/geo/commitpoint"
    payload = well_point
    response = requests.post(base_url + add_url, json=payload, cookies=cookies)
    json_result = response.json()
    return json_result


def commit_stop(base_url, cookies):
    add_url = "/geo/commitstop"
    response = requests.post(base_url + add_url, cookies=cookies)
    json_result = response.json()
    return json_result


def move_to_next_game(base_url, cookies):
    add_url = "/geo/newgame"
    response = requests.post(base_url + add_url, cookies=cookies)
    json_result = response.json()
    return json_result


def request_evaluation(base_url, cookies, well_points):
    add_url = "/geo/evaluate"
    response = requests.post(base_url + add_url, cookies=cookies, json=well_points)
    json_result = response.json()
    return json_result


def log_me_in(base_url, username=None):
    """
    :returns cookies
    :param base_url:
    :return:
    """
    add_url = "/geo/init"
    if username is None:
        data = {"userName": "python " + str(datetime.now())}
    else:
        data = {"userName": username + str(datetime.now())}
    response = requests.post(base_url + add_url, data=data)
    return response.history[0].cookies


if __name__ == "__main__":
    url = "http://127.0.0.1"
    cookies = log_me_in(base_url=url)
    print("got cookies", cookies)

    for i in range(10):
        print("GET DATA")
        data = get_data(url, cookies)
        print(data)

        print("COMMIT")
        well_point_with_score = {"wellPoint":{"X":28.559424837354033,"Y":5.092764085867846,"Angle":0.17646679684123262},"Score":-2.4948553947636065}
        well_point = well_point_with_score["wellPoint"]
        data = commit_point(url, cookies, well_point)
        print(data)

        print("EVALUATE")
        well_segment = [{"X": 0.0, "Y": -0.0, "Angle": 0.17453292519943295},
                        {"X": 28.559424837354033, "Y": 5.092764085867846, "Angle": 0.17646679684123262}]
        data = request_evaluation(url, cookies, well_segment)
        print(data)

        print("STOP")
        data = commit_stop(url, cookies)
        print(data)

        print("NEW GAME")
        data = move_to_next_game(url, cookies)
        print(data)

