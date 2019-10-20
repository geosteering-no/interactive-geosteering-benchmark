var timerCountdown = 1;
var prevButton;
var nextButton;
var wellBuffer;
var progresBuffer;
var restartButton;
var revealIndex = -1;
var scoreData = null;

function setup() {
	createCanvas(100, 100);
	prevButton = createButton("<- Previous");
	prevButton.mousePressed(buttonPreviousClick);

	nextButton = createButton("Next ->");
	nextButton.mousePressed(buttonNextClick);


	restartButton = createButton("New game");
	restartButton.position(0, 450);
	restartButton.mousePressed(restartClick);

	setSizesAndPositions();


	drawGeomodelToBuffer();
	//noLoop();
}

function draw() {
	//clear();
	if (revealIndex >= 0) {
		
		image(geoModelBuffer, 0, 0, geoModelBuffer.width, geoModelBuffer.height);
		drawAllWells();
		image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.height);
	} else {
		fill(255);
		geoModelBuffer.rect( 0, 0, geoModelBuffer.width, geoModelBuffer.height);
		drawAllProgress();
		image(progressBuffer, 0, 0, progressBuffer.width, progressBuffer.height);
	}

	//drawWellToBuffer();

	//image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.heigth);

	//for debugging
	drawFrame();

	timerCountdown--;
	if (timerCountdown <= 0) {
		timerCountdown = 60 * 10;
		fetchScoreData();
	}
}

function drawProgressToWellBuffer() {
	var buffer = wellBuffer;
}

function drawAllWells() {
	if (scoreData != null) {
		wellBuffer.background(51, 51, 51, 0);
		wellBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
		wellBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
		wellBuffer.strokeWeight(2 / scoreData.height);
		for (var i = 0; i < scoreData.userResults.length; ++i) {
			var userPoints = scoreData.userResults[i].trajectoryWithScore.slice(0)
				.map(function (withScore) {
					return withScore.wellPoint;
				});
			drawUserWellToBuffer(wellBuffer, userPoints, revealIndex);
		}
	}
}

function drawAllProgress() {
	if (scoreData != null) {
		//progressBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
		progressBuffer.noStroke();
		progressBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
		//wellBuffer.strokeWeight(2 / scoreData.height);
		var sortedResults = scoreData.userResults.slice(0).sort(function (a, b) {
			if (a.stopped != b.stopped) {
				if (a.stopped) {
					return -1;
				}
				if (b.stopped) {
					return 1;
				}
			}
			var aLen = a.trajectoryWithScore.length;
			var bLen = b.trajectoryWithScore.length;
			return bLen - aLen;
		});
		var totalUsers = sortedResults.length;
		var oneHeight = progressBuffer.height / totalUsers;
		var shift = oneHeight / 6;

		for (var i = 0; i < totalUsers; i++) {
			var userProgressInd = sortedResults[i].trajectoryWithScore.length;
			var progresFraction = userProgressInd / scoreData.totalSteps;
			progressBuffer.rect(0, i * oneHeight + shift,
				progresFraction * progressBuffer.width,
				oneHeight - shift * 2);
			if (sortedResults[i].stopped) {
				progressBuffer.text("stopped",
					progresFraction * progressBuffer.width + shift,
					i * oneHeight + shift * 2,
					progresFraction * progressBuffer.width,
					oneHeight - shift * 2);
			}
			//wellBuffer.rect(50,40,800,800);
		}
	}
}

function buttonPreviousClick() {
	if (revealIndex > -1) {
		revealIndex--;
	}
	updateAll();
}

function buttonNextClick() {
	if (scoreData != null) {		
		if (revealIndex < scoreData.totalSteps) {
			revealIndex++;
		}
	}
	updateAll();
}

function updateAll() {
	if (scoreData != null)
		drawGeomodelToBuffer(scoreData);
	else
		drawGeomodelToBuffer(null);
	drawAllWells();
}

function getPopulationData() {

}

function fetchScoreData() {
	fetch("/geo/admin/scores/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA", { credentials: 'include' })
		.then(function (res) {
			if (!res.ok) {
				alert("getting score failed");
				throw Error("getting score failed");
			}
			res.json()
				.then(function (json) {
					console.log("got score:" + JSON.stringify(json));
					scoreData = json;
					drawGeomodelToBuffer(scoreData);
					//drawAllWells();
				});
		});
}

function restartClick() {
	fetch("geo/restart/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA",
		{
			credentials: 'include',
			method: 'POST'
		})
		.then(function (res) {
			if (!res.ok) {
				alert("restarting was not accepted?!");
				//throw Error("getting userdata failed");
			}
			else {
				console.log("restart complete");
				//TODO consider making it impossible to add new points
				//TODO consider sending a message to user
			}
		});

}

function sliderAngleChange() {

}

function windowResized() {
	setSizesAndPositions();
}



function drawGeomodelToBuffer(scoredata) {

	geoModelBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
	wellBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);
	progressBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 3);

	barBuffer = createGraphics(canvasWidth, canvasHeigth / 8 * 2);



	if (scoredata != null) {
		scaleBufferForView(wellBuffer, scoredata);
		console.log("scaled");
	}

	geoModelBuffer.background(0, 0, 0);
	geoModelBuffer.blendMode(ADD);
	geoModelBuffer.strokeWeight(1);


	if (scoredata != null) {
		//if (false){
		scaleBufferForView(geoModelBuffer, scoredata);
		console.log("drawing userdat");
		console.log("this is slow?");


		var alpha = 0.3;
		//TODO this formula needs improvement
		//var alpha = 2 * (1.0 - Math.pow(0.5, 2 / reals.length));
		geoModelBuffer.noStroke();
		geoModelBuffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
		var xlist = scoredata.secretRealization.xList;
		var realization = scoredata.secretRealization;
		drawRealizationToBuffer(geoModelBuffer, xlist, realization);

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
		console.log("drawing triangles");
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


function setSizesAndPositions() {
	canvasWidth = windowWidth - oneMarginInScript * 2;
	canvasHeigth = windowHeight - oneMarginInScript;


	resizeCanvas(canvasWidth, canvasHeigth);

	drawGeomodelToBuffer();

	prevButton.size(canvasWidth / 2 - 15, 100);
	prevButton.position(10, geoModelBuffer.height + 5);

	nextButton.position(canvasWidth / 2 + 5, geoModelBuffer.height + 5);
	nextButton.size(canvasWidth / 2 - 15, 100);

}


