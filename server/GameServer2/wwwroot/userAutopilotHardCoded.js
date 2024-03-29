var userdata = null;
var realizationScores = null;

//===============================
//buffers
//===============================
var geoModelBuffer = null;
var wellBuffer = null;
var barBuffer = null;
var scaleBuffer = null;
//===============================
//buffers
//===============================

//===============================
// colors
//===============================
//ascending ged to dark blue
//http://colorbrewer2.org/#type=diverging&scheme=RdYlBu&n=6
var colorDecision = '#d73027';
var colorOldWell = '#fc8d59';

var colorSelection = '#fee090';

//light yellow skipped
//light blue skipped

var colorFutureOptions = '#4575b4';
var colorDarkFuture = '#4575b4';

var canvasWidth;
var canvasHeigth;

//light blue here
var colorInformation = '#91bfdb';

// http://colorbrewer2.org/#type=diverging&scheme=RdYlBu&n=8
// var colorDecision = '#d73027';
// var colorOldWell = '#f46d43';

// var colorSelection = '#fdae61';

// //light yellow skipped
// //light blue skipped

// var colorFutureOptions = '#4575b4';
// var colorDarkFuture = '#4575b4';

// //light blue here
// var colorInformation = '#74add1';


var colorBarsBack = colorFutureOptions;
var colorBarsFront = colorInformation;
var colorBarsGray = '#3C3C3C';

