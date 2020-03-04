import calc
import time
from client_with_character import Geosteerer, run_sequential_geosteering
from multiprocessing import Pool
import threading
import random

all_results = calc.get_user_scores()
print(all_results)

url = "http://game.geosteering.no"
# url = "http://127.0.0.1"
geosteerers = list(map(lambda r: Geosteerer(r, my_url=url, verbose=False, max_games=3), all_results))

# with Pool(processes=2) as pool:
#     results = list(pool.imap_unordered(run_sequential_geosteering, geosteerers))

for g in geosteerers:
    thr1 = threading.Thread(target=g.run_sequential, kwargs={"max_pause": 1, "max_evaluations": 3})
    thr1.start()
    time.sleep(random.randint(1, 10))




#print(results)
