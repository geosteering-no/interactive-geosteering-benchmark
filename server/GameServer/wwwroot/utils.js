var canvasWidth;
var canvasHeigth;
var oneMarginInScript = 16;

var geoModelBuffer;
var wellBuffer;
var barBuffer;
var realizationScores;
var percentileBins = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];


//bins is an array of percentiles (part * 100)
function drawBarChartsToBufferWithShift(aUserEvaluation, buffer, min, max, shiftFirst) {
    var shift = buffer.width / 10 / 5;
    var start = 0;
    var end = 0;
    var scores = aUserEvaluation.realizationScores;
    var sortedInds = aUserEvaluation.sortedIndexes;

    //TODO do negative
    for (var i = 0; i < percentileBins.length; i++) {
        //we subtract 1 so that for P10 for 100 it is based n index 9
        end = Math.floor(percentileBins[i] * (scores.length - 1) / 100.0);
        console.log("start: " + start);
        console.log("end: " + end);

        var scoreInd = sortedInds[end];
        var score = Math.max(scores[scoreInd], 0.0);
        console.log("score: " + score);

        buffer.rect(
            i / 10 * buffer.width + shift + shiftFirst,
            (max - score) / max * buffer.height,
            0.1 * buffer.width - shift * 2,
            buffer.height);
        start = end;
    }
    console.log("done");

}

function drawBarCharts() {
    //var userEvaluationOld; //from another file
    //var userEvaluation; //from another file
    barBuffer.clear();
    barBuffer.background(0, 0, 0);

    barBuffer.fill(255, 0, 0);
    var max = 1.0;
    if (userEvaluationOld != null){
        max = Math.max.apply(null, userEvaluationOld.realizationScores);
    }
    if (userEvaluation != null){
        var newMax = Math.max.apply(null, userEvaluation.realizationScores);
        max = Math.max(max, newMax);
    }
    if (userEvaluationOld != null) {
        //barBuffer.scale(barBuffer.height/max, 1.0/barBuffer.width);
        barBuffer.fill(90, 90, 90);
        barBuffer.noStroke();

        var offset = barBuffer.width / 10 / 7;
        //TODO plot old values instead
        drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset);
    }
    if (userEvaluation != null){
        barBuffer.fill(20, 50, 255);
        barBuffer.strokeWeight(1);
        barBuffer.stroke(255, 255, 255);
        drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0);
    }
}

function drawUserWell(wellBuffer, committedPoints) {
    for (var i = 0; i < committedPoints.length; i++) {
        var point = committedPoints[i];
        var prev = point;
        if (i > 0) prev = committedPoints[i - 1];
        wellBuffer.line(
            point.x,
            point.y,
            prev.x,
            prev.y);
        //circlecircle()
    }
}

function redrawEnabledForAninterval() {
    loop();
    timerCountdown = 30;
}


function drawFrame() {
    noFill();
    strokeWeight(4);
    stroke(51, 255, 10);
    rect(0, 0, width, height);
}

