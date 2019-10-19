var canvasWidth;
var canvasHeigth;
var wellBufferX;
var wellBufferY;
var barBufferX;
var barBufferY;
var oneMarginInScript = 2;

var geoModelBuffer;
var wellBuffer;
var barBuffer;
var realizationScores;
var percentileBins = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
var maxTortalAngles = 10;

function getIncluciveIndexEndForPercentile(percentile, len){
    return Math.floor(percentileBins[percentile] * (len - 1) / 100.0);
}

//bins is an array of percentiles (part * 100)
function drawBarChartsToBufferWithShift(aUserEvaluation, buffer, min, max, shiftFirst, drawSubLines = false, drawLabels = false) {
    var binsLen = percentileBins.length;
    var shift = buffer.width / binsLen / 5;
    var start = 0;
    var end = 0;
    var scores = aUserEvaluation.realizationScores;
    var sortedInds = aUserEvaluation.sortedIndexes;
    var barMaxHeight = buffer.height * 0.8;
    //TODO do negative
    for (var i = 0; i < binsLen; i++) {

        //we subtract 1 so that for P10 for 100 it is based n index 9
        end = getIncluciveIndexEndForPercentile(i, scores.length);
        //console.log("start: " + start);
        //console.log("end: " + end);
        if (!drawSubLines) {
            var scoreInd = sortedInds[end];
            var score = Math.max(scores[scoreInd], 0.0);
            var totalWidth = 1.0 / binsLen * buffer.width - shift * 2;
            var currentYtop = (max - score) / max * barMaxHeight;
            var currentHeigth = barMaxHeight - currentYtop;
            //console.log("score: " + score);
            var xLeft = i / binsLen * buffer.width + shift + shiftFirst;

            if (barTouched == i) {
                console.log("draw background bar: " + i);
                buffer.fill(255, 137, 10);
                buffer.rect(
                    xLeft-shift, 0, 1.0 / binsLen * buffer.width, barMaxHeight
                );
                barBuffer.fill(90, 90, 90);
            }

            buffer.rect(
                xLeft,
                currentYtop,
                totalWidth,
                currentHeigth);
            
            if (drawLabels){
                var textHeight = buffer.height - barMaxHeight;
                var score = Math.max(scores[scoreInd], 0.0);
                var pInd = end+1;
                var scoretext = "P" + pInd + "\n" + Math.round(score);
                if (pInd >= sortedInds.length){
                    scoretext = "max\n" + Math.round(score);
                }

                buffer.textAlign(CENTER, TOP);
                buffer.strokeWeight(0);
                buffer.text(
                    scoretext, 
                    xLeft,
                    barMaxHeight+4,
                    totalWidth,
                    textHeight*2);

                buffer.strokeWeight(1);
            }
                
        
        }
        else {
            var singleWidth = (1.0 / binsLen * buffer.width - shift * 2) / (end + 1 - start);
            var totalWidth = (1.0 / binsLen * buffer.width - shift * 2);
            for (var k = start; k <= end; ++k) {
                var scoreInd = sortedInds[k];
                var score = Math.max(scores[scoreInd], 0.0);
                var currentYtop = (max - score) / max * barMaxHeight;
                var currentHeigth = barMaxHeight - currentYtop;
                buffer.rect(
                    i / binsLen * buffer.width + shift + shiftFirst + (k-start) * singleWidth,
                    currentYtop,
                    singleWidth,
                    currentHeigth
                    );


            }
        }
        start = end + 1;
    }
    //console.log("done");

}

function drawBarCharts() {
    //var userEvaluationOld; //from another file
    //var userEvaluation; //from another file
    barBuffer.clear();
    barBuffer.background(255);

    //barBuffer.fill(255, 0, 0);
    var max = 1.0;
    if (userEvaluationOld != null) {
        max = Math.max.apply(null, userEvaluationOld.realizationScores);
    }
    if (userEvaluation != null) {
        var newMax = Math.max.apply(null, userEvaluation.realizationScores);
        max = Math.max(max, newMax);
    }
    if (userEvaluationOld != null) {
        //barBuffer.scale(barBuffer.height/max, 1.0/barBuffer.width);
        barBuffer.fill(90, 90, 90);
        barBuffer.noStroke();

        var offset = barBuffer.width / 10 / 7;
        //TODO plot old values instead
        if (userEvaluation != null){
            drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset);
        }
        else{
            drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, false, true);
        }
    }
    if (userEvaluation != null) {
        barBuffer.fill(20, 50, 255);
        barBuffer.strokeWeight(1);
        barBuffer.stroke(0);
        drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, false, true);

        barBuffer.fill(200, 50, 255);
        barBuffer.noStroke();
        drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, true);
    }
}

function drawUserWellToBuffer(wellBuffer, committedPoints, maxNum) {
    var len = committedPoints.length;
    if (maxNum != undefined){
        len = Math.min(maxNum, len);
    }
    for (var i = 0; i < len; i++) {
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

var barTouched = -1;

function mousePressed() {
    if (mouseY > barBufferY 
        && mouseY < barBufferY + barBuffer.height) {


        console.log("mouse press");
        barTouched = Math.round(mouseX/barBuffer.width * 9);
        console.log("touched: " + barTouched);

    }
    drawBarCharts();
    redrawEnabledForAninterval();
    return false;
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


function drawWellToBuffer(buffer, wellPoints){

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

function scaleBufferForView(b, userdata) {
    b.scale(b.width / userdata.width, b.height / userdata.height);
    b.translate(-userdata.xtopleft, -userdata.ytopleft);
  }

