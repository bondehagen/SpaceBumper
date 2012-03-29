var net = require('net');

var client = net.connect(1986, function () {
    console.log('connected');
    client.write("NAME NodeAi\n", 'ascii');
});

client.on('end', function () {
    console.log('disconnected');
});

function Vector(x, y) {
    this.x = x;
    this.y = y;

    this.Length = function Length() {
        return Math.sqrt((this.x * this.x) + (this.y * this.y));
    }

    this.Normalize = function Normalize() {
        var length = this.Length();
        this.x = this.x / length;
        this.y = this.y / length;
        return this;
    }
}

function Bumpership(position, velocity, score) {
    this.position = position;
    this.velocity = velocity;
    this.score = score;
}

var endMap = false;
var endState = false;

var map = null;
var bumperships = [];
var stars = [];
var meIndex = 0;
var iteration = 0;

function runAI() {
    // your AI goes here
    var acceleration = new Vector(
        Math.random() * 6-3,
        Math.random() * 6-3
    );
    //console.log(acceleration);
    client.write("ACCELERATION " + acceleration.x + " " + acceleration.y + "\n", 'ascii', function(){
        setTimeout(function(){
            client.write("GET_STATE\n", 'ascii');
        }, 500);
    });
}

client.on('data', function (data) {
    var lines = data.toString().split('\n');
    for (var i = 0; i < lines.length; i++) {
        var line = lines[i].replace('\r', '');
        var values = line.split(' ');

        switch (values[0]) {
            case "START":
                client.write("GET_STATE\n", 'ascii');
                break;
            case "BEGIN_STATE":
                iteration = values[1];
                endState = false;
                bumperships = [];
                stars = [];
                meIndex = 0;
                break;
            case "END_STATE":
                endState = true;
                runAI();
                break;
            case "BEGIN_MAP":
                map = [];
                break;
            case "END_MAP":
                endMap = true;
                var grid = "";
                for (var r = 0; r < map.length; r++) {
                    for (var c = 0; c < map[0].length; c++) {
                        grid += map[r][c];
                    }
                    grid += "\n";
                }
                console.log(grid);
                break;
            default:
                if (!endMap) {
                    map.push(line)
                } else if (!endState) {
                    switch (values[0]) {
                        case "BUMPERSHIP":
                            bumperships.push(new Bumpership(
                                new Vector(parseFloat(values[1]), parseFloat(values[2])),
                                new Vector(parseFloat(values[3]), parseFloat(values[4])),
                                parseInt(values[5])));
                            break;
                        case "YOU":
                            meIndex = parseInt(values[1]);
                            break;
                        case "STAR":
                            stars.push(new Vector(parseFloat(values[1]), parseFloat(values[2])));
                            break;
                    }
                }
        }
    }
});