function drawGeomodelToBuffer(userdata = null, specificIndices = null) {
    var t0 = performance.now();
    geoModelBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
    wellBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
    barBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 2);



    if (userdata != null) {
        scaleBufferForView(wellBuffer);
        console.log("scaled");
    }

    geoModelBuffer.background(0, 0, 0);
    geoModelBuffer.blendMode(ADD);
    geoModelBuffer.strokeWeight(1);


    if (userdata != null) {
        //if (false){
        scaleBufferForView(geoModelBuffer);
        console.log("drawing userdat");
        var reals = userdata.realizations;
        var realcount = reals.length;
        if (specificIndices != null) realcount = specificIndices.length;
        var alpha = 1.0 / realcount;
        //TODO this formula needs improvement
        //var alpha = 2 * (1.0 - Math.pow(0.5, 2 / reals.length));
        geoModelBuffer.noStroke();
        geoModelBuffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
        var xlist = userdata.xList;
        if (specificIndices == null) {
            for (var reali = 0; reali < reals.length; reali++) {
                drawRealizationToBuffer(geoModelBuffer, xlist, reals[reali]);
            }
        } else {
            for (var reali in specificIndices) {
                drawRealizationToBuffer(geoModelBuffer, xlist, reals[reali]);
            }
        }
        
        //updateBars();

        // var layerBuffer = createGraphics(geoModelBuffer.width, geoModelBuffer.height);
        // scaleBufferForView(layerBuffer);
        // layerBuffer.stroke('rgb(100%, 100%, 100%)');
        // layerBuffer.fill('rgb(100%, 100%, 100%)');
        // //console.log("guess:" + reali);
        // var polyCount = reals[reali].yLists.length / 2;
        // for (var polygoni = 0; polygoni < polyCount; polygoni++) {
        //   //console.log("poly:" + polygoni);
        //   var polytop = reals[reali].yLists[polygoni * 2];
        //   var polybottom = reals[reali].yLists[polygoni * 2 + 1];

        //   layerBuffer.beginShape();
        //   for (var vertexi = 0; vertexi < polytop.length; vertexi++) {
        //     var y = polytop[vertexi];
        //     layerBuffer.vertex(xlist[vertexi], y);
        //   }

        //   for (var vertexi = polybottom.length - 1; vertexi >= 0; vertexi--) {
        //     var y = polybottom[vertexi];
        //     layerBuffer.vertex(xlist[vertexi], y);
        //   }
        //   layerBuffer.endShape(CLOSE);
        // }
        // geoModelBuffer.tint(255, alpha);
        // geoModelBuffer.image(layerBuffer, 0, 0, layerBuffer.width, layerBuffer.heigth);


        //tint(255, 255);
    } else {
        console.log("drawing triangles");
        // draw triangles for debug
        //TODO check colors again
        var points = 3;
        var shapes = 256;
        //var fixColor = 0.8;
        var alpha = 1 / (shapes);
        //var alpha = 1.0 - Math.pow(0.5, 2 / shapes);
        //var alpha = 2.71/shapes;
        geoModelBuffer.noStroke();
        //geoModelBuffer.stroke('rgba(100%, 100%, 100%, ' + alpha + ')');
        geoModelBuffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');

        var rotate = TWO_PI / points / 10;
        geoModelBuffer.translate(geoModelBuffer.width / 2, geoModelBuffer.height / 2)

        for (var i = 0; i < shapes; i++) {
            geoModelBuffer.rotate(rotate);
            drawCircle(geoModelBuffer, 0, 0, geoModelBuffer.height / 2, points);
        }
    }
    var t1 = performance.now();
    console.log("draw geomodel to buffer " + (t1 - t0) + " milliseconds.");
}


function drawRealizationToBuffer(buffer, xlist, real) {
    var polyCount = real.yLists.length / 2;
    for (var polygoni = 0; polygoni < polyCount; polygoni++) {
        //console.log("poly:" + polygoni);
        var polytop = real.yLists[polygoni * 2];
        var polybottom = real.yLists[polygoni * 2 + 1];
        //TODO do shape intersection
        buffer.beginShape();
        for (var vertexi = 0; vertexi < polytop.length; vertexi++) {
            var y = polytop[vertexi];
            buffer.vertex(xlist[vertexi], y);
        }

        for (var vertexi = polybottom.length - 1; vertexi >= 0; vertexi--) {
            var y = polybottom[vertexi];
            buffer.vertex(xlist[vertexi], y);
        }
        buffer.endShape(CLOSE);
    }
}

function drawCircle(buffer, x, y, radius, npoints) {

    var angle = TWO_PI / npoints;
    buffer.beginShape();
    for (let a = 0; a < TWO_PI; a += angle) {
        let sx = x + cos(a) * radius;
        let sy = y + sin(a) * radius;
        buffer.vertex(sx, sy);
    }
    buffer.endShape(CLOSE);

}

