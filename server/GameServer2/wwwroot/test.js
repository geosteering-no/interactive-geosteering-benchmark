var userdata = null;
var wellBuffer;
var barBuffer;
var realizationScores;
var canvas = null;

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
var geoModelBuffer;

function setup() {
	canvas = createCanvas(500, 500);
	setSizesAndPositions();
	geoModelBuffer = createGraphics(canvasWidth, Math.round(canvasHeight / 8 *3) );
	//wellBuffer = 
	//buff.ellipse(50, 50, 80, 80);

	buff2 = createGraphics(400, 400);
	buff2.ellipse(50, 50, 80, 80);

	getUserData();
}

function draw() {
  if (geoModelBuffer != null)
  	image(geoModelBuffer, 0, 0, canvasWidth, canvasHeigth/8*3);



  image(buff2, 100, 0, 100, 100);
}


function windowResized() {
  setSizesAndPositions();
  //redrawEnabledForAninterval();
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
          //console.log("got userdata:" + JSON.stringify(json));
          userdata = json;

          //setSizesAndPositions();
          //updateBars();
          drawGeomodelToBuffer(userdata);
          //redrawEnabledForAninterval();
        });
    });
}

function setSizesAndPositions() {
  canvasWidth = windowWidth - oneMarginInScript * 2 - 50;
  canvasHeight = windowHeight - oneMarginInScript -50;
  if (canvasWidth > canvasHeight) {
    canvasWidth = canvasHeigth / 4 * 3;
  }

  resizeCanvas(canvasWidth, canvasHeight);

  var yPos = 0;
  var yMargin = 5;

  function goDown(heigth) {
    yPos = yPos + heigth + yMargin;
  }
  
  // var submitHeight = canvasHeigth * 0.05;
  // // submitDecisionButton.position(10, yPos);
  // // submitDecisionButton.size(canvasWidth -20, submitHeight);

  // goDown(submitHeight);

  // resizeCanvas(canvasWidth, canvasHeigth);

  wellBufferY = yPos;
  // drawGeomodelToBuffer(userdata);

  // goDown(geoModelBuffer.height);

  // var buttonHeigth = geoModelBuffer.height / 5;
  // prevButton.position(10, yPos);
  // prevButton.size(canvasWidth / 3 - 15, buttonHeigth);

  // stopButton.position(canvasWidth / 3 + 5, yPos);
  // stopButton.size(canvasWidth / 3 - 10, buttonHeigth);

  // nextButton.position(canvasWidth - canvasWidth / 3  + 5, yPos);
  // nextButton.size(canvasWidth / 3 - 15, buttonHeigth);

  // goDown(buttonHeigth+50);

  // var sliderHeigth = 50;
  // var offsetSlider = 40;
  // angleSlider.position(canvasWidth*0.1, yPos);
  // angleSlider.size(canvasWidth*0.75 , sliderHeigth);

  // goDown(sliderHeigth);

  // barBufferY = yPos;
  // drawBarCharts();
  // goDown(barBuffer.height);

  // updateBarsButton.position(5, yPos);
  // updateBarsButton.size(canvasWidth -10, canvasHeigth * 0.07);

  // redrawEnabledForAninterval();
}

function drawGeomodelToBuffer(userdata, specificIndices) {

  //geoModelBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
  //wellBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
  //barBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 2);

  //barBuffer.mousePressed(press);


  if (userdata != undefined) {
      //scaleBufferForView(wellBuffer, userdata);
      //console.log("scaled");
  }

  geoModelBuffer.background(0, 0, 0);
  geoModelBuffer.blendMode(ADD);
  geoModelBuffer.strokeWeight(1);


  if (userdata != undefined) {
      //if (false){
      scaleBufferForView(geoModelBuffer, userdata);
      //console.log("drawing userdat");
      var reals = userdata.realizations;
      var realcount = reals.length;
      if (specificIndices != undefined) realcount = specificIndices.length;
      var alpha = 1.0 / realcount;
      //TODO this formula needs improvement
      //var alpha = 2 * (1.0 - Math.pow(0.5, 2 / reals.length));
      geoModelBuffer.noStroke();
      geoModelBuffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
      var xlist = userdata.xList;
      if (specificIndices == undefined) {
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