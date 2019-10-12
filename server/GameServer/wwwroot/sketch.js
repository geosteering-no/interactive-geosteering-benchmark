var canvasWidth;
var canvasHeigth;
var userdata = null;
var xTravelDistance = 50;
var maxAngleChange = 3.14/180.0 * 2;
var minAngle = 0;
var maxAngle = 1.4;
var nextAngles = [ 1, 1, 0.8, 0.6, 0.4, 0.2, 0.1, 0.05, 0.02];
var editNextAngleNo = 0;
var oneMarginInScript = 16;
var updateTimerEnabled = false;
var updateTimerLeft = 0;


var prevButton;
var nextButton;
var angleSlider;

var buffer;
var wellBuffer;
var realizationObj;

function setup() {



  createCanvas(100, 100);




  prevButton = createButton("<- Previous");
  prevButton.mousePressed(previous);


  nextButton = createButton("Next ->");
  nextButton.mousePressed(next);

  angleSlider = createSlider(minAngle, maxAngle, 0, 0);
  angleSlider.input(angleChange);

  setSizesAndPositions();

  // fetch("/geo/init", { credentials: 'include' })
  //   .then(function (res) {
  //     console.log("init success");
  //     fetch("/geo/userdata", { credentials: 'include' })
  //       .then(function (res2) {
  //         res2.json()
  //           .then(function (json) {
  //             console.log("got userdata:" + JSON.stringify(json));
  //             userdata = json;
  //             drawBuffer();
  //           });

  //       });
  //   });
  var layerH = 20;
  var r1l1 = [70, 80, 60, 90, 85, 65];
  var r1l2 = [120, 100, 90, 80, 60, 50];
  var r2l1 = r1l1.map(function(n) { return n + 10;});
  var r2l2 = r1l2.map(function(n) { return n + 10;});
  var addH = function(n) {
    return n + layerH;
  };
   userdata = {
      XtopLeft : 50,
      Ytopleft : 50,
      Width : 500,
      Height : 100,

      wellPoints : [
        {X : 50, Y:50, Angle:PI/180.0*10},
        {X : 100, Y:52, Angle:PI/180.0*11}
      ],
      Xdist : 50,

      xList : [50, 100, 200, 300, 400, 500],
      realizations : [
        {
          YLists: [
            r1l1,
            r1l1.map(addH).reverse(),
            r1l2,
            r1l2.map(addH).reverse()
          ] 
        },
        {
          YLists: [
            r2l1,
            r2l1.map(addH).reverse(),
            r2l2,
            r2l2.map(addH).reverse()
          ]
        } 
      ]
   };

   console.log("userdata = " + JSON.stringify(userdata));

   drawBuffer();
}

function setSizesAndPositions() {
  canvasWidth = windowWidth - oneMarginInScript * 2;
  canvasHeigth = windowHeight  - oneMarginInScript;
  if (canvasWidth > canvasHeigth) {
    canvasWidth = canvasHeigth / 4 * 3;
  }

  resizeCanvas(canvasWidth, canvasHeigth);

  drawBuffer();

  prevButton.size(canvasWidth/2 - 15, 100);
  prevButton.position(10, buffer.height + 5);

  nextButton.position(canvasWidth/2 + 5, buffer.height + 5);
  nextButton.size(canvasWidth/2 - 15, 100);

  angleSlider.position( 10, buffer.height + prevButton.height + 10);
  angleSlider.size( canvasWidth - 40, 50);


}

function angleChange() {
  if (editNextAngleNo < nextAngles.length) {
    nextAngles[editNextAngleNo] = angleSlider.value();
  }
  console.log(angleSlider.value());
}

function previous() {
  if (editNextAngleNo > 0) {
    editNextAngleNo--;
  }
  angleSlider.value(nextAngles[editNextAngleNo]);
}

function next() {
  if (editNextAngleNo < nextAngles.length -1) {
    editNextAngleNo++;
  }
  angleSlider.value(nextAngles[editNextAngleNo]);
}

