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

var timerCountdown = 1;
var revealedPoints = 0;
var prevButton;
var nextButton;

//three-color scheme
//var colors = ['#66c2a5', '#fc8d62', '#8da0cb'];
var colors = ['#a6cee3', '#1f78b4', '#b2df8a'];
var colorBest = '#33a02c';
var oldBest = [];

var allWellData = null;
var userData = null;
var legendOffsetTop = 40;
//this is single user for now
let animatedWellPoints = null;

var canvasWidth;
var canvasHeigth;

var loadScoreButton = null;
var scoreFileNameInput = null;

var progressBuffer = null;
var wellBuffer = null;
var geoModelBuffer = null;
var legendBuffer = null;
var oldLegendBuffer = null;
var scaleBuffer = null;

var finalTime = 0;
var totalTimerTime;

var curResultsAscending = null;
var prevBest = [];

function setup() {
	oneMarginInScript = 10;
	createCanvas(100, 100);
	prevButton = createButton("Update");
	prevButton.mousePressed(buttonPreviousClick);
	prevButton.style('font-size', '28px');

	nextButton = createButton("Reveal");
	nextButton.mousePressed(buttonNextClick);
	nextButton.style('font-size', '28px');

	// loadScoreButton = createButton("Load scores");
	// loadScoreButton.position(100, 1400);
	//loadScoreButton.mousePressed(loadOldScoresClick);

	// scoreFileNameInput = createInput('game name number');
	// scoreFileNameInput.position(200, 1400);

	// showBotCheckBox = createCheckbox('Show bot', false);
	// showBotCheckBox.position(700, 1400);





	finalTime = new Date().getTime() + 15 * 60 * 1000;
	fetchUserData();
	

	setSizesAndPositions();


	drawGeomodelToBuffer();
	frameRate(30);
	totalTimerTime = 30 * 10;
	//noLoop();
}

function animateWell(wellBuffer, committedPoints){
	if (committedPoints){
		var len = committedPoints.length;
		if (len < 2){
			return;
		}

		let lambda = 1 - timerCountdown / totalTimerTime;
		let finalPoint = {};
		finalPoint.x = committedPoints[len-1].x * lambda + committedPoints[len-2].x * (1-lambda);
		finalPoint.y = committedPoints[len-1].y * lambda + committedPoints[len-2].y * (1-lambda);

		
		for (var i = 0; i < len; i++) {
			var point = committedPoints[i];
			var prev = point;
			if (i > 0) prev = committedPoints[i - 1];
			if (i >= len-1){
				point = finalPoint;
				// point.x = point.x * lambda + prev.x * (1-lambda);
				// point.y = point.y * lambda + prev.y * (1-lambda);
			}
			wellBuffer.line(
				point.x,
				point.y,
				prev.x,
				prev.y);
			//circlecircle()
		}
		wellBuffer.stroke(0);
		wellBuffer.noFill();
		wellBuffer.strokeWeight(0.1);
		for (var i = 0; i < len; i++) {
			var point = {x: committedPoints[i].x, y: committedPoints[i].y};
			point.x += random(-0.03, 0.03);
			point.y += random(-0, 0.3);
			if (i >= len-1){
				point = finalPoint;
				point.x +=  random(-0.5, 0.03);
				// point.x = point.x * lambda + prev.x * (1-lambda);
				// point.y = point.y * lambda + prev.y * (1-lambda);
			}
			if (i > 0){
				wellBuffer.line(
					point.x,
					point.y,
					prev.x,
					prev.y);
			}
			var prev = point;
		}
	}
}

