import json
import collections

files = ["637085610644465872", "637085617251785212", "637085622348807819"]

class UserScore:

    def __init__(self, name):
        self.name = name
        self.total_score_percent = 0.0
        self.total_score = 0.0
        self.scores = []
        self.percents = []

    def game_count(self):
        return len(self.scores)

    def total_score_divided(self):
        return self.total_score / self.game_count()

    def total_percent_divided(self):
        return self.total_score_percent / self.game_count()


userScores = {}

for fname in files:
    with open(fname) as f:
        data = json.load(f)
        best = data["BestPossible"]["TrajectoryWithScore"][-1]["Score"]
        print("best: {}".format(best))
        for user in data["UserResults"]:
            name = user["UserName"]
            score = user["TrajectoryWithScore"][-1]["Score"]
            score_percent = score/best*100.0
            userData = userScores.get(name, UserScore(name))
            
            userData.total_score += score
            userData.total_score_percent += score_percent
            userData.scores.append(score)
            userData.percents.append(score_percent)
            userScores[name] = userData

print('len(distinct players) = {}'.format(len(userScores)))
three_games_list = list(filter(lambda x: x.game_count() >= 2, userScores.values()))

print('len(filtered) = {}'.format(len(three_games_list)))
s = sorted(three_games_list, key=lambda x: x.total_percent_divided(), reverse=True)

i = 0
for u in s:
    i += 1
    print("{:2}. {:22}: games: {} score: {:6.0f}  percent: {:5.1f}".format(i, u.name, u.game_count(), u.total_score_divided(), u.total_percent_divided()))

        