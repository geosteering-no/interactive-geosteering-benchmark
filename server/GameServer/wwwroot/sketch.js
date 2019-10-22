var userdata = null;
var geoModelBuffer = null;
var wellBuffer;
var barBuffer;
var realizationScores;
var canvas = null;

//var xTravelDistance = 50;
var maxAngleChange = 3.14 / 180.0 * 2;
var minAngle = 0;
var maxAngle = 1.4;
var beginAngle = 3.14 / 180 * 10;
var nextAngles = [];

if (sessionStorage.getItem("angles")) {
  nextAngles = JSON.parse(sessionStorage.getItem("angles"));
}

var editNextAngleNo = 0;
//TODO consider init form data


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
var userEvaluationOld = null;
var userEvaluation = null;

var wellHeigth;
var barHeigth;

function buttonSelectSubSet(subsetIndex, curEvaluation) {
  curEvaluation = null;
  if (userEvaluationOld != null) {
    curEvaluation = userEvaluationOld;
  }
  if (userEvaluation != null) {
    curEvaluation = userEvaluation;
  }
  if (curEvaluation == null) {
    return;
  }
  selectedIndexCluster = subsetIndex;
  if (subsetIndex < 0) {
    //draw all
    drawGeomodelToBuffer(userdata);
  }
  else {
    var len = curEvaluation.sortedIndexes.length;
    var start = 0;
    if (subsetIndex > 0) {
      start = getIncluciveIndexEndForPercentile(subsetIndex - 1, len) + 1;
    }
    var end = getIncluciveIndexEndForPercentile(subsetIndex, len) + 1;
    var indexes = curEvaluation.sortedIndexes.slice(start, end);
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
        alert("Getting evaluation failed. Try refreshing the webpage.");
        //throw Error("getting userdata failed");
      }
      res.json()
        .then(function (json) {
          console.log("got user evaluation data:" + JSON.stringify(json));
          if (userEvaluation != null) {
            userEvaluationOld = userEvaluation;
          }
          userEvaluation = json;
          drawBarCharts();
          //redrawEnabledForAninterval();
        });

    });
}

function correctAnglesIfNeeded(){
  if (userdata != null){
    if (userdata.wellPoints.length + nextAngles.length > userdata.totalDecisionPoints){
      nextAngles.length = Math.max(0, userdata.totalDecisionPoints - userdata.wellPoints.length);
    }
  }
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
      alert("Updating failed. Try refreshing the page.");
      //throw Error("updating userdata failed");
    }
    else {
      res.json()
        .then(function (json) {
          console.log("got updated userdata:" + JSON.stringify(json));
          userdata = json;
          //need to remove the first angle now that it is accepted
          nextAngles.shift();
          if (editNextAngleNo > 0) {
            editNextAngleNo--;
          }
          correctAnglesIfNeeded();
          detectGameStateAndUpdateStuffAcordingly();
          sessionStorage.setItem("angles", JSON.stringify(nextAngles));
          updateSliderPosition();
          updateBars();
          drawGeomodelToBuffer(userdata);
          redrawEnabledForAninterval();
        });
    }
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
        alert("Stopping was not accepted?!");
        //throw Error("getting userdata failed");
      }
      else {
        console.log("stopping went normally let's wait for others");
        //TODO consider making it impossible to add new points
        //TODO consider sending a message to user
        res.json()
          .then(function (json) {
            console.log("got updated userdata:" + JSON.stringify(json));
            userdata = json;
            //need to remove the first angle now that it is accepted
            nextAngles.shift();
            if (editNextAngleNo > 0) {
              editNextAngleNo--;
            }
            stopGame();
            correctAnglesIfNeeded();
            updateSliderPosition();
            updateBars();
            drawGeomodelToBuffer(userdata);
            redrawEnabledForAninterval();
          });

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

  calculateCanvasSize();

  wellHeigth = canvasWidth * 0.5;
  barHeigth = canvasWidth * 0.3;

  canvas = createCanvas(canvasWidth, canvasHeigth);

  geoModelBuffer = createGraphics(windowWidth, Math.round(canvasHeigth / 8 * 3));
  wellBuffer = createGraphics(canvasWidth, Math.round(canvasHeigth / 8 * 3));
  barBuffer = createGraphics(canvasWidth, Math.round(canvasHeigth / 8 * 2));


  canvas.mousePressed(cmousePressed);
  canvas.mouseReleased(cmouseReleased);
  canvas.touchStarted(ctouchStarted);
  canvas.touchMoved(ctouchMoved);
  canvas.touchEnded(ctouchEnded);
  canvas.mouseMoved(cmouseMoved);
  prevButton = createButton("<- Previous");
  prevButton.mousePressed(previousButtonClick);

  nextButton = createButton("Next ->");
  nextButton.mousePressed(nextButtonClick);


  updateBarsButton = createButton("Reevaluate the objective");
  updateBarsButton.mousePressed(updateBars);
  //TODO reposition
  updateBarsButton.position(200, 850);

  submitDecisionButton = createButton("Check for new game.");
  submitDecisionButton.mousePressed(getUserData);
  submitDecisionButton.style('background-color', '#f44336');
  submitDecisionButton.style('color', 'white'); //font color
  //TODO reposition
  submitDecisionButton.position(200, 900);

  stopButton = createButton("Stop");
  stopButton.mousePressed(stopButtonClick);
  stopButton.position(0, 450);

  angleSlider = createSlider(-maxAngleChange, maxAngleChange, 0, 0);
  angleSlider.input(sliderAngleChange);
  // angleSlider.style('width', '280px');
  // angleSlider.style('height', '180px');
  // angleSlider.style('transform', 'scale(3)');


  setSizesAndPositions();

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

  //noLoop();

  getUserData();
  console.log("setup done");
}


