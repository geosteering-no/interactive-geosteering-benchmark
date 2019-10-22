var timerCountdown = 1;
var prevButton;
var nextButton;

var colors = ['#66c2a5', '#fc8d62', '#8da0cb'];
var oldBest = [];

var restartButton;
var revealIndex = -1;
var scoreData = null;

var updateTotalScoreButton = null;
var stopUsersButton = null;
var resetUserScoresButton = null;
var scoreBoardDiv = null;

var progressBuffer = null;
var wellBuffer = null;
var geoModelBuffer = null;
var legendBuffer = null;
var oldLegendBuffer = null;

var curResultsAscending = null;
var prevBest = [];

function setup() {
	oneMarginInScript = 10;
	createCanvas(100, 100);
	prevButton = createButton("Hide");
	prevButton.mousePressed(buttonPreviousClick);

	nextButton = createButton("Reveal");
	nextButton.mousePressed(buttonNextClick);


	restartButton = createButton("New game");
	restartButton.position(100, 1400);
	restartButton.mousePressed(restartClick);

	stopUsersButton = createButton("Stop all users");
	stopUsersButton.position(200, 1400);
	stopUsersButton.mousePressed(stopUsersClick);

	resetUserScoresButton = createButton("Reset user scores");
	resetUserScoresButton.position(300, 1400);
	resetUserScoresButton.mousePressed(resetScoresClick);

	updateTotalScoreButton = createButton("Update total scores");
	updateTotalScoreButton.position(0, 1250);
	updateTotalScoreButton.mousePressed(updateScores);

	scoreBoardDiv = createDiv('Scores');
	scoreBoardDiv.position(0, 1300);


	setSizesAndPositions();


	drawGeomodelToBuffer();
	frameRate(4);
	//noLoop();
}

function draw() {
	//clear();
	noStroke();
	if (revealIndex >= 0) {
		clear();
		image(geoModelBuffer, 0, 0, geoModelBuffer.width, geoModelBuffer.height);
		//drawAllWells();
		image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.height);
		if (legendBuffer != null) {
			image(legendBuffer, canvasWidth - legendBuffer.width, 0, legendBuffer.width, legendBuffer.height);
		}
		if (oldLegendBuffer != null) {
			fill(200);
			//rect(canvasWidth - legendBuffer.width, legendBuffer.height, legendBuffer.width, legendBuffer.height);
			//oldLegendBuffer.image(legendBuffer, 0, 0, legendBuffer.width, legendBuffer.height);
			image(oldLegendBuffer, canvasWidth - legendBuffer.width, legendBuffer.height, legendBuffer.width, legendBuffer.height);
		}
	} else {
		clear();
		fill(0);
		rect(0, 0, geoModelBuffer.width, geoModelBuffer.height);
		drawAllProgress();
		image(progressBuffer, 0, 0, progressBuffer.width, progressBuffer.height);
	}

	//drawWellToBuffer();

	//image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.heigth);

	//for debugging
	drawFrame();

	timerCountdown--;
	if (timerCountdown <= 0) {
		timerCountdown = frameRate() * 10;
		if (revealIndex < 0) {
			fetchScoreData();
		}
	}
}


function updateScores() {
	if (scoreBoardDiv != null && scoreData != null) {
		var topScores = scoreData.userResults.slice(0)
			.sort(function (a, b) {
				var valueA = a.accumulatedScoreFromPreviousGames;
				var valueB = b.accumulatedScoreFromPreviousGames;
				return valueB - valueA;
			});
		var s = "Scores<br>";
		for (var i = 0; i < Math.min(5, topScores.length); ++i) {
			var shortUserName = topScores[i].userName;
			if (shortUserName.length > 30) {
				shortUserName = shortUserName.substr(0, 30);
			}
			s += topScores[i].userName + " : " + Math.round(topScores[i].accumulatedScoreFromPreviousGames) + "<br>";
		}
		scoreBoardDiv.html(s);
	}
}

