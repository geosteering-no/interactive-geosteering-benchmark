

function drawUserWell(wellBuffer, committedPoints){
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