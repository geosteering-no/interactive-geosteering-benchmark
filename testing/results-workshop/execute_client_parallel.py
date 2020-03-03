import calc
from client_with_character import Geosteerer

all_results = calc.get_user_scores()
print(all_results)
result = all_results[0]
geos0 = Geosteerer(my_url="http://127.0.0.1", verbose=True, historical_user_result=result)
geos0.run_sequential()