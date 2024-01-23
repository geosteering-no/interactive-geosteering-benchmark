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

var oneMarginInScript = 10;
var colorInformation = '#91bfdb';

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

//TODO make work for shifted geomodel
function drawScale(scaleBuffer, userdata, textSize, color) {
    if (userdata != null) {
        var textOffset = textSize + 1;
        var barLen = textOffset / 3 * 2;
        var barWidth = textOffset / 7;
        var myStroke = textOffset / 30;
        scaleBuffer.textSize(textSize);
        scaleBuffer.textAlign(CENTER, CENTER);
        //scaleBuffer.fill(150, 40, 120);
        scaleBuffer.fill(color);
        scaleBuffer.strokeWeight(myStroke);
        scaleBuffer.stroke(0);
        //scaleBuffer.noStroke();
        scaleBuffer.rectMode(CENTER);
        var hStep = 50;
        for (var i = hStep; i - userdata.xtopleft < userdata.width; i += hStep) {
            var coordOrig = (i - userdata.xtopleft);
            var coord = coordOrig * scaleBuffer.width / userdata.width;
            scaleBuffer.rect(coord, 0, barWidth, barLen);
            scaleBuffer.text(i, coord, textOffset);
        }
        var vStep = 5;
        for (var i = vStep; i - userdata.ytopleft < userdata.height; i += vStep) {
            var coordOrig = (i - userdata.ytopleft);
            var coord = coordOrig * scaleBuffer.height / userdata.height;
            scaleBuffer.rect(0, coord, barLen, barWidth);
            scaleBuffer.text(i, textOffset, coord);
        }
    }
}



function scaleBufferForView(b, userdata) {
    b.scale(b.width / userdata.width, b.height / userdata.height);
    b.translate(-userdata.xtopleft, -userdata.ytopleft);
}


function drawRealizationToBuffer(buffer, xlist, real) {
    var polyCount = real.yLists.length / 2;
    var prevBottom = null;
    for (var polygoni = 0; polygoni < polyCount; polygoni++) {
        //console.log("poly:" + polygoni);
        var polytop = real.yLists[polygoni * 2];
        var polybottom = real.yLists[polygoni * 2 + 1];
        buffer.beginShape();
        for (var vertexi = 0; vertexi < polytop.length; vertexi++) {
            var y = polytop[vertexi];
            if (prevBottom != null){
                y = Math.max(y, prevBottom[vertexi]);
            }
            buffer.vertex(xlist[vertexi], y);
        }

        for (var vertexi = polybottom.length - 1; vertexi >= 0; vertexi--) {
            var y = Math.max(polybottom[vertexi], polytop[vertexi]);
            buffer.vertex(xlist[vertexi], y);
        }
        buffer.endShape(CLOSE);
        prevBottom = polybottom;
    }
}

function drawUserWellToBuffer(wellBuffer, committedPoints, maxNum) {

    //wellBuffer.resetMatrix();
    //scaleBufferForView(wellBuffer);
    var len = committedPoints.length;
    if (maxNum != undefined) {
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