function getUserData() {
  fetch("/geo/userdata", { credentials: 'include' })
    .then(function (res) {
      if (!res.ok) {
        alert("Getting userdata failed. Try going to login page.");
        //throw Error("getting userdata failed");
      }
      res.json()
        .then(function (json) {
          console.log("got userdata:" + JSON.stringify(json));
          userdata = json;

          detectGameStateAndUpdateStuffAcordingly();
          correctAnglesIfNeeded();
          updateSliderPosition();
          updateBars();
          drawWellToBuffer();
          drawGeomodelToBuffer(userdata);
          setSizesAndPositions();
          //redrawEnabledForAninterval();
        });
    });
}

function detectGameStateAndUpdateStuffAcordingly() {
  if (!userdata.stopped && userdata.wellPoints.length == 1) {
    // should be a new game

    submitDecisionButton.elt.textContent = "Submit current decision";
    submitDecisionButton.mousePressed(commitDecicion);
    sessionStorage.clear();
    nextAngles = [
      beginAngle,
      beginAngle - maxAngleChange,
      beginAngle - maxAngleChange * 1.2,
      beginAngle - maxAngleChange * 1.3,
      beginAngle - maxAngleChange * 1.4,
      beginAngle - maxAngleChange * 1.5,
      beginAngle - maxAngleChange * 1.6,
      beginAngle - maxAngleChange * 1.7,
      beginAngle - maxAngleChange * 1.8];
  }
  else if (userdata.stopped) {
      stopGame();
  }
  else if (nextAngles.length == 0) {

    submitDecisionButton.elt.textContent = "Stop drilling and end game";
  }
  else {
    submitDecisionButton.elt.textContent = "Submit current decision";
    submitDecisionButton.mousePressed(commitDecicion);
  }
}

function stopGame() {
  submitDecisionButton.elt.textContent =  "Stopped.. Click to check for new game.";
  submitDecisionButton.mousePressed(getUserData);
}

function calculateCanvasSize() {
  if (windowWidth > windowHeight) {
    canvasHeigth = Math.round(windowHeight);
    canvasWidth = Math.round(windowHeight * 0.5);
  } else {
    canvasHeigth = Math.round(windowWidth * 2);
    canvasWidth = Math.round(windowWidth);
  }
  // if (canvasWidth > canvasHeigth) {
  //   canvasWidth = Math.round(canvasHeigth / 4 * 3);
  // }
}