function drawAllWells() {
	if (scoreData != null) {
		//oldLegendBuffer.resizeCanvas(legendBuffer.width, legendBuffer.height);
		//oldLegendBuffer.clear();
		//oldLegendBuffer.tint(255);
		//oldLegendBuffer.tint(100, 0.3);
		//oldLegendBuffer.image(legendBuffer, 0, 0, legendBuffer.width, legendBuffer.height);
		wellBuffer.clear();
		legendBuffer.clear();
		legendBuffer.noStroke();
		var textShift = 10;
		legendBuffer.textSize(textShift * 3);

		//var best = [];
		//wellBuffer.textSize(1);
		//wellBuffer.background(51, 51, 51, 0);
		//wellBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
		//wellBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
		//wellBuffer.strokeWeight(2 / scoreData.height);
		curResultsAscending = scoreData.userResults.slice(0)
			.sort(function (a, b) {
				var aLastInd = Math.min(a.trajectoryWithScore.length, revealIndex + 1) - 1;
				var bLastInd = Math.min(b.trajectoryWithScore.length, revealIndex + 1) - 1;
				var valueA = a.trajectoryWithScore[aLastInd].score;
				var valueB = b.trajectoryWithScore[bLastInd].score;
				return valueA - valueB;
			});
		for (var i = 0; i < curResultsAscending.length; ++i) {
			var fromTop = curResultsAscending.length - 1 - i;
			var userPoints = curResultsAscending[i].trajectoryWithScore.slice(0)
				.map(function (withScore) {
					return withScore.wellPoint;
				});
			if (fromTop < colors.length) {
				wellBuffer.stroke(colors[fromTop]);
				wellBuffer.fill(colors[fromTop]);
				wellBuffer.strokeWeight(5 / scoreData.height);
				var lastInd = Math.min(userPoints.length, revealIndex + 1) - 1;
				var score = curResultsAscending[i].trajectoryWithScore[lastInd].score;
				var shortUserName = curResultsAscending[i].userName;
				if (shortUserName.length > 30) {
					shortUserName = shortUserName.substr(0, 30);
				}

				legendBuffer.fill(colors[fromTop]);
				legendBuffer.textAlign(LEFT);
				legendBuffer.text(shortUserName + " : ", 0, textShift + (fromTop) * legendBuffer.height / colors.length,
					legendBuffer.width, legendBuffer.windowHeight / colors.length);
				legendBuffer.textAlign(RIGHT);
				legendBuffer.text(Math.round(score), 0, textShift + (fromTop) * legendBuffer.height / colors.length,
					legendBuffer.width, legendBuffer.windowHeight / colors.length);
				//best[userName] = null;
				// if (userName in prevBest){
				// 	prevBest
				// }
			} else {
				wellBuffer.stroke(220);
				wellBuffer.stroke(220);
				wellBuffer.strokeWeight(2 / scoreData.height);
			}

			drawUserWellToBuffer(wellBuffer, userPoints, revealIndex + 1);
			//var lastInd = Math.min(userPoints.length, revealIndex + 1) - 1;
			//var score = curResults[i].trajectoryWithScore[lastInd].score;
			//wellBuffer.text(score, userPoints[lastInd].x, userPoints[lastInd].y, 100, 30);
		}
	}
}

function drawAllProgress() {
	if (scoreData != null) {
		//progressBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
		progressBuffer.noStroke();
		//progressBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
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
			var progresFraction = (userProgressInd - 0.5) / scoreData.totalDecisionPoints;
			if (sortedResults[i].stopped) {
				progressBuffer.fill(colors[0]);
				// progressBuffer.text("stopped",
				// 	progresFraction * progressBuffer.width + shift,
				// 	i * oneHeight + shift * 2,
				// 	progresFraction * progressBuffer.width,
				// 	oneHeight - shift * 2);
			} else {
				progressBuffer.fill(colors[1]);
			}
			progressBuffer.rect(0, i * oneHeight + shift,
				progresFraction * progressBuffer.width,
				oneHeight - shift * 2);

			//wellBuffer.rect(50,40,800,800);
		}
	}
}

function updateButtonLabels() {
	if (scoreData != null) {
		if (revealIndex - 1 >= 0) {
			prevButton.html('<== ' + ((revealIndex - 1) / scoreData.totalDecisionPoints * 100) + '%');
		}
		else {
			prevButton.html('Hide');
			nextButton.html('Reveal');
		}
		if (revealIndex + 1 <= scoreData.totalDecisionPoints) {
			nextButton.html(((revealIndex + 1) / scoreData.totalDecisionPoints * 100) + '%' + ' ==>');
		}
		else {
			nextButton.html('Final step');
		}
	}
}

