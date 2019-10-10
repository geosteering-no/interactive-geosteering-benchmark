var canvasWidth;
var canvasHeigth;
var realization = null;
var committedPoints = [{ x: 0.0, y: 0.0 }, { x: 0.5, y: 0.5 }];
var nextPoints = [{ x: 0.98, y: 1.0 }];
var oneMarginInScript = 16;
var updateTimerEnabled = false;
var updateTimerLeft = 0;



function setup() {
  canvasWidth = windowWidth - oneMarginInScript * 2;
  canvasHeigth = windowHeight  - oneMarginInScript;
  if (canvasWidth > canvasHeigth) {
    canvasWidth = canvasHeigth / 4 * 3;
  }

  //detect screen orientation
  //mobile
  // if (windowWidth * 5 / 3 < windowHeight){
  //   canvasWidth = windowWidth;
  //   canvasHeigth = windowHeight/2;
  // }
  // else //(windowWidth * 5 / 3 >= windowHeight)
  // {
  //   //make 1/2 aspect ratio
  //   canvasHeigth = windowHeight/2;
  //   canvasWidth = windowHeight;
  // }


  createCanvas(canvasWidth, canvasHeigth);

  drawBuffer();

  fetch("/geo/init", { credentials: 'include' })
    .then(function (res) {
      console.log("init success");
      fetch("/geo/realization", { credentials: 'include' })
        .then(function (res2) {
          res2.json()
            .then(function (json) {
              console.log("got realization:" + JSON.stringify(json));
              realization = json;
              drawBuffer();
            });

        });
    });
}

function buttonSubmitPressed() {
  
}

function windowResized() {
  //TODO implement
  //resizeCanvas(windowWidth, windowHeight);
}

function mousePressed() {
  console.log("start mouse");
}

function touchStarted() {
  console.log("start touch");
}

function touchMoved() {
  buffer.stroke(255, 0, 0);
  buffer.line(mouseX, mouseY, pmouseX, pmouseY);
  return false;
}

function touchEnded() {
  buffer.stroke(0, 0, 255);
  buffer.line(mouseX, mouseY, pmouseX, pmouseY);
  return false;
}



// function mouseDragged(){
//   buffer.stroke(0, 255, 0);
//   buffer.line(mouseX, mouseY, pmouseX, pmouseY);
// }

let buffer;
let realizationObj;

