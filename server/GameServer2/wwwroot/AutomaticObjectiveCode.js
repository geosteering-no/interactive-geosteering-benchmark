/* 
 *  
 *  MIT License
 *  
 *  Copyright (c) 2019 NORCE Energy, Sergey Alyaev, Morten Bendiksen, Andrew Holsaeter, Sofija Ivanova
 *  Original repository: https://github.com/NORCE-Energy/geosteering-game-gui
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

/**
 * @compiler Bridge.NET 17.9.0
 */
Bridge.assembly("Demo", function ($asm, globals) {
    "use strict";

    Bridge.define("ServerObjectives.DrillingCostObjective", {
        fields: {
            /**
             * Drilling cost used in compute drilling cost deligate
             *
             * @instance
             * @public
             * @memberof ServerObjectives.DrillingCostObjective
             * @default 1.0
             * @type number
             */
            DrillingCost: 0
        },
        ctors: {
            init: function () {
                this.DrillingCost = 1.0;
            }
        },
        methods: {
            ComputeDrillingCost: function (x0, y0, x1, y1) {
                var length = ServerObjectives.Utils.Hypot(x1 - x0, y1 - y0);
                return -length * this.DrillingCost;
            }
        }
    });

    Bridge.define("ServerObjectives.RealizationData", {
        fields: {
            _yLists: null
        },
        props: {
            YLists: {
                get: function () {
                    return this._yLists;
                },
                set: function (value) {
                    this._yLists = value;
                }
            }
        },
        ctors: {
            init: function () {
                this._yLists = new (System.Collections.Generic.List$1(System.Collections.Generic.IList$1(System.Double))).ctor();
            }
        }
    });

    Bridge.define("ServerObjectives.SweetSpotObjective", {
        fields: {
            SweetSpotMult: 0,
            OtherReservoirMult: 0,
            FollowBottom: false,
            SweetSpotOffset: 0,
            SweetSpotEnd: 0
        },
        props: {
            NumPoint: 0
        },
        ctors: {
            init: function () {
                this.SweetSpotMult = 2.0;
                this.OtherReservoirMult = 1.0;
                this.FollowBottom = true;
                this.SweetSpotOffset = 0.5;
                this.SweetSpotEnd = 1.5;
            },
            ctor: function () {
                this.$initialize();
                this.NumPoint = 10;
            },
            $ctor1: function (numPoint) {
                this.$initialize();
                this.NumPoint = numPoint;
            }
        },
        methods: {
            /**
             * Function should be
             0 outside
             dReservoir value at Above value
             decaying fast below Above value
             decaying slowly above Above value
             *
             * @instance
             * @private
             * @this ServerObjectives.SweetSpotObjective
             * @memberof ServerObjectives.SweetSpotObjective
             * @param   {System.Tuple$2}    thicknessAndDistanceAbove    
             * @param   {number}            dReservoirValue
             * @return  {number}
             */
            _reservoirValueFunction: function (thicknessAndDistanceAbove, dReservoirValue) {
                var currThickness = thicknessAndDistanceAbove.Item1;
                var currAbove = thicknessAndDistanceAbove.Item2;
                if (this.FollowBottom) {
                    currAbove = currThickness - currAbove;
                }
                var positionValue = 0.0;
                if (currAbove < 0 || currAbove > currThickness) {
                    positionValue = 0.0;
                } else if (currAbove >= this.SweetSpotOffset && currThickness - currAbove >= this.SweetSpotOffset && currAbove <= this.SweetSpotEnd) {
                    positionValue = this.SweetSpotMult;
                } else {
                    positionValue = this.OtherReservoirMult;
                }
                return positionValue * currThickness * dReservoirValue;

            },
            ComputeReservoirValue: function (T, xs, model, thicknessAndDistenceAbove, x0, y0, x1, y1) {
                var dLength = (x1 - x0) / this.NumPoint;
                var sum = 0.0;
                for (var lambda = 1.0 / this.NumPoint / 2; lambda < 1.0; lambda += 1.0 / this.NumPoint) {
                    var x = x0 * lambda + x1 * (1.0 - lambda);
                    var y = x0 * lambda + x1 * (1.0 - lambda);
                    var thicknessAndDistance = thicknessAndDistenceAbove(xs, model, x, y);
                    sum += this._reservoirValueFunction(thicknessAndDistance, dLength);
                }
                return sum;
            }
        }
    });

    Bridge.define("ServerObjectives.TheServerObjective", {
        fields: {
            _drillingCostObjective: null,
            _sweetSpotObjective: null
        },
        ctors: {
            ctor: function () {
                this.$initialize();
                this._drillingCostObjective = new ServerObjectives.DrillingCostObjective();
                this._sweetSpotObjective = new ServerObjectives.SweetSpotObjective.ctor();
            }
        },
        methods: {
            TheObjective: function (xs, realization, x0, y0, x1, y1) {
                var sum = 0.0;
                sum += this._drillingCostObjective.ComputeDrillingCost(x0, y0, x1, y1);
                sum += this._sweetSpotObjective.ComputeReservoirValue(ServerObjectives.RealizationData, xs, realization, ServerObjectives.Utils.TnDRealizationData, x0, y0, x1, y1);
                return sum;
            }
        }
    });

    Bridge.define("ServerObjectives.Utils", {
        statics: {
            methods: {
                Hypot: function (x, y) {
                    return Math.sqrt(x * x + y * y);
                },
                IndexOfX: function (xPositions, x) {
                    var left = 0;
                    var right = (System.Array.getCount(xPositions, System.Double) - 1) | 0;
                    while (((right - left) | 0) > 1) {
                        var mid = (Bridge.Int.div((((right + left) | 0)), 2)) | 0;
                        if (System.Array.getItem(xPositions, mid, System.Double) > x) {
                            right = mid;
                        } else {
                            left = mid;
                        }
                    }
                    return left;
                },
                TnDRealizationData: function (xs, realization, x, z) {
                    var indexPrev = ServerObjectives.Utils.IndexOfX(xs, x);
                    var len = System.Array.getItem(xs, ((indexPrev + 1) | 0), System.Double) - System.Array.getItem(xs, indexPrev, System.Double);
                    x -= System.Array.getItem(xs, indexPrev, System.Double);
                    var lambda = x / len;

                    var zTop = 0;
                    var zBot = 0;
                    var zTop2 = 0;
                    var zBot2 = 0;
                    if (System.Array.getCount(realization.YLists, System.Collections.Generic.IList$1(System.Double)) >= 2) {
                        var upperBoundary = System.Array.getItem(realization.YLists, 0, System.Collections.Generic.IList$1(System.Double));
                        var lowerBoundary = System.Array.getItem(realization.YLists, 1, System.Collections.Generic.IList$1(System.Double));

                        zTop = System.Array.getItem(upperBoundary, indexPrev, System.Double) * (1 - lambda) + System.Array.getItem(upperBoundary, ((indexPrev + 1) | 0), System.Double) * lambda;
                        zBot = System.Array.getItem(lowerBoundary, indexPrev, System.Double) * (1 - lambda) + System.Array.getItem(lowerBoundary, ((indexPrev + 1) | 0), System.Double) * lambda;
                    }

                    if (System.Array.getCount(realization.YLists, System.Collections.Generic.IList$1(System.Double)) >= 4) {
                        var upperBoundary2 = System.Array.getItem(realization.YLists, 2, System.Collections.Generic.IList$1(System.Double));
                        var lowerBoundary2 = System.Array.getItem(realization.YLists, 3, System.Collections.Generic.IList$1(System.Double));
                        zTop2 = System.Array.getItem(upperBoundary2, indexPrev, System.Double) * (1 - lambda) + System.Array.getItem(upperBoundary2, ((indexPrev + 1) | 0), System.Double) * lambda;
                        zBot2 = System.Array.getItem(lowerBoundary2, indexPrev, System.Double) * (1 - lambda) + System.Array.getItem(lowerBoundary2, ((indexPrev + 1) | 0), System.Double) * lambda;
                    }

                    if (zBot >= zTop2) {
                        if (zTop <= z && z <= zBot2) {
                            return { Item1: zBot2 - zTop, Item2: zBot2 - z };
                        } else {
                            return { Item1: 0.0, Item2: 0.0 };
                        }
                    }
                    if (zTop <= z && z <= zBot) {
                        return { Item1: zBot - zTop, Item2: zBot - z };
                    }
                    if (zTop2 <= z && z <= zBot2) {
                        return { Item1: zBot2 - zTop2, Item2: zBot2 - z };
                    }
                    return { Item1: 0.0, Item2: 0.0 };
                }
            }
        }
    });
});
