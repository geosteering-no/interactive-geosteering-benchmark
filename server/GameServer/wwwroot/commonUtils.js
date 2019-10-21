var oneMarginInScript = 10;

function drawCircle(buffer, x, y, radius, npoints) {

    var angle = TWO_PI / npoints;
    buffer.beginShape();
    for (var a = 0; a < TWO_PI; a += angle) {
        var sx = x + cos(a) * radius;
        var sy = y + sin(a) * radius;
        buffer.vertex(sx, sy);
    }
    buffer.endShape(CLOSE);

}

function drawFrame() {
    noFill();
    strokeWeight(4);
    stroke(51, 255, 10);
    rect(0, 0, width, height);
}



function scaleBufferForView(b, userdata) {
    b.scale(b.width / userdata.width, b.height / userdata.height);
    b.translate(-userdata.xtopleft, -userdata.ytopleft);
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

function drawUserWellToBuffer(wellBuffer, committedPoints, maxNum) {

    //wellBuffer.resetMatrix();
    //scaleBufferForView(wellBuffer);
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

