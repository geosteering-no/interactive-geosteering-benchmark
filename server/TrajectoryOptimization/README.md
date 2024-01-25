# Automated decision-support-system (DSS) bot implementation

The DSS bot combines a global discrete dynamic programming algorithm with robust optimization for the immediate decision.
It was introduced in [Alyaev et al. (2019)](https://doi.org/10.1016/j.petrol.2019.106381):

"At every decision point, a discrete dynamic programming algorithm computes all potential well trajectories for the entire drilling operation and the corresponding value of the well for each realization. Then, the DSS considers all immediate alternatives (continue/steer/stop) and chooses the one that gives the best-predicted value across the realizations. This approach works for a variety of objectives and constraints and suggests reproducible decisions under uncertainty. Moreover, it has real-time performance."

Read Alyaev et al. (2019) for details:

Sergey Alyaev, Erich Suter, Reider Brumer Bratvold, Aojie Hong, Xiaodong Luo, Kristian Fossum,
**A decision support system for multi-target geosteering**,
*Journal of Petroleum Science and Engineering, Volume 183,*
2019,
106381,
ISSN 0920-4105,
https://doi.org/10.1016/j.petrol.2019.106381
