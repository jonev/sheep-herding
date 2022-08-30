"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/communication").build();
var colors = ['green', 'red', 'red', 'red', 'yellow'];


connection.on("ReceiveMessage", function (user, message) {
    // console.log(message)
    var objects = message.split("!")
    var centroid = objects[0].split(",");
    var coordinatesCollection = objects[1].split(";");
    var vectorCollection = objects[2].split(";");
    var circle = objects[3].split(";");
    // console.log(coordinatesCollection, vectorCollection)
    clearCanvas();
    drawPoint(centroid[0], centroid[1], 'pink')
    drawCircle(circle[0], circle[1], circle[2], "pink")
    for (let i = 0; i < coordinatesCollection.length - 1; i++) {
        let coordinates = coordinatesCollection[i].split(",")
        // console.log("X: " + coordinates[0] + " Y: " + coordinates[1])
        drawPoint(coordinates[0], coordinates[1], colors[i])
    }

    for (let i = 0; i < vectorCollection.length - 1; i++) {
        let vector = vectorCollection[i].split(",")
        // console.log("Vector X1: " + vector[0] + " Y1: " + vector[1] + " X2: " + vector[2] + " Y2: " + vector[3])
        drawVector(vector[0], vector[1], vector[2], vector[3], 'red')
    }
});

connection.on("Scoreboard", function (list) {
    console.log(list)
    let htmlList = document.getElementById("scoreboard");
    htmlList.innerHTML = ""
    for (let i = 0; i < list.length; i++) {
        var li = document.createElement("li");
        li.appendChild(document.createTextNode(list[i].name + " sheeps: " + list[i].nrOfSheeps + " time: " + list[i].time));
        htmlList.appendChild(li);
    }
});

function reset(){
    var nr = document.getElementById("nrOfSheeps").value;
    var s1 = document.getElementById("setting1").value;
    var s2 = document.getElementById("setting2").value;
    var s3 = document.getElementById("setting3").value;
    console.log("Reset, number of sheeps: ", nr)
    connection.invoke("Reset", nr, s1, s2, s3).catch(function (err) {
        return console.error(err.toString());
    });
}

function startStop(){
    console.log("StartStop")
    connection.invoke("StartStop").catch(function (err) {
        return console.error(err.toString());
    });
}

function saveName(){
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

function clearCanvas() {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
}

function drawPoint(x, y, color) {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    // ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = color;
    ctx.fillRect(x, y, 10, 10)
}

function drawVector(x1, y1, x2, y2, color) {
    // https://www.javascripttutorial.net/web-apis/javascript-draw-line/
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    // ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = color;
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