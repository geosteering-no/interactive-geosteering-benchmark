var userdata = null;

//var xTravelDistance = 50;
var maxAngleChange = 3.14 / 180.0 * 2;
var minAngle = 0;
var maxAngle = 1.4;
var beginAngle = 3.14 / 180 * 10;
var nextAngles = [
  beginAngle,
  beginAngle - maxAngleChange,
  beginAngle - maxAngleChange * 1.2,
  beginAngle - maxAngleChange * 1.3,
  beginAngle - maxAngleChange * 1.4,
  beginAngle - maxAngleChange * 1.5,
  beginAngle - maxAngleChange * 1.6,
  beginAngle - maxAngleChange * 1.7,
  beginAngle - maxAngleChange * 1.8];
var editNextAngleNo = 0;

var updateTimerEnabled = false;
var updateTimerLeft = 0;
var timerCountdown = 0;

//===============================
//controls
//===============================

//navigation
var prevButton;
var nextButton;
//angle
var angleSlider;

//stop/continue
var stopButton;
var continueButton;

//selction buttons
var pButtons = [];
var pShowAllButton;

//evaluation 
var updateBarsButton;

//submission
var submitDecisionButton;

//===============================
//end of controls
//===============================

//variable for selected index of claster
var selectedIndexCluster = -1;

//var fullUserTrajectory;
var userEvaluationOld;
var userEvaluation;

function buttonSelectSubSet(subsetIndex) {
  if (userEvaluation == null) {
    return;
  }
  selectedIndexCluster = subsetIndex;
  if (subsetIndex < 0) {
    //draw all
    drawGeomodelToBuffer(userdata);
  }
  else {
    var len = userEvaluation.sortedIndexes.length;
    var start = 0;
    if (subsetIndex > 0) {
      start = getIncluciveIndexEndForPercentile(subsetIndex - 1, len) + 1;
    }
    var end = getIncluciveIndexEndForPercentile(subsetIndex, len) + 1;
    var indexes = userEvaluation.sortedIndexes.slice(start, end);
    drawGeomodelToBuffer(userdata, indexes);
  }
  drawBarCharts();
  redrawEnabledForAninterval();
}