function buttonPreviousClick() {
	if (revealIndex > -1) {
		revealIndex--;

	}
	updateButtonLabels();
	updateAll();
}

function buttonNextClick() {
	if (scoreData != null) {
		if (revealIndex < scoreData.totalDecisionPoints) {
			revealIndex++;
		}
	}
	updateButtonLabels();
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

function stopUsersClick(){
	fetch("geo/stopall/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA",
		{
			credentials: 'include',
			method: 'POST'
		})
		.then(function (res) {
			if (!res.ok) {
				alert("stopping all users was not accepted?!");
				//throw Error("getting userdata failed");
			}
			else {
				console.log("stopping complete");
				fetchScoreData();
				//TODO consider making it impossible to add new points
				//TODO consider sending a message to user
			}
		});
}

function resetScoresClick(){
	fetch("geo/resetallscores/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA",
	{
		credentials: 'include',
		method: 'POST'
	})
	.then(function (res) {
		if (!res.ok) {
			alert("resetting all user scores was not accepted?!");
			//throw Error("getting userdata failed");
		}
		else {
			console.log("reseting complete");
			fetchScoreData();
			restartClick();
			//TODO consider making it impossible to add new points
			//TODO consider sending a message to user
		}
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
				revealIndex = -1;
				updateButtonLabels();
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

	var bufferHeight = Math.round(canvasHeigth / 8 * 7);
	var legendWidth = Math.round(canvasWidth / 3);
	var legendHeight = Math.round(canvasHeigth / 8);
	if (geoModelBuffer == null) {
		geoModelBuffer = createGraphics(canvasWidth, bufferHeight);
		wellBuffer = createGraphics(canvasWidth, bufferHeight);
		progressBuffer = createGraphics(canvasWidth, bufferHeight);
		legendBuffer = createGraphics(legendWidth, legendHeight);
		oldLegendBuffer = createGraphics(legendWidth, legendHeight);
	} else {
		geoModelBuffer.resizeCanvas(canvasWidth, bufferHeight);
		wellBuffer.resizeCanvas(canvasWidth, bufferHeight);
		progressBuffer.resizeCanvas(canvasWidth, bufferHeight);
		oldLegendBuffer.resizeCanvas(legendWidth, legendHeight);
		oldLegendBuffer.tint(100);
		oldLegendBuffer.image(legendBuffer, 0, 0, legendBuffer.width, legendBuffer.height);
		legendBuffer.resizeCanvas(legendWidth, legendHeight);
	}



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
	canvasWidth = Math.round(windowWidth - oneMarginInScript * 2);
	canvasHeigth = Math.round(windowHeight - oneMarginInScript);


	resizeCanvas(canvasWidth, canvasHeigth);

	// var bufferHeight = Math.round(canvasHeigth / 8 * 7);
	// var legendWidth = Math.round(canvasWidth / 3);
	// var legendHeight = Math.round(canvasHeigth / 8);

	// if (geoModelBuffer == null) {
	// 	geoModelBuffer = createGraphics(canvasWidth, bufferHeight);
	// 	wellBuffer = createGraphics(canvasWidth, bufferHeight);
	// 	progressBuffer = createGraphics(canvasWidth, bufferHeight);
	// 	legendBuffer = createGraphics(legendWidth, legendHeight);
	// 	oldLegendBuffer = createGraphics(legendWidth, legendHeight);
	// } else {
	// 	geoModelBuffer.resizeCanvas(canvasWidth, bufferHeight);
	// 	wellBuffer.resizeCanvas(canvasWidth, bufferHeight);
	// 	progressBuffer.resizeCanvas(canvasWidth, bufferHeight);
	// 	legendBuffer.resizeCanvas(legendWidth, legendHeight);
	// 	oldLegendBuffer.resizeCanvas(legendWidth, legendHeight);
	// }


	drawGeomodelToBuffer();

	prevButton.size(canvasWidth / 2 - 15, 100);
	prevButton.position(10, geoModelBuffer.height + 5);

	nextButton.position(canvasWidth / 2 + 5, geoModelBuffer.height + 5);
	nextButton.size(canvasWidth / 2 - 15, 100);

}


