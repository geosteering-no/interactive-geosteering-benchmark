from sqlalchemy.orm import scoped_session

import calc

import collections
from scipy.optimize import curve_fit
import scipy
import numpy as np
from scipy import exp
import matplotlib.pyplot as plt
from matplotlib.ticker import (MultipleLocator, FormatStrFormatter,
                               AutoMinorLocator)

def several_steps_done(x):
    for traj in x.trajectories:
        if len(traj) < 6:
            return False
    return True

def custom_func(x):
    return several_steps_done(x)







def print_results(results, i = 0, do_plot=False):



    correlations = []

    for u in results:
        # games: {}  u.game_count(),

        if do_plot:
            plt.figure(i)

        score: calc.UserScore = u
        name = u.name
        trajs = score.trajectories
        for j in range(3):
            traj = trajs[j]
            xs = list(map(lambda x: x["wellPoint"]["X"], traj))
            ys = list(map(lambda x: -x["wellPoint"]["Y"], traj))
            if do_plot:
                plt.plot(xs, ys, label=str(j))

        if do_plot:
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

        norm12 = np.linalg.norm(y_vec1 - y_vec2, 1) / min_len
        norm01 = np.linalg.norm(y_vec0 - y_vec1, 1) / min_len
        norm02 = np.linalg.norm(y_vec0 - y_vec2, 1) / min_len

        all_diffs_np = np.array([norm01, norm02, norm12])
        mean_diff = np.mean(all_diffs_np)
        std_diff  = np.std(all_diffs_np)

        #corr12 = np.corrcoef(ys1, ys2)[0, 1]

        correlations.append(norm12)
        print("{:2}. {:22}: score: {:6.0f}  {:5.1f}% distance norm between trajectories; "
              "different: {:6.1f}, {:6.1f}  same: {:6.1f}      mean: {:6.1f} std: {:6.1f}"
              .format(i, u.name, u.total_score, u.total_percent_divided(), norm01, norm02, norm12, mean_diff, std_diff))

        u.all_diffs = all_diffs_np
        u.std_diff = std_diff
        u.mean_diff = mean_diff


        #plt.show()
        i += 1

results = calc.get_user_scores(filter_users_func=custom_func)
results = sorted(results, key=lambda x: x.name.lower(), reverse=False)
print("Total qualified ", len(results))
print_results(results)


def dss_test(x):
    return x.all_diffs[2] < 1e-9


def simple_consistent_test(x):
    return not dss_test(x) and x.all_diffs[2] <= 0.5


def comparatively_consistent_test(x):
    return (not dss_test(x) and not simple_consistent_test(x)) \
        and x.all_diffs[2] <= x.all_diffs[0]-x.std_diff*2 \
        and x.all_diffs[2] <= x.all_diffs[1]-x.std_diff*2


def sort_scores(results):
    return sorted(results, key=lambda x: x.total_percent_divided(), reverse=True)

def make_plot(results, start_index, label='', plot_number=None):
    if plot_number != None:
        plt.figure(plot_number)
    else:
        plt.figure()
    xs = np.array(range(start_index,start_index+len(results)))
    ys = np.array(list(map(lambda x: x.total_percent_divided(), results)))
    plt.bar(xs, ys, label=label)


plot_number = 1000



i = 0

my_fig = plt.figure(plot_number)
my_ax : plt.Axes = my_fig.add_subplot(1,1,1)
plt.xticks(range(1, len(results)+1, 2))
plt.grid(True, which='major', axis='y', linewidth='1')
plt.grid(True, which='minor', axis='y', linewidth='0.5')
my_ax.set_axisbelow(True)
# Make a plot with major ticks that are multiples of 20 and minor ticks that
# are multiples of 5.  Label major ticks with '%d' formatting but don't label
# minor ticks.
my_ax.yaxis.set_major_locator(MultipleLocator(10))
my_ax.yaxis.set_major_formatter(FormatStrFormatter('%d'))

# For the minor ticks, use no labels; default NullFormatter.
my_ax.yaxis.set_minor_locator(MultipleLocator(2))
#my_ax.tick_params('major', width=2)


dss_users = sort_scores(list(filter(dss_test, results)))
print("consistent_users")
print_results(dss_users, i+1)
make_plot(dss_users, 1, label="DSS bot user", plot_number=plot_number)

i += len(dss_users)

plt.figure(plot_number)
# a bit hard-coded
plt.bar([0.5 + len(dss_users)], [100], align='edge', color='gray', alpha=0.3,
        label='Correct layer guessed randomly',
        width=(len(results)-len(dss_users))/8)

consistent_users = sort_scores(list(filter(simple_consistent_test, results)))
print("consistent_users")
print_results(consistent_users, i+1)
make_plot(consistent_users, i+1, label="Consistent users", plot_number=plot_number)

i += len(consistent_users)

comparatively_consistent_users = sort_scores(list(filter(comparatively_consistent_test,
                                        results)))

print("comparatively_consistent_users")
print_results(comparatively_consistent_users, i+1, do_plot=True)
make_plot(comparatively_consistent_users, i+1,
          label="Relatively consistent users", plot_number=plot_number)

i += len(comparatively_consistent_users)

non_consistent_users = sort_scores(list(filter(lambda x:
                                               (not dss_test(x))
                                   and
                                   (not comparatively_consistent_test(x))
                                   and
                                   (not simple_consistent_test(x)),
                                   results)))

print("non_consistent_users")
print_results(non_consistent_users, i+1)
make_plot(non_consistent_users, i+1,
          label="Other users", plot_number=plot_number)

# line for indication of best DSS
ys = np.array(list(map(lambda x: x.total_percent_divided(), dss_users)))
plt.plot([0, len(results)], [ys[0], ys[0]])


plt.legend()

plt.show()


# compare  consistency with results