function setSizesAndPositions() {
  calculateCanvasSize();


  resizeCanvas(Math.round(canvasWidth), Math.round(canvasHeigth));
  wellHeigth = canvasWidth * 0.5;
  barHeigth = canvasWidth * 0.3;

  //TODO if problems with safari, check here
  if (geoModelBuffer != null && wellBuffer != null) {
    geoModelBuffer.resizeCanvas(Math.round(canvasWidth), Math.round(wellHeigth));
    wellBuffer.resizeCanvas(Math.round(canvasWidth), Math.round(wellHeigth));
    if (userdata != null) {
      drawGeomodelToBuffer(userdata);
      drawWellToBuffer();
    }
  }
  if (barBuffer != null){
    barBuffer.resizeCanvas(Math.round(canvasWidth), Math.round(barHeigth));
    drawBarCharts();
  }


  if (geoModelBuffer) {
    // geoModelBuffer.width = Math.round(canvasWidth);
    // geoModelBuffer.height = Math.round(canvasWidth * 0.6);

    // wellBuffer.width = Math.round(canvasWidth);
    // wellBuffer.height = Math.round(canvasWidth * 0.6);

    // barBuffer.width = Math.round(canvasWidth);
    // barBuffer.height = Math.round(canvasWidth * 0.3);
    // wellBuffer.size(Math.round(canvasWidth), Math.round(canvasHeigth / 8 * 3));
    // barBuffer.size(Math.round(canvasWidth), Math.round(canvasHeigth / 8 * 2));
  }

  var yPos = 0;
  var yMargin = canvasWidth * 0.01;

  function goDown(heigth) {
    yPos = yPos + heigth + yMargin;
  }

  goDown(0);
  var submitHeight = wellHeigth / 5;
  submitDecisionButton.position(canvasWidth / 4, yPos);
  submitDecisionButton.size(canvasWidth / 2, submitHeight);

  goDown(submitHeight);


  wellBufferY = yPos;
  //drawGeomodelToBuffer(userdata);

  goDown(wellHeigth);

  var buttonHeigth = wellHeigth / 5;
  prevButton.position(10, yPos);
  prevButton.size(canvasWidth / 3 - 15, buttonHeigth);

  stopButton.position(canvasWidth / 3 + 5, yPos);
  stopButton.size(canvasWidth / 3 - 10, buttonHeigth);

  nextButton.position(canvasWidth - canvasWidth / 3 + 5, yPos);
  nextButton.size(canvasWidth / 3 - 15, buttonHeigth);

  goDown(buttonHeigth);

  var sliderHeigth = canvasHeigth * 0.05;
  angleSlider.position(canvasWidth * 0.1, yPos);
  angleSlider.size(canvasWidth * 0.8, sliderHeigth);

  goDown(sliderHeigth);

  barBufferY = yPos;
  //drawBarCharts();
  goDown(barHeigth);

  updateBarsButton.position(5, yPos);
  updateBarsButton.size(canvasWidth - 10, wellHeigth / 5);

  //redrawEnabledForAninterval();
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
    barBuffer.noStroke();
    var offset = barBuffer.width / 10 / 7;
    if (barTouched >= 0 && userEvaluation == null){
      barBuffer.fill(255, 137, 10);
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, false, true, barTouched);
    }
    //barBuffer.scale(barBuffer.height/max, 1.0/barBuffer.width);
    barBuffer.fill(110);

    //TODO plot old values instead
    if (userEvaluation != null) {
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset);
    }
    else {
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, false, true);
      barBuffer.fill(60);
      barBuffer.noStroke();
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, true);
    }
  }
  if (userEvaluation != null) {
    if (barTouched >= 0){
      barBuffer.fill(255, 137, 10);
      drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, false, true, barTouched);
    }
    barBuffer.fill(20, 50, 255);
    barBuffer.strokeWeight(1);
    barBuffer.stroke(0);
    drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, false, true);

    barBuffer.fill(200, 50, 255);
    barBuffer.noStroke();
    drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, true);
  }
  

  barBuffer.noFill();
  barBuffer.strokeWeight(4);
  barBuffer.stroke(51, 255, 10);
  barBuffer.rect(0, 0, barBuffer.width, barBuffer.height);
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
    var fullUserTrajectory = userdata.wellPoints.slice(0);
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
    if (userEvaluation != null) {
      userEvaluationOld = userEvaluation;
      userEvaluation = null;
      drawBarCharts();
    }
  }



  //console.log(angleSlider.value());
  redrawEnabledForAninterval();

}

