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
        self.games = []

    def game_count(self):
        return len(self.scores)

    def total_score_divided(self):
        return self.total_score / self.game_count()

    def total_percent_divided(self):
        return self.total_score_percent / self.game_count()


userScores = {}
game_ind = 0
for fname in files:
    game_ind += 1
    with open(fname) as f:
        data = json.load(f)
        best = data["BestPossible"]["TrajectoryWithScore"][-1]["Score"]
        print("best: {}".format(best))
        for user in data["UserResults"]:
            name = user["UserName"]
            score = user["TrajectoryWithScore"][-1]["Score"]
            # if score <= 1e-2:
            #     continue
            score_percent = score/best*100.0
            userData = userScores.get(name, UserScore(name))
            
            userData.total_score += score
            userData.total_score_percent += score_percent
            userData.scores.append(score)
            userData.percents.append(score_percent)
            userData.games.append(game_ind)
            userScores[name] = userData

print('len(distinct players) = {}'.format(len(userScores)))
filtered_list = list(filter(lambda x: x.game_count() >= 3, userScores.values()))

print('len(filtered) = {}'.format(len(filtered_list)))
s = sorted(filtered_list, key=lambda x: x.total_percent_divided(), reverse=True)
#s = sorted(filtered_list, key=lambda x: x.name.lower())

i = 0
for u in s:
    i += 1
    #games: {}  u.game_count(),
    print("{:2}. {:22}: score: {:6.0f}  {:5.1f}%".format(i, u.name,u.total_score, u.total_percent_divided()))

        