function drawBuffer() {
  buffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);

  buffer.background(0, 0, 0);
  buffer.blendMode(BLEND);
  buffer.strokeWeight(1);


  if (realization != null) {
    var alpha = 2.55 / realization.length;
    buffer.stroke('rgba(100%, 100%, 100%, ' + alpha + ')');
    buffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
    for (guessi = 0; guessi < realization.length; guessi++) {
      console.log("guess:" + guessi);
      for (polygoni = 0; polygoni < realization[guessi].polygons.length; polygoni++) {
        console.log("poly:" + polygoni);
        var poly = realization[guessi].polygons[polygoni];

        buffer.beginShape();
        for (vertexi = 0; vertexi < poly.length; vertexi++) {
          var x = poly[vertexi].item1 * buffer.width;
          var y = poly[vertexi].item2 * buffer.height;
          buffer.vertex(x, y);
        }
        buffer.endShape(CLOSE);
      }
    }
  } else {
    // draw triangles for debug
    var points = 3;
    var shapes = 10;
    var alpha = 2.55 / shapes;
    buffer.stroke('rgba(100%, 100%, 100%, ' + alpha + ')');
    buffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');

    var rotate = TWO_PI / points / shapes;
    buffer.translate(buffer.width / 2, buffer.height / 2)

    for (var i = 0; i < shapes; i++) {
      buffer.rotate(rotate);
      drawCircle(buffer, 0, 0, buffer.height / 2, points);
    }
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

function drawFrame(){
  noFill();
  strokeWeight(4);
  stroke(51,255,10);
  rect(0,0,width, height);

}

function draw() {
  clear();
  image(buffer, 0, 0, buffer.width, buffer.heigth);

  drawWell();

  //for debugging
  drawFrame();


}

function drawWell() {

  stroke('rgba(100%, 0%, 0%, 1.0)');
  fill('rgba(100%, 0%, 0%, 1.0)');
  strokeWeight(2);
  for (i = 0; i < committedPoints.length; i++) {
    if (i > 0) {
      line(
        committedPoints[i].x * buffer.width,
        committedPoints[i].y * buffer.height,
        committedPoints[i - 1].x * buffer.width,
        committedPoints[i - 1].y * buffer.height);
    }
    //circlecircle()
  }


  for (i = 0; i < nextPoints.length; i++) {
    var prev;
    if (i === 0) {
      prev = committedPoints[committedPoints.length - 1];
    } else {
      prev = nextPoints[i - 1];
    }
    dashedLine(
      prev.x * buffer.width,
      prev.y * buffer.height,
      nextPoints[i].x * buffer.width,
      nextPoints[i].y * buffer.height,
      4, 4);
    circle(nextPoints[i].x * buffer.width, nextPoints[i].y * buffer.height, 10);
  }
}

function dashedLine(x1, y1, x2, y2, l, g) {
  var pc = dist(x1, y1, x2, y2) / 100;
  var pcCount = 1;
  var lPercent = gPercent = 0;
  var currentPos = 0;
  var xx1 = yy1 = xx2 = yy2 = 0;

  while (int(pcCount * pc) < l) {
    pcCount++
  }
  lPercent = pcCount;
  pcCount = 1;
  while (int(pcCount * pc) < g) {
    pcCount++
  }
  gPercent = pcCount;

  lPercent = lPercent / 100;
  gPercent = gPercent / 100;
  while (currentPos < 1) {
    xx1 = lerp(x1, x2, currentPos);
    yy1 = lerp(y1, y2, currentPos);
    xx2 = lerp(x1, x2, currentPos + lPercent);
    yy2 = lerp(y1, y2, currentPos + lPercent);
    if (x1 > x2) {
      if (xx2 < x2) {
        xx2 = x2;
      }
    }
    if (x1 < x2) {
      if (xx2 > x2) {
        xx2 = x2;
      }
    }
    if (y1 > y2) {
      if (yy2 < y2) {
        yy2 = y2;
      }
    }
    if (y1 < y2) {
      if (yy2 > y2) {
        yy2 = y2;
      }
    }

    line(xx1, yy1, xx2, yy2);
    currentPos = currentPos + lPercent + gPercent;
  }
}

function drawRealization(gr, realizationObj, realization) {
  var xArray = realizationObj.xArray;
  var interface = realization.interfaces[0];
  gr.stroke(126);
  for (var k = 0; k < 4; ++k) {
    for (var i = 0; i < xArray.length - 1; ++i) {
      gr.line(xArray[i], interface[i], xArray[i + 1], interface[i + 1]);
    }
  }
}

function getBoundingBox() {

}

function getRealizations() {
  var realizationObj = {};
  var maxInd = 40;
  var xArray = [];
  for (var j = 0; j < maxInd; ++j) {
    xArray[j] = j * 4;
  }
  realizationObj.xArray = xArray;
  var realizations = [];
  for (var i = 0; i < 100; i++) {
    realizations[i] = getOneRealiztion(xArray);
  }
  realizationObj.realizations = realizations;
  return realizationObj;
}


let initPositions = [10.0, 15.0, 23.0, 24.0];

function getOneRealiztion(xArray) {
  var realization = {};
  realization.interfaces = [];
  for (var k = 0; k < 4; ++k) {
    var positions = [];
    for (var j = 0; j < xArray.length; ++j) {
      positions[j] = initPositions[j] + random(-2, 2);
    }
    realization.interfaces[k] = positions;
  }
  return realization;
}