function buttonSubmitPressed() {
  
}

function windowResized() {
  setSizesAndPositions();
}



function drawBuffer() {
  buffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
  wellBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);

  if (userdata != null) {
    buffer.scale(buffer.width/userdata.Width, buffer.height/userdata.Height);
  }

  buffer.background(0, 0, 0);
  buffer.blendMode(BLEND);
  buffer.strokeWeight(1);


  if (userdata != null) {
    console.log("drawing userdat");
    var reals = userdata.realizations;
    var alpha = 2.55 / reals.length;
    buffer.stroke('rgba(100%, 100%, 100%, ' + alpha + ')');
    buffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
    for (reali = 0; reali < reals.length; reali++) {
      var layerBuffer = createGraphics(buffer.width, buffer.height);
      var xlist = userdata.xList;
      var xreverse = xlist.slice().reverse();
      //console.log("guess:" + reali);
      var polyCount = reals[reali].YLists.length/2;
      for (polygoni = 0; polygoni < polyCount; polygoni++) {
        //console.log("poly:" + polygoni);
        var polytop = reals[reali].YLists[polygoni*2];
        var polybottom = reals[reali].YLists[polygoni*2 +1].reverse();

        buffer.beginShape();
        for (vertexi = 0; vertexi < polytop.length; vertexi++) {
          var y = polytop[vertexi].item2;
          buffer.vertex(xlist[vertexi], y);
        }

        for (vertexi = 0; vertexi < polybottom.length; vertexi++) {
          var y = polybottom[vertexi].item2;
          buffer.vertex(xreverse[vertexi], y);
        }
        buffer.endShape(CLOSE);
      }
    }
  } else {
    console.log("drawing triangles");
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

  image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.heigth);

  //for debugging
  drawFrame();

}

function drawWell() {
  if (userdata == null) return ;

  wellBuffer.clear();

  wellBuffer.scale(wellBuffer.width/userdata.Width, wellBuffer.height/userdata.Height);

  wellBuffer.stroke('rgba(100%, 0%, 0%, 1.0)');
  wellBuffer.fill('rgba(100%, 0%, 0%, 1.0)');
  wellBuffer. strokeWeight(2);
  var x = 0.0;
  var y = 0.0;
  var committedPoints = userdata.wellPoints;
  for (i = 0; i < committedPoints.length; i++) {
    var point = committedPoints[i];
    var prev = point;
    if (i > 0) prev = committedPoints[i-1];
    wellBuffer.line(
      point.X,
      point.Y,
      prev.X,
      prev.Y);
  
    x = x2;
    y = y2;
    //circlecircle()
  }


  for (i = 0; i < nextAngles.length; i++) {
    wellBuffer.stroke('rgba(100%, 0%, 0%, 1.0)');
    wellBuffer.fill('rgba(100%, 0%, 0%, 1.0)');
    var angle = nextAngles[i];
    var x2 = x + xTravelDistance;
    var y2 = y + tan(angle) * xTravelDistance;
    // dashedLine(
    //   x,
    //   y,
    //   x2,
    //   y2,
    //   4, 4);
    
    if (editNextAngleNo === i) {
      wellBuffer.stroke('rgba(100%, 100%, 0%, 1.0)');
      wellBuffer.fill('rgba(100%, 100%, 0%, 1.0)');
    }
    //wellBuffer.circle(x, y, 10);
    x = x2;
    y = y2;
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

    wellBuffer.line(xx1, yy1, xx2, yy2);
    currentPos = currentPos + lPercent + gPercent;
  }
}

function drawRealization(gr, realizationObj, userdat) {
  var xArray = realizationObj.xArray;
  var interface = userdata.interfaces[0];
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
  var userdata = {};
  userdata.interfaces = [];
  for (var k = 0; k < 4; ++k) {
    var positions = [];
    for (var j = 0; j < xArray.length; ++j) {
      positions[j] = initPositions[j] + random(-2, 2);
    }
    userdata.interfaces[k] = positions;
  }
  return userdat;
}