var autopilotData = 
{"UserName":"DavidSLarsen","TrajectoryWithScore":[{"wellPoint":{"X":0.0,"Y":-0.0,"Angle":0.17453292519943295},"Score":0.0},{"wellPoint":{"X":28.559424837354033,"Y":5.092764085867846,"Angle":0.17646679684123262},"Score":-2.4948553947636065},{"wellPoint":{"X":57.118849674708066,"Y":9.162866028922387,"Angle":0.14156021184123263},"Score":-4.975782459977491},{"wellPoint":{"X":85.6782745120621,"Y":12.381591123502048,"Angle":0.11222915238888473},"Score":-7.4474424162581965},{"wellPoint":{"X":114.23769934941613,"Y":14.59429068126991,"Angle":0.07732256738888474},"Score":253.58460018989484},{"wellPoint":{"X":142.79712418677016,"Y":15.806393733896666,"Angle":0.04241598238888474},"Score":439.0514550562074},{"wellPoint":{"X":171.3565490241242,"Y":16.020861835573918,"Angle":0.007509397388884745},"Score":613.617960679202},{"wellPoint":{"X":199.91597386147825,"Y":16.020861835573918,"Angle":0.0},"Score":800.540108321677},{"wellPoint":{"X":228.4753986988323,"Y":16.020861835573918,"Angle":0.0},"Score":1035.723791973303},{"wellPoint":{"X":257.0348235361863,"Y":16.020861835573918,"Angle":0.0},"Score":1234.9247864501103},{"wellPoint":{"X":285.5942483735403,"Y":16.020861835573918,"Angle":0.0},"Score":1422.7043403831005},{"wellPoint":{"X":314.15367321089434,"Y":16.020861835573918,"Angle":0.0},"Score":1548.1225965264105},{"wellPoint":{"X":342.71309804824836,"Y":16.020861835573918,"Angle":0.0},"Score":1682.6052403002657}],"Stopped":true,"AccumulatedScoreFromPreviousGames":0.0,"AccumulatedScorePercentFromPreviousGames":0.0};
//this requies localization to make score better match
//{"UserName":"Ecs1","TrajectoryWithScore":[{"wellPoint":{"X":0.0,"Y":-0.0,"Angle":0.17453292519943295},"Score":0.0},{"wellPoint":{"X":28.559424837354033,"Y":5.875146804658078,"Angle":0.20288616665537165},"Score":-2.5075424793083654},{"wellPoint":{"X":57.118849674708066,"Y":12.037519906788429,"Angle":0.21251556941399236},"Score":-5.020178724050391},{"wellPoint":{"X":85.6782745120621,"Y":17.23889314788435,"Angle":0.18015007680862835},"Score":186.83805890543724},{"wellPoint":{"X":114.23769934941613,"Y":21.717245464421822,"Angle":0.15554160309215326},"Score":292.1209913378523},{"wellPoint":{"X":142.79712418677016,"Y":25.24005788837759,"Angle":0.12273030480351976},"Score":289.6462661290963},{"wellPoint":{"X":171.3565490241242,"Y":27.81503667505904,"Angle":0.08991900651488627},"Score":287.18019267933613},{"wellPoint":{"X":199.91597386147825,"Y":29.387746644888377,"Angle":0.05501242151488627},"Score":456.311559350906},{"wellPoint":{"X":228.4753986988323,"Y":30.35064697500301,"Angle":0.03370290985460527},"Score":652.3541456417245},{"wellPoint":{"X":257.0348235361863,"Y":30.35064697500301,"Angle":0.0},"Score":966.2606345168077},{"wellPoint":{"X":285.5942483735403,"Y":30.35064697500301,"Angle":0.0},"Score":1160.5527111364524},{"wellPoint":{"X":314.15367321089434,"Y":30.35064697500301,"Angle":0.0},"Score":1372.0917388560902},{"wellPoint":{"X":342.71309804824836,"Y":30.35064697500301,"Angle":0.0},"Score":1596.3539994904722}],"Stopped":true,"AccumulatedScoreFromPreviousGames":0.0,"AccumulatedScorePercentFromPreviousGames":0.0};
//{"UserName":"Ecs1","TrajectoryWithScore":[{"wellPoint":{"X":0.0,"Y":-0.0,"Angle":0.17453292519943295},"Score":0.0},{"wellPoint":{"X":28.559424837354033,"Y":6.018614235137343,"Angle":0.20770086803468196},"Score":-2.5100576842667275},{"wellPoint":{"X":57.118849674708066,"Y":12.936761790032639,"Angle":0.23765900995039077},"Score":-5.037201761565177},{"wellPoint":{"X":85.6782745120621,"Y":18.807927475793853,"Angle":0.20275242495039078},"Score":182.50729830264083},{"wellPoint":{"X":114.23769934941613,"Y":23.647036528738195,"Angle":0.1678458399503908},"Score":235.56909836138738},{"wellPoint":{"X":142.79712418677016,"Y":27.46623041533523,"Angle":0.1329392549503908},"Score":233.09112362170077},{"wellPoint":{"X":171.3565490241242,"Y":30.274990628301794,"Angle":0.09803266995039081},"Score":365.5894000824642},{"wellPoint":{"X":199.91597386147825,"Y":32.080233850125715,"Angle":0.06312608495039082},"Score":527.4162468573159},{"wellPoint":{"X":228.4753986988323,"Y":33.655497945169664,"Angle":0.05510158265154024},"Score":672.6368972656866},{"wellPoint":{"X":257.0348235361863,"Y":34.429790376599584,"Angle":0.02710498574221714},"Score":837.6012256407039},{"wellPoint":{"X":285.5942483735403,"Y":34.429790376599584,"Angle":0.0},"Score":1031.8933022603487},{"wellPoint":{"X":314.15367321089434,"Y":34.429790376599584,"Angle":0.0},"Score":1243.4323299799862},{"wellPoint":{"X":342.71309804824836,"Y":34.429790376599584,"Angle":0.0},"Score":1467.6945906143683}],"Stopped":true,"AccumulatedScoreFromPreviousGames":0.0,"AccumulatedScorePercentFromPreviousGames":0.0};
//{"UserName":"Ecs1","TrajectoryWithScore":[{"wellPoint":{"X":0.0,"Y":-0.0,"Angle":0.17453292519943295},"Score":0.0},{"wellPoint":{"X":28.559424837354033,"Y":5.03579715234098,"Angle":0.17453292519943295},"Score":-2.4939999999999998},{"wellPoint":{"X":57.118849674708066,"Y":9.100210370290528,"Angle":0.14136498236418393},"Score":-4.974858088012759},{"wellPoint":{"X":85.6782745120621,"Y":12.152139255950713,"Angle":0.10645839736418394},"Score":-7.444952682448931},{"wellPoint":{"X":114.23769934941613,"Y":14.73610311398139,"Angle":0.09023107049317504},"Score":-9.911095640563804},{"wellPoint":{"X":142.79712418677016,"Y":16.317752632060152,"Angle":0.05532448549317504},"Score":-12.370969806271482},{"wellPoint":{"X":171.3565490241242,"Y":17.79470065191306,"Angle":0.05166887889036533},"Score":202.55112835562332},{"wellPoint":{"X":199.91597386147825,"Y":18.496372449408888,"Angle":0.02456389334758113},"Score":437.48872473691983},{"wellPoint":{"X":228.4753986988323,"Y":18.496372449408888,"Angle":0.0},"Score":695.6726540452216},{"wellPoint":{"X":257.0348235361863,"Y":18.97004455979302,"Angle":0.0165839714176245},"Score":880.1846878102419},{"wellPoint":{"X":285.5942483735403,"Y":18.97004455979302,"Angle":0.0},"Score":1073.64316074558},{"wellPoint":{"X":314.15367321089434,"Y":18.97004455979302,"Angle":0.0},"Score":1272.5902989835913},{"wellPoint":{"X":342.71309804824836,"Y":18.97004455979302,"Angle":0.0},"Score":1460.566832941697}],"Stopped":true,"AccumulatedScoreFromPreviousGames":0.0,"AccumulatedScorePercentFromPreviousGames":0.0};
//QUITE BAD
//{"UserName":"DavidSLarsen","TrajectoryWithScore":[{"wellPoint":{"X":0.0,"Y":-0.0,"Angle":0.17453292519943295},"Score":0.0},{"wellPoint":{"X":28.559424837354033,"Y":5.092764085867846,"Angle":0.17646679684123262},"Score":-2.4948553947636065},{"wellPoint":{"X":57.118849674708066,"Y":9.162866028922387,"Angle":0.14156021184123263},"Score":-4.975782459977491},{"wellPoint":{"X":85.6782745120621,"Y":12.381591123502048,"Angle":0.11222915238888473},"Score":-7.4474424162581965},{"wellPoint":{"X":114.23769934941613,"Y":14.59429068126991,"Angle":0.07732256738888474},"Score":253.58460018989484},{"wellPoint":{"X":142.79712418677016,"Y":15.806393733896666,"Angle":0.04241598238888474},"Score":439.0514550562074},{"wellPoint":{"X":171.3565490241242,"Y":16.020861835573918,"Angle":0.007509397388884745},"Score":613.617960679202},{"wellPoint":{"X":199.91597386147825,"Y":16.020861835573918,"Angle":0.0},"Score":800.540108321677},{"wellPoint":{"X":228.4753986988323,"Y":16.020861835573918,"Angle":0.0},"Score":1035.723791973303},{"wellPoint":{"X":257.0348235361863,"Y":16.020861835573918,"Angle":0.0},"Score":1234.9247864501103},{"wellPoint":{"X":285.5942483735403,"Y":16.020861835573918,"Angle":0.0},"Score":1422.7043403831005},{"wellPoint":{"X":314.15367321089434,"Y":16.020861835573918,"Angle":0.0},"Score":1548.1225965264105},{"wellPoint":{"X":342.71309804824836,"Y":16.020861835573918,"Angle":0.0},"Score":1682.6052403002657}],"Stopped":true,"AccumulatedScoreFromPreviousGames":0.0,"AccumulatedScorePercentFromPreviousGames":0.0};



