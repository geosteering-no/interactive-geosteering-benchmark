import seaborn as sns
import matplotlib.pyplot as plt
import numpy as np
from scipy.stats import norm


x = np.random.random_integers(0, 100, 100) / 100.0

with open("percentiles.txt") as file:  # Use file to refer to the file object
    all_lines = file.readlines()
    percentile_list = np.array(list(filter(lambda x: x > 0, map(lambda s: round(float(s.strip()), 3)*100, all_lines))))
    print(percentile_list)
    # sns.distplot(percentile_list)
    # ax = sns.distplot(percentile_list)
    ax = sns.distplot(percentile_list, fit=norm)
    # ax.set_xlim([0, 1])
    # ax.set_ylim([0, 0.1])
    # plt.xlim(0, 1)
    # ax = sns.kdeplot(percentile_list)
    plt.savefig("result_plots/predictions.pdf")
    plt.show()