function draw() {
	//clear();
	var now = new Date().getTime();
	var t = finalTime - now;
	var mins = Math.floor((t % (1000 * 60 * 60)) / (1000 * 60));
	var secs = Math.floor((t % (1000 * 60)) / 1000);
	var secsStr = "0" + secs;
	if (secsStr.length > 2) {
		secsStr = secsStr.substring(secsStr.length - 2);
	}
	noStroke();
	// if (revealIndex >= 0) 
	{
		clear();
		//geoModelBuffer.resetMatrix();
		image(geoModelBuffer, 0, 0, geoModelBuffer.width, geoModelBuffer.height);
		drawAllWells();
		if (animatedWellPoints){
			pointsCopy = animatedWellPoints.slice(0, revealedPoints);
			animateWell(wellBuffer, pointsCopy);
		}

		image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.height);
		image(scaleBuffer, 0, 0, scaleBuffer.width, scaleBuffer.height);

		if (legendBuffer != null) {
			image(legendBuffer, canvasWidth - legendBuffer.width, legendOffsetTop,
				legendBuffer.width, legendBuffer.height);
		}

	} 
	// else {
	// 	clear();
	// 	fill(0);
	// 	rect(0, 0, geoModelBuffer.width, geoModelBuffer.height);
	// 	drawAllProgress();
	// 	image(progressBuffer, 0, 0, progressBuffer.width, progressBuffer.height);
	// 	nextButton.html('Remaining time: ' + mins + ":" + secsStr);
	// }

	//drawWellToBuffer();

	//image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.heigth);

	//for debugging
	//drawFrame();





	timerCountdown--;
	if (timerCountdown <= 0) {
		timerCountdown = totalTimerTime;
		if (animatedWellPoints === null || animatedWellPoints === undefined || revealedPoints >= animatedWellPoints.length){
			fetchScoreData();
		}else{
			revealedPoints++;
		}
	}
}


function updateScores() {
	// if (scoreBoardDiv != null && scoreData != null) {
	// 	var topScores = scoreData.userResults.slice(0)
	// 		.sort(function (a, b) {
	// 			var valueA = a.accumulatedScoreFromPreviousGames;
	// 			var valueB = b.accumulatedScoreFromPreviousGames;
	// 			//var valueA = a.accumulatedScorePercentFromPreviousGames;
	// 			//var valueB = b.accumulatedScorePercentFromPreviousGames;
	// 			return valueB - valueA;
	// 		});
	// 	var topPercents = scoreData.userResults.slice(0)
	// 		.sort(function (a, b) {
	// 			//var valueA = a.accumulatedScoreFromPreviousGames;
	// 			//var valueB = b.accumulatedScoreFromPreviousGames;
	// 			var valueA = a.accumulatedScorePercentFromPreviousGames;
	// 			var valueB = b.accumulatedScorePercentFromPreviousGames;
	// 			return valueB - valueA;
	// 		});

	// 	var s = "Total top <br>";
	// 	for (var i = 0; i < Math.min(5, topScores.length); ++i) {
	// 		var shortUserName = topScores[i].userName;
	// 		if (shortUserName.length > 30) {
	// 			shortUserName = shortUserName.substr(0, 30);
	// 		}
	// 		s += shortUserName + " : " + Math.round(topScores[i].accumulatedScoreFromPreviousGames) + "<br>";
	// 	}
	// 	s += "Percent top <br>";
	// 	for (var i = 0; i < Math.min(5, topPercents.length); ++i) {
	// 		var shortUserName = topPercents[i].userName;
	// 		if (shortUserName.length > 30) {
	// 			shortUserName = shortUserName.substr(0, 30);
	// 		}
	// 		s += shortUserName + " : " + Math.round(topPercents[i].accumulatedScorePercentFromPreviousGames * 100) + "<br>";
	// 	}
	// 	scoreBoardDiv.html(s);
	// }
}



