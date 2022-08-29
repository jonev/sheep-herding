"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
var mouseX = 0;
var mouseY = 0;

var lastMouseX = 0;
var lastMouseY = 0;

var colors = ['green', 'red', 'red', 'red', 'yellow'];


connection.on("ReceiveMessage", function (user, message) {
    // console.log(message)
    var objects = message.split("!")
    var centroid = objects[0].split(",");
    var coordinatesCollection = objects[1].split(";");
    var vectorCollection = objects[2].split(";");
    // console.log(coordinatesCollection, vectorCollection)
    clearCanvas();
    draw(centroid[0], centroid[1], 'pink')
    
    for (let i = 0; i < coordinatesCollection.length - 1; i++) {
        let coordinates = coordinatesCollection[i].split(",")
        // console.log("X: " + coordinates[0] + " Y: " + coordinates[1])
        draw(coordinates[0], coordinates[1], colors[i])
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
    var herderThreshold = document.getElementById("herderThreshold").value;
    var oversightThreshold = document.getElementById("oversightThreshold").value;
    var oversightSpeed = document.getElementById("oversightSpeed").value;
    console.log("Reset, number of sheeps: ", nr, herderThreshold, oversightThreshold, oversightThreshold)
    connection.invoke("Reset", nr, herderThreshold, oversightThreshold, oversightSpeed).catch(function (err) {
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

connection.start().then(function () {
    console.log("Connection started")
}).catch(function (err) {
    return console.error(err.toString());
});

function sendMousePosition(x,y){
    lastMouseX = x;
    lastMouseY = y;
    connection.invoke("MousePosition", x+","+y).catch(function (err) {
        return console.error(err.toString());
    });
}

function clearCanvas() {
    const canvas = document.querySelector('#canvas');

    if (!canvas.getContext) {
        return;
    }
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
}

function draw(x, y, color) {
    // https://www.javascripttutorial.net/web-apis/javascript-draw-line/
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

// function mousemove(event){
//     mouseX = event.clientX;
//     mouseY = event.clientY;
//     if(mouseX > (lastMouseX + 10) || mouseX < (lastMouseX - 10) || mouseY > (lastMouseY + 10) || mouseY > (lastMouseY + 10)){
//         sendMousePosition(mouseX, mouseY)
//     }
// }
//
// window.addEventListener('mousemove', mousemove);