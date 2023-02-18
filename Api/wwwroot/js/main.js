"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/communication").build();
var colors = ['dark grey', 'grey', 'grey', 'grey', 'light grey', 'white'];
var mouseX = 0;
var mouseY = 0;

var lastMouseX = 0;
var lastMouseY = 0;
var clearCanvasOn = true;
var sendMouseOn = false;

connection.on("ReceiveMessage", function (user, message) {
    // console.log(message)
    var objects = message.split("!")
    var elapsedTime = objects[0].split(",");
    var centroidCollection = objects[1].split(";");
    var droneOversight = objects[2].split(",");
    var herders = objects[3].split(";");
    var sheeps = objects[4].split(";");
    var vectorCollection = objects[5].split(";");
    var circle = objects[6].split(";");
    var pathCoordinates = objects[7].split(";");
    var pathCoordinatesAchieved = objects[8].split(";");
    var commandsCoordinates = objects[9].split(";");
    var state = objects[10];
    var terrainPath = objects[11].split(";");

    printElapsedTime(elapsedTime);
    printState(state);
    if (clearCanvasOn) {
        clearCanvas();
    }

    if (clearCanvasOn) {

        for (let i = 0; i < pathCoordinates.length; i++) {
            let line = pathCoordinates[i].split(",");
            drawVector(line[0], line[1], line[2], line[3], 'grey', 5)
        }

        for (let i = 0; i < terrainPath.length - 1; i++) {
            let from = terrainPath[i].split(",")
            let to = terrainPath[i + 1].split(",")
            drawVector(from[0], from[1], to[0], to[1], 'black', 3)
        }

        for (let i = 0; i < pathCoordinatesAchieved.length - 1; i++) {
            let from = pathCoordinatesAchieved[i].split(",")
            let to = pathCoordinatesAchieved[i + 1].split(",")
            drawVector(from[0], from[1], to[0], to[1], 'lightgrey', 4)
        }

        drawCircle(circle[0], circle[1], circle[2], "black", 0.25)

        for (let i = 0; i < vectorCollection.length; i++) {
            let vector = vectorCollection[i].split(",")
            drawVector(vector[0], vector[1], vector[2], vector[3], 'black', 0.25)
        }
    }
    drawPoint(droneOversight[0], droneOversight[1], "lightgrey", 15)

    for (let i = 0; i < herders.length; i++) {
        let coordinates = herders[i].split(",")
        drawPoint(coordinates[0], coordinates[1], 'black', 10)
    }

    for (let i = 0; i < sheeps.length; i++) {
        let coordinates = sheeps[i].split(",")
        drawCircle(coordinates[0], coordinates[1], 5, "black", 2)
    }

    for (let i = 0; i < centroidCollection.length; i++) {
        let coordinates = centroidCollection[i].split(",").map(s => parseFloat(s))
        triangle(coordinates[0], coordinates[1], 'lightgrey')
    }

    for (let i = 0; i < commandsCoordinates.length; i++) {
        let coordinates = commandsCoordinates[i].split(",")
        drawPoint(coordinates[0], coordinates[1], 'black', 5)
    }
});

function reset() {
    var nr = "5"; // document.getElementById("nrOfSheeps").value;
    var s1 = document.getElementById("setting1").value;
    var s2 = "1"; // document.getElementById("setting2").value;
    var s3 = document.getElementById("setting3").value;
    connection.invoke("Reset", nr, s1, s2, s3).catch(function (err) {
        return console.error(err.toString());
    });
}

function startStop() {
    console.log("StartStop")
    connection.invoke("StartStop").catch(function (err) {
        return console.error(err.toString());
    });
}

function startStopDrones() {
    console.log("StartStopDrones")
    connection.invoke("StartStopDrones").catch(function (err) {
        return console.error(err.toString());
    });
}

function toggleInterceptCross() {
    console.log("ToggleInterceptCross")
    connection.invoke("ToggleInterceptCross").catch(function (err) {
        return console.error(err.toString());
    });
}

connection.start().then(function () {
    console.log("Connection started")
}).catch(function (err) {
    return console.error(err.toString());
});

function toggleClearCanvas() {
    clearCanvasOn = !clearCanvasOn;
}

function toggleSendMouse() {
    sendMouseOn = !sendMouseOn;
}

function clearCanvas() {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
}

function drawPoint(x, y, color, size = 10) {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    // ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = color;
    ctx.fillRect(x, y, size, size)
}

function drawVector(x1, y1, x2, y2, color, lineWidth) {
    // https://www.javascripttutorial.net/web-apis/javascript-draw-line/
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    // ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = color;
    ctx.lineWidth = lineWidth;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();
}

function drawCircle(x, y, radius, color, lineWidth) {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    // ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = color;
    ctx.fillStyle = color;
    ctx.lineWidth = lineWidth;
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, 2 * Math.PI);
    ctx.stroke();
}

function triangle(x, y, color) {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.moveTo(x, y);
    ctx.lineTo(x - 10, y + 10);
    ctx.lineTo(x + 10, y + 10);
    ctx.fill();
}

function printElapsedTime(time) {
    const text = document.querySelector('#elapsedTime');
    text.innerHTML = time;
}

function printState(state) {
    const text = document.querySelector('#state');
    text.innerHTML = state;
}

function sendMousePosition(x, y) {
    lastMouseX = x;
    lastMouseY = y;
    connection.invoke("MousePosition", x + "," + y).catch(function (err) {
        return console.error(err.toString());
    });
}

function mousemove(event) {
    if (!sendMouseOn) return;
    mouseX = event.clientX;
    mouseY = event.clientY;
    if (mouseX > (lastMouseX + 10) || mouseX < (lastMouseX - 10) || mouseY > (lastMouseY + 10) || mouseY > (lastMouseY + 10)) {
        sendMousePosition(mouseX, mouseY)
    }
}

window.addEventListener('mousemove', mousemove);