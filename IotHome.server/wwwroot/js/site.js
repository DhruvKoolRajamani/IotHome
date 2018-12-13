// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

"use strict";

$(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/app")
        .build();

    var toggleMotor = $("#toggleMotor");
    var toggleBooster = $("#toggleBooster");

    var upperTankHeight = 50;
    var lowerTankHeight = 30;
    var motorState = false;
    var boosterState = false;

    connection.on("status", function (_motorState, _boosterState) {
        $(toggleMotor).prop('checked', _motorState).change();
        motorState = _motorState;
        $(toggleBooster).prop('checked', _boosterState).change();
        boosterState = _boosterState;
        alert("Motor/Booster Status Changed");
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

    // if (connection.start()) {
    //     connection.invoke("Init").catch(function (err) {
    //         return console.error(err.toString());
    //     });
    // }

    $(toggleMotor).change(function () {
        $('#console-event-motor').html('Motor Status: ' + $(this).prop('checked'));
    })

    $(toggleBooster).change(function () {
        $('#console-event-booster').html('Booster Status: ' + $(this).prop('checked'));
    })

    $('#Submit').click(function () {
        motorState = $(toggleMotor).prop('checked');
        boosterState = $(toggleBooster).prop('checked');
        // 1-ff, 2-ft, 3-tf, 4-tt
        if ((motorState == false) && (boosterState == false)) {
            connection.invoke("controller", 1).catch(function (err) {
                return console.error(err.toString());
            });
        }
        else if ((motorState == false) && (boosterState == true)) {
            connection.invoke("controller", 2).catch(function (err) {
                return console.error(err.toString());
            });
        }
        else if ((motorState == true) && (boosterState == false)) {
            connection.invoke("controller", 3).catch(function (err) {
                return console.error(err.toString());
            });
        }
        else if ((motorState == true) && (boosterState == true)) {
            connection.invoke("controller", 4).catch(function (err) {
                return console.error(err.toString());
            });
        }
        else {
            alert("Error");
        }
    });

    document.getElementById("upper_tank").style.height = upperTankHeight.toString() + "%";
    document.getElementById("lower_tank").style.height = lowerTankHeight.toString() + "%";
});