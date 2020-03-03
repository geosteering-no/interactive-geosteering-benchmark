import calc
import random
import time
from datetime import datetime
from trajectory_distribution import custom_func
from simple_client import *



def run_sequential_geosteering(self, max_pause=0, max_evaluations=1):
    """

    :return:
    """
    if self.cookies is None:
        self.initialize_user()

    for i in range(self.max_games):

        data = get_data(self.url, self.cookies)
        if self.verbose:
            print("GET DATA")
            print(data)

        time.sleep(random.random() * max_pause)

        replay_index = i % len(self.trajectories)
        if self.verbose:
            print("replay index ", replay_index)

        for j in range(1, len(self.trajectories[replay_index])):
            point = self.trajectories[replay_index][j]
            data = commit_point(self.url, self.cookies, point)
            if self.verbose:
                print("COMMIT")
                print(data)

            time.sleep(random.random() * max_pause)

            for k in range(1, random.randint(max_evaluations)):
                data = request_evaluation(self.url, self.cookies, self.trajectories[replay_index])
                if self.verbose:
                    print("EVALUATE")
                    print(data)
            # TODO add a random operation here from time to time
            time.sleep(random.random() * max_pause / (max_evaluations + 1))

        time.sleep(random.random() * max_pause)

        data = commit_stop(self.url, self.cookies)
        if self.verbose:
            print("STOP")
            print(data)

        time.sleep(random.random() * max_pause)

        data = move_to_next_game(self.url, self.cookies)
        if self.verbose:
            print("NEW GAME")
            print(data)

        time.sleep(random.random() * max_pause)

    return data


class Geosteerer:

    def __init__(self, historical_user_result=None, verbose=False, my_url="http://game.geosteering.no"):
        self.user_result = historical_user_result
        self.user_name = historical_user_result.name
        self.traj_with_scores = historical_user_result.trajectories
        self.trajectories = self._generate_trajectories()
        self.cookies = None
        self.verbose = verbose
        self.url = my_url
        self.max_games = random.randint(len(self.trajectories), 50)

    def _generate_trajectories(self):
        trajectories = []
        for tr in self.traj_with_scores:
            removed_score = list(map(lambda pt: pt["wellPoint"], tr))
            trajectories.append(removed_score)
        return trajectories

    def initialize_user(self):
        self.cookies = log_me_in(base_url=self.url, username=self.user_name)
        if self.verbose:
            print("got cookies", self.cookies)

    def run_sequential(self, max_pause=0, max_evaluations=1):
        """

        :return:
        """
        if self.cookies is None:
            self.initialize_user()

        for i in range(self.max_games):

            data = get_data(self.url, self.cookies)
            if self.verbose:
                print("GET DATA")
                print(data)

            time.sleep(random.random()*max_pause)

            replay_index = i % len(self.trajectories)
            if self.verbose:
                print("replay index ", replay_index)

            for j in range(1, len(self.trajectories[replay_index])):
                point = self.trajectories[replay_index][j]
                data = commit_point(self.url, self.cookies, point)
                if self.verbose:
                    print("COMMIT")
                    print(data)

                time.sleep(random.random() * max_pause)

                for k in range(max(1, random.randint(0, max_evaluations))):
                    data = request_evaluation(self.url, self.cookies, self.trajectories[replay_index])
                    if self.verbose:
                        print("EVALUATE")
                        print(data)
                # TODO add a random operation here from time to time
                time.sleep(random.random() * max_pause / (max_evaluations + 1))

            time.sleep(random.random()*max_pause)

            data = commit_stop(self.url, self.cookies)
            if self.verbose:
                print("STOP")
                print(data)

            time.sleep(random.random()*max_pause)

            data = move_to_next_game(self.url, self.cookies)
            if self.verbose:
                print("NEW GAME")
                print(data)

            time.sleep(random.random()*max_pause)

        return data


    def run_n_wait(self):
        """

        :return:
        """


if __name__ == "__main__":
    all_results = calc.get_user_scores(filter_users_func=custom_func)
    print(all_results)
    result = all_results[0]
    geos0 = Geosteerer(my_url="http://127.0.0.1", verbose=True, historical_user_result=result)
    geos0.run_sequential()