function updateBars() {
  //TODO add full trajectory
  var fullUserTrajectory = getFullUserTrajectory();
  var bodyString = JSON.stringify(fullUserTrajectory);
  //+"/?"+bodyString
  fetch("/geo/evaluate",
    {
      credentials: 'include',
      method: 'POST',
      headers: {
        'Content-Type': 'application/json; charset=UTF-8'
        // 'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: bodyString
    })
    .then(function (res) {
      if (!res.ok) {
        alert("getting evaluation for a user failed");
        //throw Error("getting userdata failed");
      }
      res.json()
        .then(function (json) {
          console.log("got user evaluation data:" + JSON.stringify(json));
          userEvaluationOld = userEvaluation;
          userEvaluation = json;
          drawBarCharts();
          redrawEnabledForAninterval();
        });

    });
}

function commitNextPoint(wellPoint) {
  var bodyString = JSON.stringify(wellPoint);
  fetch("/geo/commitpoint", {
    credentials: 'include',
    method: 'POST',
    headers: {
      'Content-Type': 'application/json; charset=UTF-8'
      // 'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: bodyString
  }).then(function (res) {
    if (!res.ok) {
      alert("updating userdata failed");
      //throw Error("updating userdata failed");
    }

    res.json()
      .then(function (json) {
        console.log("got updated userdata:" + JSON.stringify(json));
        userdata = json;
        //need to remove the first angle now that it is accepted
        nextAngles.shift();
        if (editNextAngleNo > 0) {
          editNextAngleNo--;
        }
        updateBars();
        drawGeomodelToBuffer(userdata);
        redrawEnabledForAninterval();
      });
  });
}

function commitStop() {
  fetch("/geo/commitstop",
    {
      credentials: 'include',
      method: 'POST'
    })
    .then(function (res) {
      if (!res.ok) {
        alert("stopping was not accepted?!");
        //throw Error("getting userdata failed");
      }
      else {
        console.log("stopping went normally let's wait for others");
        //TODO consider making it impossible to add new points
        //TODO consider sending a message to user
      }
    });
}

function commitDecicion() {
  var fullUserTrajectory = getFullUserTrajectory();
  if (userdata != null && fullUserTrajectory != null) {
    var nextIndex = userdata.wellPoints.length;
    //check if we have a point to commit
    if (nextIndex < fullUserTrajectory.length) {
      var nextPoint = fullUserTrajectory[nextIndex];
      commitNextPoint(nextPoint);
      //this function should also do the new evaluation
    }
    else {
      commitStop();
    }
  }
}

function setup() {
  createCanvas(100, 100);
  prevButton = createButton("<- Previous");
  prevButton.mousePressed(buttonPreviousClick);

  nextButton = createButton("Next ->");
  nextButton.mousePressed(buttonNextClick);

  //TODO fix the button positions @morten
  for (var i = 0; i < 10; ++i) {
    if (i < 9) {
      pButtons[i] = createButton("P" + ((i + 1) * 10));
    } else {
      pButtons[i] = createButton("max");
    }


    pButtons[i].mousePressed(function () {
      var j = i;
      return function () {
        return buttonSelectSubSet(j);
      };
    }());

    pButtons[i].position(i * 800.0 / 10, 800);
  }

  pShowAllButton = createButton("Show all");
  //TODO reposition
  pShowAllButton.position(0, 830);
  pShowAllButton.mousePressed(function () {
    buttonSelectSubSet(-1);
  });

  updateBarsButton = createButton("Reevaluate the objective");
  updateBarsButton.mousePressed(updateBars);
  //TODO reposition
  updateBarsButton.position(200, 850);

  submitDecisionButton = createButton("Submit current decision");
  submitDecisionButton.mousePressed(commitDecicion);
  //TODO reposition
  submitDecisionButton.position(200, 900);

  stopButton = createButton("Stop");
  stopButton.mousePressed(stopDecision);
  stopButton.position(0, 450);

  continueButton = createButton("Continue");
  continueButton.mousePressed(continueDecision);
  continueButton.position(0, 480);


  angleSlider = createSlider(-maxAngleChange, maxAngleChange, 0, 0);
  angleSlider.input(sliderAngleChange);
  angleSlider.style('width', '280px');
  angleSlider.style('height', '180px');

  setSizesAndPositions();

  getUserData();

  var layerH = 15;
  var r1l1 = [100, 80, 60, 90, 85, 65];
  var r1l2 = [120, 100, 90, 80, 60, 50];
  var r2l1 = r1l1.map(function (n) { return n + 20; });
  var r2l2 = r1l2.map(function (n) { return n + 20; });
  var addH = function (n) {
    return n + layerH;
  };
  userdataFake = {
    Xtopleft: 50,
    Ytopleft: 50,
    Width: 450,
    Height: 100,

    wellPoints: [
      { X: 50, Y: 50, Angle: PI / 180.0 * 10 },
      { X: 100, Y: 52, Angle: PI / 180.0 * 11 }
    ],
    Xdist: 50,

    xList: [50, 100, 200, 300, 400, 500],
    realizations: [
      {
        yLists: [
          r1l1,
          r1l1.map(addH),
          r1l2,
          r1l2.map(addH)
        ]
      },
      {
        yLists: [
          r2l1,
          r2l1.map(addH),
          r2l2,
          r2l2.map(addH)
        ]
      }
    ]
  };

  noLoop();
}

function getUserData() {
  fetch("/geo/userdata", { credentials: 'include' })
    .then(function (res) {
      if (!res.ok) {
        alert("getting userdata failed");
        throw Error("getting userdata failed");
      }
      res.json()
        .then(function (json) {
          console.log("got userdata:" + JSON.stringify(json));
          userdata = json;
          updateBars();
          drawGeomodelToBuffer(userdata);
          redrawEnabledForAninterval();
        });
    });
}


function setSizesAndPositions() {
  canvasWidth = windowWidth - oneMarginInScript * 2;
  canvasHeigth = windowHeight - oneMarginInScript;
  if (canvasWidth > canvasHeigth) {
    canvasWidth = canvasHeigth / 4 * 3;
  }

  resizeCanvas(canvasWidth, canvasHeigth);

  drawGeomodelToBuffer(userdata);
  drawBarCharts();

  prevButton.size(canvasWidth / 2 - 15, 100);
  prevButton.position(10, geoModelBuffer.height + 5);

  nextButton.position(canvasWidth / 2 + 5, geoModelBuffer.height + 5);
  nextButton.size(canvasWidth / 2 - 15, 100);

  angleSlider.position(80, geoModelBuffer.height + prevButton.height + 10);
  angleSlider.size(canvasWidth - 80 * 2, 50);
  redrawEnabledForAninterval();
}






function allowedAngle(prev, dA) {
  //var newDA = Math.max(-dA, -maxAngleChange);
  //newDA = Math.max(-dA, -maxAngleChange);
  dA = Math.max(dA, -maxAngleChange);
  dA = Math.min(dA, maxAngleChange);
  return Math.max(prev + dA, 0);
}

function prevAngle(editNextAngleNo) {
  if (editNextAngleNo > 0) {
    return nextAngles[editNextAngleNo - 1];
  } else {
    if (userdata != null) {
      return userdata.wellPoints[userdata.wellPoints.length - 1].angle;
    }
  }
  return 0;
}

function getFullUserTrajectory() {
  if (userdata != null) {
    var xTravelDistance = userdata.xdist;
    var fullUserTrajectory = userdata.wellPoints.slice();
    var lastDefinedPoint = fullUserTrajectory[fullUserTrajectory.length - 1];
    var x = lastDefinedPoint.x;
    var y = lastDefinedPoint.y;
    for (var i = 0; i < nextAngles.length; ++i) {
      var angle = nextAngles[i];
      var x2 = x + xTravelDistance;
      var y2 = y + tan(angle) * xTravelDistance;
      fullUserTrajectory.push({ x: x2, y: y2, angle: angle });
      x = x2;
      y = y2;
    }
    return fullUserTrajectory;
  }
  return null;
}

function sliderAngleChange() {

  if (editNextAngleNo < nextAngles.length) {
    var prev = prevAngle(editNextAngleNo);
    nextAngles[editNextAngleNo] = allowedAngle(prev, -angleSlider.value());
    for (var i = editNextAngleNo + 1; i < nextAngles.length; ++i) {
      nextAngles[i] = allowedAngle(nextAngles[i - 1], nextAngles[i] - nextAngles[i - 1])
    }
  }



  //console.log(angleSlider.value());
  redrawEnabledForAninterval();

}

function buttonPreviousClick() {
  if (editNextAngleNo > 0) {
    editNextAngleNo--;
  }
  angleSlider.value(-(nextAngles[editNextAngleNo] - prevAngle(editNextAngleNo)));
  redrawEnabledForAninterval();
}

function stopDecision() {
  //TODO implemetn
}


function continueDecision() {
  //TODO implemetn
}

function buttonNextClick() {
  if (editNextAngleNo < nextAngles.length - 1) {
    editNextAngleNo++;
  }
  angleSlider.value(-(nextAngles[editNextAngleNo] - prevAngle(editNextAngleNo)));
  redrawEnabledForAninterval();
}

function buttonSubmitPressed() {

}

function windowResized() {
  setSizesAndPositions();
  redrawEnabledForAninterval();
}

function scaleBufferForView(b) {
  b.scale(b.width / userdata.width, b.height / userdata.height);
  b.translate(-userdata.xtopleft, -userdata.ytopleft);
}

function drawLayerToBuffer() {

}






function draw() {
  clear();
  image(geoModelBuffer, 0, 0, geoModelBuffer.width, geoModelBuffer.heigth);

  drawWellToBuffer();

  image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.heigth);

  if (barBuffer != null) {
    console.log("draw bars");
    image(barBuffer, 0, wellBuffer.height + 170, barBuffer.width, barBuffer.height);
    //image(barBuffer, 0, 10, 100, 100);
  }



  //for debugging
  drawFrame();

  timerCountdown--;
  if (timerCountdown <= 0) {
    noLoop();
  }
}

function drawWellToBuffer() {

  if (userdata == null) return;
  //var t0 = performance.now();
  wellBuffer.clear();

  wellBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
  wellBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
  wellBuffer.strokeWeight(2 / userdata.height);
  var userPoints = userdata.wellPoints.slice();
  drawUserWell(wellBuffer, userPoints);

  //main trajectory
  var x = userdata.wellPoints[userdata.wellPoints.length - 1].x;
  var y = userdata.wellPoints[userdata.wellPoints.length - 1].y;
  var xTravelDistance = userdata.xdist;

  for (var i = 0; i < nextAngles.length; i++) {
    if (i == 0) {
      wellBuffer.stroke('rgba(100%, 0%, 0%, 1.0)');
      wellBuffer.fill('rgba(100%, 0%, 0%, 1.0)');
      //wellBuffer.strokeWeight(3 / userdata.height);
    }
    else {
      wellBuffer.stroke('rgba(40%, 70%, 10%, 1.0)');
      wellBuffer.fill('rgba(40%, 70%, 10%, 1.0)');
      wellBuffer.strokeWeight(1 / userdata.height);
    }
    var angle = nextAngles[i];
    var x2 = x + xTravelDistance;
    var y2 = y + tan(angle) * xTravelDistance;
    //userPoints.push({ x: x2, y: y2, angle: angle });
    wellBuffer.line(
      x,
      y,
      x2,
      y2);
    // wellBuffer.line(
    //   x,
    //   y,
    //   x2,
    //   y2);
    x = x2;
    y = y2;
  }

  wellBuffer.stroke('rgba(40%, 30%, 80%, 1.0)');
  wellBuffer.fill('rgba(40%, 30%, 80%, 1.0)');
  wellBuffer.strokeWeight(0.5 / userdata.height)

  //possible trajectory up
  if (nextAngles.length > 0) {
    x = userdata.wellPoints[userdata.wellPoints.length - 1].x;
    y = userdata.wellPoints[userdata.wellPoints.length - 1].y;
    var myAngle = nextAngles[0];
    x = x + xTravelDistance;
    y = y + tan(myAngle) * xTravelDistance;
    wellBuffer.stroke('rgba(40%, 30%, 80%, 1.0)');
    wellBuffer.fill('rgba(40%, 30%, 80%, 1.0)');
    for (var i = 1; i < nextAngles.length; i++) {
      myAngle = myAngle + maxAngleChange;
      var x2 = x + xTravelDistance;
      var y2 = y + tan(myAngle) * xTravelDistance;
      wellBuffer.line(
        x,
        y,
        x2,
        y2);
      // wellBuffer.line(
      //   x,
      //   y,
      //   x2,
      //   y2);
      x = x2;
      y = y2;
    }
    x = userdata.wellPoints[userdata.wellPoints.length - 1].x;
    y = userdata.wellPoints[userdata.wellPoints.length - 1].y;
    myAngle = nextAngles[0];
    x = x + xTravelDistance;
    y = y + tan(myAngle) * xTravelDistance;
    wellBuffer.stroke('rgba(40%, 30%, 80%, 1.0)');
    wellBuffer.fill('rgba(40%, 30%, 80%, 1.0)');
    for (var i = 1; i < nextAngles.length; i++) {
      myAngle = Math.max(0, myAngle - maxAngleChange);
      var x2 = x + xTravelDistance;
      var y2 = y + tan(myAngle) * xTravelDistance;
      wellBuffer.line(
        x,
        y,
        x2,
        y2);
      // wellBuffer.line(
      //   x,
      //   y,
      //   x2,
      //   y2);
      x = x2;
      y = y2;
    }

  }

  //ellipses
  x = userdata.wellPoints[userdata.wellPoints.length - 1].x;
  y = userdata.wellPoints[userdata.wellPoints.length - 1].y;
  for (var i = 0; i < nextAngles.length; i++) {
    var angle = nextAngles[i];
    var x2 = x + xTravelDistance;
    var y2 = y + tan(angle) * xTravelDistance;
    x = x2;
    y = y2;
    wellBuffer.stroke('rgba(40%, 30%, 80%, 1.0)');
    wellBuffer.fill('rgba(40%, 30%, 80%, 1.0)');
    if (editNextAngleNo === i) {
      wellBuffer.stroke('rgba(100%, 100%, 0%, 1.0)');
      wellBuffer.fill('rgba(100%, 100%, 0%, 1.0)');
    }
    //TODO fix the scaling here
    wellBuffer.ellipse(x, y,
      userdata.doiX,
      userdata.doiY);
  }


  //var t1 = performance.now();
  //console.log("draw well to buffer " + (t1 - t0) + " milliseconds.");
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

function drawRealizationDepricated(gr, realizationObj, userdat) {
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


var initPositions = [10.0, 15.0, 23.0, 24.0];

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