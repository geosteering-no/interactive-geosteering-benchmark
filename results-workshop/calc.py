import json

files = ["637085610644465872", "637085617251785212", "637085622348807819"]

userScores = {}

for fname in files:
    with open(fname) as f:
        data = json.load(f)
        best = data["BestPossible"]["TrajectoryWithScore"][-1]["Score"]
        print("best: {}".format(best))
        for user in data["UserResults"]:
            name = user["UserName"]
            score = user["TrajectoryWithScore"][-1]["Score"]
            score = score/best/3.0*100.0
            userData = userScores.get(name, [0.0, []])
            
            userData[0] = userData[0] + score
            userData[1].append(score)
            userScores[name] = userData
        
    
s = sorted(userScores.items(), key=lambda kv: kv[1][0])

for u in s:
    print(u[0] + ": {}".format(u[1]))

        