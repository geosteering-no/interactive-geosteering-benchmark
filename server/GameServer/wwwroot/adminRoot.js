var timerCountdown = 1;
var prevButton;
var nextButton;
var angleSlider;
var restartButton;
var revealIndex = 0;
var maxIndex = 10;

function setup() {
	createCanvas(100, 100);
	prevButton = createButton("<- Previous");
	prevButton.mousePressed(buttonPreviousClick);

	nextButton = createButton("Next ->");
	nextButton.mousePressed(buttonNextClick);

	angleSlider = createSlider(0, 10, 0, 0);
	angleSlider.input(sliderAngleChange);
	angleSlider.style('width', '280px');
	angleSlider.style('height', '180px');

	restartButton = createButton("New game");
	restartButton.position(0,450);
	restartButton.mousePressed(restartClick);

	setSizesAndPositions();


	//drawGeomodelToBuffer();
	noLoop();
}

function draw() {
	clear();
	image(geoModelBuffer, 0, 0, geoModelBuffer.width, geoModelBuffer.heigth);

	//drawWellToBuffer();

	//image(wellBuffer, 0, 0, wellBuffer.width, wellBuffer.heigth);

	//for debugging
	drawFrame();

	timerCountdown--;
	if (timerCountdown <= 0) {
		noLoop();
	}
}

function buttonPreviousClick() {
	if (revealIndex > 0) {
		revealIndex--;
	}
	redrawEnabledForAninterval();
}

function buttonNextClick() {
	if (revealIndex < maxIndex) {
		revealIndex++;
	}
	redrawEnabledForAninterval();
}

function restartClick() {
	fetch("geo/restart",
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

function setSizesAndPositions() {
	canvasWidth = windowWidth - oneMarginInScript * 2;
	canvasHeigth = windowHeight - oneMarginInScript;
	if (canvasWidth > canvasHeigth) {
		canvasWidth = canvasHeigth / 4 * 3;
	}

	resizeCanvas(canvasWidth, canvasHeigth);

	drawGeomodelToBuffer();

	prevButton.size(canvasWidth / 2 - 15, 100);
	prevButton.position(10, geoModelBuffer.height + 5);

	nextButton.position(canvasWidth / 2 + 5, geoModelBuffer.height + 5);
	nextButton.size(canvasWidth / 2 - 15, 100);

	angleSlider.position(80, geoModelBuffer.height + prevButton.height + 10);
	angleSlider.size(canvasWidth - 80 * 2, 50);
}


