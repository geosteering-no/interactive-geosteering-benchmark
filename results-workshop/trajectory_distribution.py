from sqlalchemy.orm import scoped_session

import calc

import collections
from scipy.optimize import curve_fit
import scipy
import numpy as np
from scipy import exp
import matplotlib.pyplot as plt

def several_steps_done(x):
    for traj in x.trajectories:
        if len(traj) < 6:
            return False
    return True

def custom_func(x):
    return several_steps_done(x)

results = calc.get_user_scores(filter_users_func=custom_func)
results = sorted(results, key=lambda x: x.name.lower(), reverse=False)

print("Total qualified ", len(results))

i = 0
correlations = []

for u in results:
    # games: {}  u.game_count(),

    score: calc.UserScore = u
    name = u.name
    trajs = score.trajectories
    for j in range(3):
        traj = trajs[j]
        xs = list(map(lambda x: x["wellPoint"]["X"], traj))
        ys = list(map(lambda x: -x["wellPoint"]["Y"], traj))
        plt.plot(xs, ys, label=str(j))
    plt.axis([0, 350, -40, 0])
    plt.legend()
    plt.title(name)

    # computing correlations
    ys0 = list(map(lambda x: x["wellPoint"]["Y"], trajs[0]))
    ys1 = list(map(lambda x: x["wellPoint"]["Y"], trajs[1]))
    ys2 = list(map(lambda x: x["wellPoint"]["Y"], trajs[2]))
    while len(ys1) < len(ys2):
        ys1.append(0.0)
    while len(ys1) > len(ys2):
        ys2.append(0.0)
    min_len = min(len(ys1), len(ys2))

    while len(ys0) < len(ys2):
        ys0.append(0.0)
    y_vec0 = scipy.asarray(ys0[0:min_len])
    y_vec1 = scipy.asarray(ys1[0:min_len])
    y_vec2 = scipy.asarray(ys2[0:min_len])

    norm12 = np.linalg.norm(y_vec1-y_vec2, 1)/min_len
    norm01 = np.linalg.norm(y_vec0-y_vec1, 1)/min_len

    #corr12 = np.corrcoef(ys1, ys2)[0, 1]

    correlations.append(norm12)
    print("{:2}. {:22}: score: {:6.0f}  {:5.1f}% distance norm between trajectories; "
          "different: {:6.1f} same: {:6.1f}"
          .format(i, u.name, u.total_score, u.total_percent_divided(), norm01, norm12))

    # plt.show()
    i += 1