function drawAllWells() {
	if (allWellData != null && userData != null) {
		//oldLegendBuffer.resizeCanvas(legendBuffer.width, legendBuffer.height);
		//oldLegendBuffer.clear();
		//oldLegendBuffer.tint(255);
		//oldLegendBuffer.tint(100, 0.3);
		//oldLegendBuffer.image(legendBuffer, 0, 0, legendBuffer.width, legendBuffer.height);
		wellBuffer.clear();
		legendBuffer.clear();
		legendBuffer.fill(0, 0, 0, 100);
		legendBuffer.rect(0, 0, legendBuffer.width, legendBuffer.height);

		legendBuffer.noStroke();

		//var legendLength = colors.length + 1;
		var legendLength = 1;
		var textShift = 10;
		var textHeight = (legendBuffer.height - textShift * 2) / legendLength;
		legendBuffer.textSize(textHeight);
		var thicknessMultLine = 7;

		curResultsAscending = allWellData.userResults.slice(0)
			.sort(function (a, b) {
				return a.timeTicks - b.timeTicks;
				var aLastInd = a.trajectoryWithScore.length - 1;
				var bLastInd = b.trajectoryWithScore.length - 1;
				if (aLastInd < 0) {
					return 1;
				}
				if (bLastInd < 0) {
					return -1;
				}
				var valueA = a.trajectoryWithScore[aLastInd].score;
				var valueB = b.trajectoryWithScore[bLastInd].score;
				return valueA - valueB;
			});

		bestScore = 1;
		// if (allWellData.bestPossible != null){
		// 	var userPoints = allWellData.bestPossible.trajectoryWithScore.slice(0)
		// 	.map(function (withScore) {
		// 		return withScore.wellPoint;
		// 	});
		// 	var lastInd = userPoints.length - 1;
		// 	bestScore = allWellData.bestPossible.trajectoryWithScore[lastInd].score / 100.0;
		// }
		let skipWell = false;
		for (var i = 0; i < curResultsAscending.length; ++i) {
			skipWell = false;
			var fromTop = curResultsAscending.length - 1 - i;
			var userPoints = curResultsAscending[i].trajectoryWithScore.slice(0);
			if (fromTop < 1) {
				wellBuffer.stroke(colors[fromTop]);
				wellBuffer.fill(colors[fromTop]);
				wellBuffer.strokeWeight(thicknessMultLine * userData.height / wellBuffer.height * 2);

				animatedWellPoints = userPoints;
				skipWell = true;

				var lastInd = userPoints.length - 1;
				var score = 0;
				if (lastInd >= 0) {
					score = curResultsAscending[i].trajectoryWithScore[lastInd].score;
				}
				var shortUserName = curResultsAscending[i].userName;
				if (shortUserName.length > 22) {
					shortUserName = shortUserName.substr(0, 22);
				}

				legendBuffer.fill(colors[fromTop]);
				legendBuffer.textAlign(LEFT);
				legendBuffer.text(shortUserName, 0, textShift + (fromTop) * legendBuffer.height / legendLength,
					legendBuffer.width, legendBuffer.height / legendLength);
				legendBuffer.textAlign(RIGHT);
				// legendBuffer.text(Math.round(score) , 0, textShift + (fromTop) * legendBuffer.height / legendLength,
				// 	legendBuffer.width, legendBuffer.height / legendLength);
				
				//best[userName] = null;
				// if (userName in prevBest){
				// 	prevBest
				// }
			} 
			// else if (fromTop == Math.round(curResultsAscending.length/2.0)){
			// 	fromTop = 1;
			// 	wellBuffer.stroke(colors[fromTop]);
			// 	wellBuffer.fill(colors[fromTop]);
			// 	wellBuffer.strokeWeight(thicknessMultLine * userData.height / wellBuffer.height * 2);
			// 	var lastInd = userPoints.length - 1;
			// 	var score = 0;
			// 	if (lastInd >= 0) {
			// 		score = curResultsAscending[i].trajectoryWithScore[lastInd].score;
			// 	}
			// 	var shortUserName = curResultsAscending[i].userName;
			// 	if (shortUserName.length > 18) {
			// 		shortUserName = shortUserName.substr(0, 18);
			// 	}

			// 	legendBuffer.fill(colors[fromTop]);
			// 	legendBuffer.textAlign(LEFT);
			// 	legendBuffer.text(shortUserName + " (median):", 0, textShift + (fromTop) * legendBuffer.height / legendLength,
			// 		legendBuffer.width * 0.75, legendBuffer.height / legendLength);
			// 	legendBuffer.textAlign(RIGHT);
			// 	legendBuffer.text(Math.round(score) + "" , 0, textShift + (fromTop) * legendBuffer.height / legendLength,
			// 		legendBuffer.width, legendBuffer.height / legendLength);
			// } 
			else {
				wellBuffer.stroke(220, 220, 220, 100);
				wellBuffer.strokeWeight(thicknessMultLine * userData.height / wellBuffer.height);
			}
			
			// TODO add link
			if (!skipWell){
				drawUserWellToBuffer(wellBuffer, userPoints);
			}


			//var lastInd = Math.min(userPoints.length, revealIndex + 1) - 1;
			//var score = curResults[i].trajectoryWithScore[lastInd].score;
			//wellBuffer.text(score, userPoints[lastInd].x, userPoints[lastInd].y, 100, 30);
		}

		//best trajectory
		// if (allWellData.bestPossible != null) {
		// 	wellBuffer.stroke(colorBest);
		// 	wellBuffer.fill(colorBest);
		// 	wellBuffer.strokeWeight(thicknessMultLine * allWellData.height / wellBuffer.height * 2);
		// 	var userPoints = allWellData.bestPossible.trajectoryWithScore.slice(0)
		// 		.map(function (withScore) {
		// 			return withScore.wellPoint;
		// 		});
		// 	var lastInd = userPoints.length - 1;
		// 	var score = 0;
		// 	if (lastInd >= 0) {
		// 		score = allWellData.bestPossible.trajectoryWithScore[lastInd].score;
		// 	}
		// 	var shortUserName = allWellData.bestPossible.userName;
		// 	if (shortUserName.length > 30) {
		// 		shortUserName = shortUserName.substr(0, 30);
		// 	}

		// 	var fromTop = 3;
		// 	legendBuffer.fill(colorBest);
		// 	legendBuffer.textAlign(LEFT);
		// 	legendBuffer.text(shortUserName + " : ", 0, textShift + (fromTop) * legendBuffer.height / legendLength,
		// 		legendBuffer.width * 0.75, legendBuffer.windowHeight / legendLength);
		// 	legendBuffer.textAlign(RIGHT);
		// 	legendBuffer.text(Math.round(score / bestScore) + "%", 0, textShift + (fromTop) * legendBuffer.height / legendLength,
		// 		legendBuffer.width, legendBuffer.windowHeight / legendLength);
		// 	//best[userName] = null;
		// 	// if (userName in prevBest){
		// 	// 	prevBest
		// 	// }


		// 	drawUserWellToBuffer(wellBuffer, userPoints, revealIndex + 1);
		// }
	}
}