//var xTravelDistance = 50;
var maxAngleChange = 3.14159265 / 180.0 * 2;
var minAngle = 0;
var maxAngle = 1.4;
//var beginAngle = 3.14159265 / 180 * 10;
var nextAngles = [];

var myResult = undefined;




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

//selction buttons
var pButtons = [];
var pShowAllButton;

//evaluation 
var updateBarsButton;

//submission
var submitDecisionButton;

//canvas is where you touch
var canvas = null;

//resizeButton
resizeButton = null;

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
          if (logDisabled === undefined) {
            console.log("got user evaluation data:" + JSON.stringify(json));
          }
          if (userEvaluation != null) {
            userEvaluationOld = userEvaluation;
          }
          userEvaluation = json;
          updateBarsButton.elt.textContent = "No need to evaluate";
          //TODO change color
          updateBarsButton.style('background-color', colorBarsGray);

          //setSizesAndPositions();
          drawBarCharts();
          //redrawEnabledForAninterval();
        });

    });
}

function correctAnglesIfNeeded() {
  if (userdata != null) {
    if (userdata.stopped) {
      nextAngles.length = 0;
      return;
    }

    if (userdata.wellPoints.length + nextAngles.length > userdata.totalDecisionPoints) {
      nextAngles.length = Math.max(0, userdata.totalDecisionPoints - userdata.wellPoints.length);
    }
  }
}

