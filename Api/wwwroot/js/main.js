"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/communication").build();
var colors = ['green', 'red', 'red', 'red', 'indigo', 'yellow'];
var mouseX = 0;
var mouseY = 0;

var lastMouseX = 0;
var lastMouseY = 0;
var clearCanvasOn = true;

connection.on("ReceiveMessage", function (user, message) {
    console.log(message)
    var objects = message.split("!")
    var elapsedTime = objects[0].split(",");
    var centroidCollection = objects[1].split(";");
    var coordinatesCollection = objects[2].split(";");
    var vectorCollection = objects[3].split(";");
    var circle = objects[4].split(";");
    var pathCoordinates = objects[5].split(";");
    var pathCoordinatesAchieved = objects[6].split(";");
    var commandsCoordinates = objects[7].split(";");
    var state = objects[8];
    var terrainPath = objects[9].split(";");

    printElapsedTime(elapsedTime);
    printState(state);
    if (clearCanvasOn) {
        clearCanvas();
    }

    if (clearCanvasOn) {
        for (let i = 0; i < terrainPath.length - 2; i++) {
            let from = terrainPath[i].split(",")
            let to = terrainPath[i + 1].split(",")
            drawVector(from[0], from[1], to[0], to[1], 'red', 3)
        }
        
        for (let i = 0; i < pathCoordinates.length - 2; i++) {
            let from = pathCoordinates[i].split(",")
            let to = pathCoordinates[i + 1].split(",")
            drawVector(from[0], from[1], to[0], to[1], 'yellow', 5)
        }

        for (let i = 0; i < pathCoordinatesAchieved.length - 2; i++) {
            let from = pathCoordinatesAchieved[i].split(",")
            let to = pathCoordinatesAchieved[i + 1].split(",")
            drawVector(from[0], from[1], to[0], to[1], 'green', 4)
        }

        drawCircle(circle[0], circle[1], circle[2], "pink")

        for (let i = 0; i < vectorCollection.length - 1; i++) {
            let vector = vectorCollection[i].split(",")
            drawVector(vector[0], vector[1], vector[2], vector[3], 'red', 1)
        }
    }
    for (let i = 0; i < coordinatesCollection.length - 1; i++) {
        let coordinates = coordinatesCollection[i].split(",")
        drawPoint(coordinates[0], coordinates[1], colors[i])
    }

    for (let i = 0; i < centroidCollection.length - 1; i++) {
        let coordinates = centroidCollection[i].split(",")
        drawPoint(coordinates[0], coordinates[1], 'black')
    }

    for (let i = 0; i < commandsCoordinates.length - 1; i++) {
        let coordinates = commandsCoordinates[i].split(",")
        drawPoint(coordinates[0], coordinates[1], 'black', 5)
    }

    // let coordinates = commandsCoordinates[0].split(",")
    // drawPoint(coordinates[0], coordinates[1], 'pink')
    // coordinates = commandsCoordinates[1].split(",")
    // drawPoint(coordinates[0], coordinates[1], 'purple')
});

connection.on("Scoreboard", function (list) {
    console.log(list)
    let htmlList = document.getElementById("scoreboard");
    htmlList.innerHTML = ""
    for (let i = 0; i < list.length; i++) {
        var li = document.createElement("li");
        li.appendChild(document.createTextNode(list[i].name + " sheeps: " + list[i].nrOfSheeps + " time: " + list[i].time + " adjusted time: " + list[i].points));
        htmlList.appendChild(li);
    }
});

function reset() {
    var nr = document.getElementById("nrOfSheeps").value;
    var s1 = document.getElementById("setting1").value;
    var s2 = document.getElementById("setting2").value;
    var s3 = document.getElementById("setting3").value;
    console.log("Reset, number of sheeps: ", nr)
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

function saveName() {
    var name = document.getElementById("player").value;
    console.log("Player: ", name)
    connection.invoke("SetName", name).catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("SendName", function (name) {
    console.log("Player playing: ", name)
    document.getElementById("player").value = name;
});

connection.start().then(function () {
    console.log("Connection started")
}).catch(function (err) {
    return console.error(err.toString());
});

function toggleClearCanvas() {
    if (clearCanvasOn) {
        clearCanvasOn = false;
    } else {
        clearCanvasOn = true;
    }
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

function drawCircle(x, y, radius, color) {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    // ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = color;
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, 2 * Math.PI);
    ctx.stroke();
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
    mouseX = event.clientX;
    mouseY = event.clientY;
    if (mouseX > (lastMouseX + 10) || mouseX < (lastMouseX - 10) || mouseY > (lastMouseY + 10) || mouseY > (lastMouseY + 10)) {
        sendMousePosition(mouseX, mouseY)
    }
}

window.addEventListener('mousemove', mousemove);