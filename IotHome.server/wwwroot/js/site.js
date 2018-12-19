// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

"use strict";

$( document ).ready(function () {

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/app")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    $("#motor_toggle").hide();
    $("#booster_toggle").hide();
    $("#submit_toggle").hide();

    var toggleMotor = $("#toggleMotor");
    var toggleBooster = $("#toggleBooster");

    var upperTankHeight = 0;
    var lowerTankHeight = 0;
    var motorState = false;
    var boosterState = false;

    connection.on("status", function (_motorState, _boosterState) {
        motorState = _motorState;
        boosterState = _boosterState;

        $(toggleBooster).prop('checked', _boosterState).change();
        $(toggleMotor).prop('checked', _motorState).change();
    });

    connection.on("levels", function (upperLevel, lowerLevel) {
        upperTankHeight = upperLevel*100;
        lowerTankHeight = lowerLevel*100;

        if(upperLevel >= 1)
            upperTankHeight = 100;
        
        if(lowerLevel >= 1)
            lowerTankHeight = 100;

        if(upperLevel <= 0)
            upperTankHeight = 0;
        
        if(lowerLevel <= 0)
            lowerTankHeight = 0;

        document.getElementById("upper_tank").style.height = upperTankHeight.toString() + "%";
        document.getElementById("lower_tank").style.height = lowerTankHeight.toString() + "%";

        console.log('Getting Levels: ' + upperTankHeight + " : " + lowerTankHeight);
    });

    $('#Load').click(function () {
        $("#motor_toggle").show();
        $("#booster_toggle").show();
        $("#submit_toggle").show();

        connection.invoke("GetStates").catch(function (err) {
            return console.error(err.toString());
        });

        getLevel();
        
        $("#Load").hide();
    });

    async function getLevel() {
        try {
            await connection.invoke("GetTankLevels");
            setTimeout(() => getLevel(), 1000);
        } catch (err) {
            console.log(err);
            setTimeout(() => getLevel(), 1000);
        }
    };

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

    $(toggleMotor).change(function () {
        $('#console-event-motor').html('Motor Status: ' + $(this).prop('checked'));
    })

    $(toggleBooster).change(function () {
        $('#console-event-booster').html('Booster Status: ' + $(this).prop('checked'));
    })

    $('#Submit').click(function () {
        motorState = $(toggleMotor).prop('checked');
        boosterState = $(toggleBooster).prop('checked');

        connection.invoke("SetStates", motorState, boosterState).catch(function (err) {
            return console.error(err.toString());
        });
    });

    connection.invoke("GetTankLevels").catch(function (err) {
        return console.error(err.toString());
    });

    async function start() {
        try {
            await connection.start();
            console.log('connected');
        } catch (err) {
            console.log(err);
            setTimeout(() => start(), 5000);
        }
    };

    connection.onclose(async () => {
        await start();
    });
});