function commitNextPoint(wellPoint) {
  preventUpdatingAndUpdateButton();
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
      alert("Updating failed. Update?");
      getUserData();
      //throw Error("updating userdata failed");
    }
    else {
      res.json()
        .then(function (json) {
          console.log("got updated userdata");
          if (logDisabled === undefined) {
            console.log("data : " + JSON.stringify(json));
          }
          userdata = json;
          //need to remove the first angle now that it is accepted
          nextAngles.shift();
          if (editNextAngleNo > 0) {
            editNextAngleNo--;
          }
          correctAnglesIfNeeded();
          detectGameStateAndUpdateButton();
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
  preventUpdatingStopAndUpdateButton();
  fetch("/geo/commitstop",
    {
      credentials: 'include',
      method: 'POST'
    })
    .then(function (res) {
      if (!res.ok) {
        alert("Stopping was not accepted! Update?");
        getUserData();
        //throw Error("getting userdata failed");
      }
      else {
        console.log("stopping went normally let's wait for others");
        res.json()
          .then(function (json) {
            console.log("got updated userdata");
            if (logDisabled === undefined) {
              console.log("useradata : " + JSON.stringify(json));
            }
            //userdata = json;
            myResult = json;
            stopGame();
            getUserData();
            // //need to remove the first angle now that it is accepted
            // nextAngles.shift();
            // if (editNextAngleNo > 0) {
            //   editNextAngleNo--;
            // }
            // stopGame();
            // correctAnglesIfNeeded();
            // detectGameStateAndUpdateButton();
            // updateSliderPosition();
            // updateBars();
            // drawGeomodelToBuffer(userdata);
            // redrawEnabledForAninterval();
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
      disableSubmitForShortTime();
      //this function should also do the new evaluation
    }
    else {
      commitStop();
    }
  }
}

function disableSubmitForShortTime() {
  submitDecisionButton.elt.disabled = true;
  setTimeout(
    function () {
      submitDecisionButton.elt.disabled = false;
    },
    500);
}
function centerCanvas() {
  var canvas = $("#defaultCanvas0");
  var bod = $("body")[0];

  // Check to make sure these elements exist
  if (canvas.length && bod) {
    var canv_width = canvas.width();
    var margin = (bod.offsetWidth - canv_width) / 2;

    bod.style.marginLeft = margin + "px";
    bod.style.marginRight = margin + "px";
    if (logDisabled === undefined) {
      console.log("centered");
    }
  }
  else {
    if (logDisabled === undefined) {
      console.log("could not center");
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
  scaleBuffer = createGraphics(canvasWidth, Math.round(canvasHeigth / 8 * 3));

  barBuffer = createGraphics(canvasWidth, Math.round(canvasHeigth / 8 * 2));



  canvas.mousePressed(cmousePressed);
  canvas.mouseReleased(cmouseReleased);
  canvas.touchStarted(ctouchStarted);
  canvas.touchMoved(ctouchMoved);
  canvas.touchEnded(ctouchEnded);
  canvas.mouseMoved(cmouseMoved);
  prevButton = createButton("<- Previous");
  prevButton.mousePressed(previousButtonClick);

  nextButton = createButton("Plan ahead ->");
  nextButton.mousePressed(nextButtonClick);


  updateBarsButton = createButton("Evaluate the well plan");
  updateBarsButton.mousePressed(updateBars);
  updateBarsButton.style('background-color', colorBarsFront);
  updateBarsButton.position(200, 850);

  submitDecisionButton = createButton("Check for new game.");
  submitDecisionButton.mousePressed(getUserData);
  submitDecisionButton.style('background-color', colorDecision);
  submitDecisionButton.style('color', 'white'); //font color
  submitDecisionButton.position(200, 900);

  stopButton = createButton("Plan stopping");
  stopButton.mousePressed(stopButtonClick);
  stopButton.position(0, 450);

  // resizeButton = createButton("Resize");
  // resizeButton.mousePressed(setSizesAndPositions);
  // resizeButton.position(300, 0);

  angleSlider = createSlider(-maxAngleChange, maxAngleChange, 0, 0);
  angleSlider.input(sliderAngleChange);
  // angleSlider.style('width', '280px');
  // angleSlider.style('height', '180px');
  // angleSlider.style('transform', 'scale(3)');


  setSizesAndPositions();

  // var layerH = 15;
  // var r1l1 = [100, 80, 60, 90, 85, 65];
  // var r1l2 = [120, 100, 90, 80, 60, 50];
  // var r2l1 = r1l1.map(function (n) { return n + 20; });
  // var r2l2 = r1l2.map(function (n) { return n + 20; });
  // var addH = function (n) {
  //   return n + layerH;
  // };
  // userdataFake = {
  //   Xtopleft: 50,
  //   Ytopleft: 50,
  //   Width: 450,
  //   Height: 100,

  //   wellPoints: [
  //     { X: 50, Y: 50, Angle: PI / 180.0 * 10 },
  //     { X: 100, Y: 52, Angle: PI / 180.0 * 11 }
  //   ],
  //   Xdist: 50,

  //   xList: [50, 100, 200, 300, 400, 500],
  //   realizations: [
  //     {
  //       yLists: [
  //         r1l1,
  //         r1l1.map(addH),
  //         r1l2,
  //         r1l2.map(addH)
  //       ]
  //     },
  //     {
  //       yLists: [
  //         r2l1,
  //         r2l1.map(addH),
  //         r2l2,
  //         r2l2.map(addH)
  //       ]
  //     }
  //   ]
  // };

  //noLoop();

  getUserData();
  console.log("setup done");
}

function tryStartNewGame() {
  if (userdata == null) {
    return;
  }
  if (!userdata.stopped && userdata.wellPoints.length == 1) {
    // should be a new game

    submitDecisionButton.elt.textContent = "Submit current decision";
    submitDecisionButton.mousePressed(commitDecicion);
    sessionStorage.clear();

    //relative here
    //pre fill from autopilot
    nextAngles = [];
    for (var i = 1; i < autopilotData.TrajectoryWithScore.length; ++i) {
      nextAngles.push( 
        autopilotData.TrajectoryWithScore[i].wellPoint.Angle -
        autopilotData.TrajectoryWithScore[i-1].wellPoint.Angle);
    }
  }
}

function commitNewGame(){
  preventUpdatingNewGameAndUpdateButton();
  //this one just returns game index
  fetch("/geo/newgame",
    {
      credentials: 'include',
      method: 'POST'
    })
    .then(function (res) {
      if (!res.ok) {
        alert("New game was not accepted! Update?");
        getUserData();
        //throw Error("getting userdata failed");
      }
      else {
        console.log("new game request went normally");
        userEvaluationOld = null;
        userEvaluation = null;
        res.json().then(function (json){
          console.log("starting game "+JSON.stringify(json));
          getUserData();
        }
        );
      }
    });
}

function getUserData() {
  fetch("/geo/userdata", { credentials: 'include' })
    .then(function (res) {
      if (!res.ok) {
        alert("Getting userdata failed. Try going to login page.");
        window.location.href = "/login.html";
        //throw Error("getting userdata failed");
      }
      res.json()
        .then(function (json) {
          console.log("got userdata");
          if (logDisabled === undefined) {
            console.log("userdata : " + JSON.stringify(json));
          }
          userdata = json;

          tryStartNewGame();
          detectGameStateAndUpdateButton();
          correctAnglesIfNeeded();
          updateSliderPosition();
          updateBars();
          drawWellToBuffer();
          drawGeomodelToBuffer(userdata);
          // window.resizeTo(width - 1, height);
          // window.resizeTo(width + 1, height);
          setSizesAndPositions();
          //redrawEnabledForAninterval();
        });
    });
}

function doNothing() {
  alert("Drilling in progress... wait...");
}

function preventUpdatingAndUpdateButton() {
  submitDecisionButton.elt.textContent = "Drilling and updating...";
  submitDecisionButton.mousePressed(doNothing);
}

function preventUpdatingStopAndUpdateButton() {
  submitDecisionButton.elt.textContent = "Stopping and pulling out...";
  submitDecisionButton.mousePressed(doNothing);
}

function preventUpdatingNewGameAndUpdateButton() {
  submitDecisionButton.elt.textContent = "Finding new location to drill...";
  submitDecisionButton.mousePressed(doNothing);
}

function detectGameStateAndUpdateButton() {
  if (userdata.stopped) {
    stopGame();
  }
  else if (nextAngles.length == 0) {
    submitDecisionButton.elt.textContent = "Stop drilling! (end game and see score)";
    submitDecisionButton.mousePressed(commitDecicion);
  }
  else {
    submitDecisionButton.elt.textContent = "Drill ahead!";
    submitDecisionButton.mousePressed(commitDecicion);
  }
}

function stopGame() {
  if (myResult){
    var value = myResult.scoreValue;
    var percent = myResult.scorePercent;
    var percentile = myResult.youDidBetterThan;
    submitDecisionButton.elt.textContent = "Your score is " + Math.round(value) + ". You did better than " 
      + Math.round(percentile) + "%. New game?";
  }else{
    submitDecisionButton.elt.textContent = "Stopped. Click to check for new game.";
  }
  submitDecisionButton.mousePressed(commitNewGame);
}

function calculateCanvasSize() {
  if (windowWidth > windowHeight / 3 * 2) {
    canvasHeigth = Math.round(windowHeight);
    canvasWidth = Math.round(windowHeight / 3 * 2);
  } else {
    canvasHeigth = Math.round(windowHeight);
    canvasWidth = Math.round(windowWidth);
  }
  // if (canvasWidth > canvasHeigth) {
  //   canvasWidth = Math.round(canvasHeigth / 4 * 3);
  // }
}


function setSizesAndPositions() {
  calculateCanvasSize();


  resizeCanvas(Math.round(canvasWidth), Math.round(canvasHeigth));

  //all ints
  var buttonHeight = 44;
  var marginHeight = 5;
  var totalButtonHeight = buttonHeight * 5 + marginHeight * 7;
  var totalContentHeight = Math.round(canvasHeigth) - totalButtonHeight;

  wellHeigth = Math.round(totalContentHeight * 0.65);
  barHeigth = Math.round(totalContentHeight * 0.35);

  //TODO if problems with safari, check here
  if (geoModelBuffer != null && wellBuffer != null) {
    geoModelBuffer.resizeCanvas(Math.round(canvasWidth), Math.round(wellHeigth));
    wellBuffer.resizeCanvas(Math.round(canvasWidth), Math.round(wellHeigth));
    if (userdata != null) {
      drawGeomodelToBuffer(userdata);
      drawWellToBuffer();
    }
  }
  if (scaleBuffer != null && userdata != null) {
    scaleBuffer.resizeCanvas(Math.round(canvasWidth), Math.round(wellHeigth));
    //drawScale(scaleBuffer);
    drawScale(scaleBuffer, userdata, 14, colorInformation);
  }

  if (barBuffer != null) {
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
  var yMargin = marginHeight;

  function goDown(heigth) {
    yPos = yPos + heigth + yMargin;
  }

  //submit button
  goDown(0);

  //var submitHeight = wellHeigth / 5;

  submitDecisionButton.position(canvasWidth / 4, yPos);
  submitDecisionButton.size(canvasWidth / 2, buttonHeight);

  //main display
  goDown(buttonHeight);

  wellBufferY = yPos;
  //drawGeomodelToBuffer(userdata);

  //prev stop next buttons
  goDown(wellHeigth);

  //var buttonHeight = wellHeigth / 5;
  prevButton.position(10, yPos);
  prevButton.size(canvasWidth / 3 - 15, buttonHeight);

  stopButton.position(canvasWidth / 3 + 5, yPos);
  stopButton.size(canvasWidth / 3 - 10, buttonHeight);

  nextButton.position(canvasWidth - canvasWidth / 3 + 5, yPos);
  nextButton.size(canvasWidth / 3 - 15, buttonHeight);

  //slider
  goDown(buttonHeight);

  //var sliderHeigth = canvasHeigth * 0.05;
  angleSlider.position(canvasWidth * 0.1, yPos + buttonHeight / 2);
  angleSlider.size(canvasWidth * 0.8, buttonHeight);

  //bars
  goDown(buttonHeight * 2);

  barBufferY = yPos;

  //recompute button
  //drawBarCharts();
  goDown(barHeigth);

  updateBarsButton.position(canvasWidth / 4, yPos);
  updateBarsButton.size(canvasWidth / 2, buttonHeight);
  //submitDecisionButton.position(canvasWidth / 4, yPos);
  //submitDecisionButton.size(canvasWidth / 2, buttonHeight);

  centerCanvas();
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
    if (barTouched >= 0 && userEvaluation == null) {
      barBuffer.fill(colorSelection);
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, false, true, barTouched);
    }
    //barBuffer.scale(barBuffer.height/max, 1.0/barBuffer.width);
    barBuffer.fill(110);

    if (userEvaluation != null) {
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset);
    }
    else {
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, false, true);
      barBuffer.fill(colorBarsGray);
      barBuffer.noStroke();
      drawBarChartsToBufferWithShift(userEvaluationOld, barBuffer, 0, max, -offset, true);
    }
  }
  if (userEvaluation != null) {
    if (barTouched >= 0) {
      barBuffer.noStroke();
      barBuffer.fill(colorSelection);
      drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, false, true, barTouched);
    }
    barBuffer.fill(colorBarsBack);
    barBuffer.strokeWeight(1);
    barBuffer.stroke(0);
    drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, false, true);

    barBuffer.fill(colorBarsFront);
    barBuffer.noStroke();
    drawBarChartsToBufferWithShift(userEvaluation, barBuffer, 0, max, 0.0, true);
  }


  barBuffer.noFill();
  barBuffer.strokeWeight(1.5);
  barBuffer.stroke(51);
  var barMaHeight = getBarMaxHeight(barBuffer);
  barBuffer.rect(0, 0, barBuffer.width, barMaHeight);
}

function allowedAngleRelative(prev, dA) {
  //var newDA = Math.max(-dA, -maxAngleChange);
  //newDA = Math.max(-dA, -maxAngleChange);
  dA = Math.max(dA, -maxAngleChange);
  dA = Math.min(dA, maxAngleChange);
  if (prev + dA < 0) {
    dA = -prev;
  }
  return dA;
}

function allowedAngle(prev, dA) {
  //var newDA = Math.max(-dA, -maxAngleChange);
  //newDA = Math.max(-dA, -maxAngleChange);
  dA = Math.max(dA, -maxAngleChange);
  dA = Math.min(dA, maxAngleChange);
  return Math.max(prev + dA, 0);
}

function prevAngle(editNextAngleNo) {
  if (userdata != null) {
    var prev = userdata.wellPoints[userdata.wellPoints.length - 1].angle;
    for (var i = 0; i < editNextAngleNo; i++) {
      prev += nextAngles[i];
    }
    return prev;
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
    var angle = lastDefinedPoint.angle;
    //relative angle here
    for (var i = 0; i < nextAngles.length; ++i) {
      angle += nextAngles[i];
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

function invalidateUserEvaluation() {
  if (userEvaluation != null) {
    userEvaluationOld = userEvaluation;
    userEvaluation = null;
    drawBarCharts();
    updateBarsButton.elt.textContent = "Evaluate the new well-plan";
    updateBarsButton.style('background-color', colorBarsFront);
    //TODO change the color
  }
}


function sliderAngleChange() {
  //relative angle here
  if (editNextAngleNo < nextAngles.length) {
    var prev = prevAngle(editNextAngleNo);
    nextAngles[editNextAngleNo] = allowedAngleRelative(prev, -angleSlider.value());
    for (var i = editNextAngleNo + 1; i < nextAngles.length; ++i) {
      prev += nextAngles[i - 1];
      nextAngles[i] = allowedAngleRelative(prev, nextAngles[i])
    }
    invalidateUserEvaluation();
  }
  //console.log(angleSlider.value());
  redrawEnabledForAninterval();

}

function updateSliderPosition() {
  //relative angle here
  if (editNextAngleNo < nextAngles.length) {
    angleSlider.value(-(nextAngles[editNextAngleNo]));
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
  detectGameStateAndUpdateButton();
  invalidateUserEvaluation();
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
    if (userdata.stopped) {
      return;
    }
    var submittedLen = userdata.wellPoints.length;
    var newAnglesLen = nextAngles.length;
    if (submittedLen + newAnglesLen < userdata.totalDecisionPoints) {
      //relative angle here
      // if (newAnglesLen === 0) {
      //   nextAngles.push(userdata.wellPoints[submittedLen - 1].angle);
      // } else {
      //   nextAngles.push(nextAngles[newAnglesLen - 1]);
      // }
      nextAngles.push(0.0);
    }
    detectGameStateAndUpdateButton();
    invalidateUserEvaluation();
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
    //console.log("scaled");
  }
  var maxAlpha = 255;
  geoModelBuffer.colorMode(RGB, maxAlpha);

  geoModelBuffer.background(0, 0, 0);
  geoModelBuffer.blendMode(ADD);
  //geoModelBuffer.strokeWeight(1);
  geoModelBuffer.noStroke();


  if (userdata != null) {
    //if (false){
    scaleBufferForView(geoModelBuffer, userdata);
    var reals = userdata.realizations;
    var realcount = reals.length;
    var alpha = Math.floor(maxAlpha / realcount);
    if (specificIndices != null) {
      var mult = Math.round(realcount / specificIndices.length);
      realcount = specificIndices.length;
      alpha *= mult;
    }
    //var alpha = 2 * (1.0 - Math.pow(0.5, 2 / reals.length));
    geoModelBuffer.noStroke();

    //geoModelBuffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
    geoModelBuffer.fill(maxAlpha, maxAlpha, maxAlpha, alpha);
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
  var geoModelHeight = wellHeigth;
  clear();
  image(geoModelBuffer, 0, wellBufferY, canvasWidth, geoModelHeight);

  //drawWellToBuffer();

  image(wellBuffer, 0, wellBufferY, canvasWidth, geoModelHeight);

  if (scaleBuffer != null) {
    image(scaleBuffer, 0, wellBufferY, canvasWidth, geoModelHeight);
  }

  if (barBuffer != null) {
    //console.log("draw bars");
    image(barBuffer, 0, barBufferY, canvasWidth, barBuffer.height);
    //image(barBuffer, 0, 10, 100, 100);
  }



  //for debugging
  //drawFrame();

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
  var thicknessMultLine = 4;

  //setup for main trajectory
  //wellBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
  //wellBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
  wellBuffer.stroke(colorOldWell);
  wellBuffer.fill(colorOldWell);
  wellBuffer.strokeWeight(thicknessMultLine * userdata.height / wellBuffer.height * 2);
  //wellBuffer.strokeWeight(2);
  var userPoints = userdata.wellPoints.slice(0);
  drawUserWellToBuffer(wellBuffer, userPoints);

  //main trajectory
  var x = userdata.wellPoints[userdata.wellPoints.length - 1].x;
  var y = userdata.wellPoints[userdata.wellPoints.length - 1].y;
  var angle = userdata.wellPoints[userdata.wellPoints.length - 1].angle;
  var xTravelDistance = userdata.xdist;

  for (var i = 0; i < nextAngles.length; i++) {
    if (i == 0) {
      //wellBuffer.stroke('rgba(100%, 0%, 0%, 1.0)');
      //wellBuffer.fill('rgba(100%, 0%, 0%, 1.0)');
      wellBuffer.stroke(colorDecision);
      wellBuffer.fill(colorDecision);
      wellBuffer.strokeWeight(thicknessMultLine * userdata.height / wellBuffer.height * 2);
      //wellBuffer.strokeWeight(userdata.doiY * thicknessMultLine * 2);
    }
    else {
      //wellBuffer.stroke('rgba(40%, 70%, 10%, 1.0)');
      //wellBuffer.fill('rgba(40%, 70%, 10%, 1.0)');
      wellBuffer.stroke(colorFutureOptions);
      wellBuffer.fill(colorFutureOptions);
      wellBuffer.strokeWeight(thicknessMultLine * userdata.height / wellBuffer.height);
    }
    //relative angle here
    angle += nextAngles[i];
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

  //wellBuffer.stroke('rgba(40%, 30%, 80%, 1.0)');
  //wellBuffer.fill('rgba(40%, 30%, 80%, 1.0)');
  wellBuffer.stroke(colorFutureOptions);
  wellBuffer.fill(colorFutureOptions);
  wellBuffer.strokeWeight(thicknessMultLine * userdata.height / wellBuffer.height * 0.5)


  //possible trajectory up
  if (nextAngles.length > 0) {
    x = userdata.wellPoints[userdata.wellPoints.length - 1].x;
    y = userdata.wellPoints[userdata.wellPoints.length - 1].y;
    //relative angle here
    var myAngle = nextAngles[0] + prevAngle(0);
    x = x + xTravelDistance;
    y = y + tan(myAngle) * xTravelDistance;
    var remainingLen = userdata.totalDecisionPoints - userdata.wellPoints.length;
    for (var i = 1; i < remainingLen; i++) {
      myAngle = myAngle + maxAngleChange;
      var x2 = x + xTravelDistance;
      var y2 = y + tan(myAngle) * xTravelDistance;
      // wellBuffer.line(
      //   x,
      //   y,
      //   x2,
      //   y2);
      dashedLine(wellBuffer, x, y, x2, y2, 0.1, 0.1);
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
    //relative angle here
    myAngle = nextAngles[0] + prevAngle(0);
    x = x + xTravelDistance;
    y = y + tan(myAngle) * xTravelDistance;
    for (var i = 1; i < remainingLen; i++) {
      myAngle = Math.max(0, myAngle - maxAngleChange);
      var x2 = x + xTravelDistance;
      var y2 = y + tan(myAngle) * xTravelDistance;
      // wellBuffer.line(
      //   x,
      //   y,
      //   x2,
      //   y2);
      dashedLine(wellBuffer, x, y, x2, y2, 0.1, 0.1);
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
  var angle = userdata.wellPoints[userdata.wellPoints.length - 1].angle;
  for (var i = 0; i < nextAngles.length; i++) {
    //relative angle here
    angle += nextAngles[i];
    var x2 = x + xTravelDistance;
    var y2 = y + tan(angle) * xTravelDistance;
    x = x2;
    y = y2;
    // wellBuffer.stroke('rgba(40%, 30%, 80%, 1.0)');
    // wellBuffer.fill('rgba(40%, 30%, 80%, 1.0)');
    //wellBuffer.stroke(colorFutureOptions);
    wellBuffer.noStroke();
    wellBuffer.fill(colorFutureOptions);
    if (editNextAngleNo === i) {
      //wellBuffer.stroke('rgba(100%, 100%, 0%, 1.0)');
      //wellBuffer.fill('rgba(100%, 100%, 0%, 1.0)');
      wellBuffer.fill(colorSelection);
    }
    else if (i === 0) {
      wellBuffer.fill(colorDecision);
    }
    wellBuffer.ellipse(x, y,
      userdata.doiX,
      userdata.doiY);
  }


  //var t1 = performance.now();
  //console.log("draw well to buffer " + (t1 - t0) + " milliseconds.");
}

function dashedLine(buffer, x1, y1, x2, y2, lPercent, gPercent) {
  //var pc = dist(x1, y1, x2, y2) / 100;
  //var pcCount = 1;

  //var lPercent = gPercent = 0;
  var currentPos = 0;
  var xx1 = yy1 = xx2 = yy2 = 0;

  // while (int(pcCount * pc) < l) {
  //   pcCount++
  // }
  // lPercent = pcCount;
  // pcCount = 1;
  // while (int(pcCount * pc) < g) {
  //   pcCount++
  // }
  // gPercent = pcCount;

  // lPercent = lPercent / 100;
  // gPercent = gPercent / 100;
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

    buffer.line(xx1, yy1, xx2, yy2);
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