function drawAllProgress() {
	if (allWellData != null && userData != null) {
		//progressBuffer.stroke('rgba(50%, 50%, 0%, 1.0)');
		progressBuffer.noStroke();
		//progressBuffer.fill('rgba(50%, 50%, 0%, 1.0)');
		//wellBuffer.strokeWeight(2 / scoreData.height);
		var sortedResults = allWellData.userResults.slice(0).sort(function (a, b) {
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
			var progresFraction = (userProgressInd - 0.5) / userData.totalDecisionPoints;
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
	prevButton.html('Update');
	nextButton.html('TODO: new name');
}

function buttonPreviousClick() {
	fetchScoreData();
	updateButtonLabels();
	updateAll();
}

function buttonNextClick() {

	updateButtonLabels();
	updateAll();
}

function updateAll() {
	if (userData != null)
		drawGeomodelToBuffer(userData);
	else
		drawGeomodelToBuffer(null);
	drawAllWells();
}

function getPopulationData() {

}

// function loadOldScoresClick() {
// 	var bodyString = JSON.stringify(scoreFileNameInput.value());
// 	fetch("/geo/admin/load/iERVaNDsOrphIcATHOrSeRlabLYpoIcESTawLstenTESTENTIonosterTaKOReskICIMPLATeRnA", 
// 		{ 
// 			credentials: 'include',
// 			method: 'POST',
// 			headers: {
// 				'Content-Type': 'application/json; charset=UTF-8'
// 				// 'Content-Type': 'application/x-www-form-urlencoded',
// 			  },
// 			  body: bodyString
// 		})
// 		.then(function (res) {
// 			if (!res.ok) {
// 				alert("getting score failed");
// 				throw Error("loading score failed");
// 			}
// 			res.json()
// 				.then(function (json) {
// 					console.log("got score:" + JSON.stringify(json));
// 					allWellData = json;
// 					//drawGeomodelToBuffer(allWellData);
// 					//drawAllWells();
// 				});
// 		});
// }

function fetchUserData() {
	fetch("/geo/userdatadefault", 
		{ credentials: 'include' }
		)
		.then(function (res) {
			if (!res.ok) {
				alert("getting score failed");
				throw Error("getting score failed");
			}
			res.json()
				.then(function (json) {
					console.log("got score:" + JSON.stringify(json));
					userData = json;
					drawGeomodelToBuffer(userData);
					fetchScoreData();
				});
		});
}


function fetchScoreData() {
	fetch("/geo/screen", 
		{ credentials: 'include' }
		)
		.then(function (res) {
			if (!res.ok) {
				alert("getting score failed");
				throw Error("getting score failed");
			}
			res.json()
				.then(function (json) {
					console.log("got score:" + JSON.stringify(json));
					allWellData = json;
					revealedPoints = 2;
					drawAllWells();
				});
		});
}

function windowResized() {
	setSizesAndPositions();
	updateAll();
}

function drawGeomodelToBuffer(scoredata) {

	var bufferHeight = Math.round(canvasHeigth / 8 * 7);
	var legendWidth = Math.round(canvasWidth / 3);
	var legendHeight = Math.round(canvasHeigth / 15);
	if (geoModelBuffer == null) {
		geoModelBuffer = createGraphics(canvasWidth, bufferHeight);
		wellBuffer = createGraphics(canvasWidth, bufferHeight);
		progressBuffer = createGraphics(canvasWidth, bufferHeight);
		scaleBuffer = createGraphics(canvasWidth, bufferHeight);
		legendBuffer = createGraphics(legendWidth, legendHeight);
		oldLegendBuffer = createGraphics(legendWidth, legendHeight);

	} else {
		geoModelBuffer.resizeCanvas(canvasWidth, bufferHeight);
		wellBuffer.resizeCanvas(canvasWidth, bufferHeight);
		scaleBuffer.resizeCanvas(canvasWidth, bufferHeight);
		progressBuffer.resizeCanvas(canvasWidth, bufferHeight);
		oldLegendBuffer.resizeCanvas(legendWidth, legendHeight);
		oldLegendBuffer.tint(100);
		oldLegendBuffer.image(legendBuffer, 0, 0, legendBuffer.width, legendBuffer.height);
		legendBuffer.resizeCanvas(legendWidth, legendHeight);
	}




	if (scoredata != null) {
		scaleBufferForView(wellBuffer, scoredata);
		drawScale(scaleBuffer, scoredata, 25, colorInformation);

		//console.log("scaled");
	}

	geoModelBuffer.background(0, 0, 0);
	geoModelBuffer.blendMode(ADD);
	//geoModelBuffer.strokeWeight(1);
	geoModelBuffer.noStroke();


	if (scoredata != null) {
		var maxAlpha = 255;
		geoModelBuffer.colorMode(RGB, maxAlpha);
		//if (false){
		scaleBufferForView(geoModelBuffer, scoredata);
		//console.log("drawing userdat");
		//console.log("this is slow?");
		var reals = scoredata.realizations;
		var realcount = reals.length;
		var alpha = Math.floor(maxAlpha / realcount);


		  //var alpha = 2 * (1.0 - Math.pow(0.5, 2 / reals.length));
		geoModelBuffer.noStroke();
	  
		  //geoModelBuffer.fill('rgba(100%, 100%, 100%, ' + alpha + ')');
		geoModelBuffer.fill(maxAlpha, maxAlpha, maxAlpha, alpha);

		var xlist = scoredata.xList;
		
		for (var i=0; i<scoredata.realizations.length; ++i){
			var realization = scoredata.realizations[i];
			drawRealizationToBuffer(geoModelBuffer, xlist, realization);
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


	drawGeomodelToBuffer(userData);

	prevButton.size(canvasWidth / 2 - 15, 100);
	prevButton.position(10, geoModelBuffer.height + 5);

	nextButton.position(canvasWidth / 2 + 5, geoModelBuffer.height + 5);
	nextButton.size(canvasWidth / 2 - 15, 100);

}