function updateSliderPosition(){
  if (editNextAngleNo < nextAngles.length){
    angleSlider.value(-(nextAngles[editNextAngleNo] - prevAngle(editNextAngleNo)));
  }
  
}

function previousButtonClick() {
  if (editNextAngleNo > 0) {
    editNextAngleNo--;
  }
  updateSliderPosition();
  redrawEnabledForAninterval();
}

function stopButtonClick() {
  nextAngles.length = editNextAngleNo;
  redrawEnabledForAninterval();
}

function nextButtonClick() {
  if (editNextAngleNo >= nextAngles.length - 1) {
    continueClick();
  }
  if (editNextAngleNo < nextAngles.length - 1) {
    editNextAngleNo++;
  }
  updateSliderPosition();
  redrawEnabledForAninterval();
}


function continueClick() {
  if (userdata != null) {
    var submittedLen = userdata.wellPoints.length;
    var newAnglesLen = nextAngles.length;
    if (submittedLen + newAnglesLen <= maxTortalAngles) {
      if (newAnglesLen === 0) {
        nextAngles.push(userdata.wellPoints[submittedLen - 1].angle);
      } else {
        nextAngles.push(nextAngles[newAnglesLen - 1]);
      }
    }
    detectGameStateAndUpdateStuffAcordingly();
    redrawEnabledForAninterval();
  }
}



function buttonSubmitPressed() {

}


function drawGeomodelToBuffer(userdata = null, specificIndices = null) {
  if (geoModelBuffer == undefined) return;
  //barBuffer.mousePressed(press);
  geoModelBuffer.resetMatrix();
  geoModelBuffer.clear();

  if (userdata != null) {
    scaleBufferForView(wellBuffer, userdata);
    console.log("scaled");
  }

  geoModelBuffer.background(0, 0, 0);
  geoModelBuffer.blendMode(ADD);
  geoModelBuffer.strokeWeight(1);


  if (userdata != null) {
    //if (false){
    scaleBufferForView(geoModelBuffer, userdata);
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
      for (var realj = 0; realj < specificIndices.length; realj++) {
        var reali = specificIndices[realj];
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
    //console.log("drawing triangles");
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

}

function windowResized() {
  setSizesAndPositions();
  redrawEnabledForAninterval();
}



function drawLayerToBuffer() {

}






function draw() {

  if (geoModelBuffer == null) return;

  //console.log("draw, wellY: " + wellBufferY);
  var geoModelHeight = canvasWidth * 0.5;
  clear();
  image(geoModelBuffer, 0, wellBufferY, canvasWidth, geoModelHeight);

  //drawWellToBuffer();

  image(wellBuffer, 0, wellBufferY, canvasWidth, geoModelHeight);

  if (barBuffer != null) {
    //console.log("draw bars");
    image(barBuffer, 0, barBufferY, canvasWidth, barBuffer.height);
    //image(barBuffer, 0, 10, 100, 100);
  }



  //for debugging
  drawFrame();

  // timerCountdown--;
  // if (timerCountdown <= 0) {
  //   noLoop();
  // }
}

function drawWellToBuffer() {

  if (userdata == null) return;
  //var t0 = performance.now();
  wellBuffer.clear();
  wellBuffer.resetMatrix();
  wellBuffer.background(0, 0, 0, 0);
  scaleBufferForView(wellBuffer, userdata);

  wellBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
  wellBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
  wellBuffer.strokeWeight(2 / userdata.height);
  var userPoints = userdata.wellPoints.slice(0);
  drawUserWellToBuffer(wellBuffer, userPoints);

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