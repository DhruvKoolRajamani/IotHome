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

    var iSubmit = 0;
    var onSubmitted = false;

    var toggleMotor = $("#toggleMotor");
    var toggleBooster = $("#toggleBooster");

    var upperTankHeight = 50;
    var lowerTankHeight = 30;
    var motorState = false;
    var boosterState = false;

    connection.on("status", function (_motorState, _boosterState) {
        motorState = _motorState;
        boosterState = _boosterState;

        $(toggleBooster).prop('checked', _boosterState).change();
        $(toggleMotor).prop('checked', _motorState).change();
        
        if(iSubmit == 1){
            alert("Motor/Booster Status Changed");
        }
    });

    $('#Load').click(function () {
        $("#motor_toggle").show();
        $("#booster_toggle").show();
        $("#submit_toggle").show();

        connection.invoke("Init").catch(function (err) {
            return console.error(err.toString());
        });

        $("#Load").hide();
        iSubmit = 0;
    });

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
        onSubmitted = true;
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
        iSubmit = 1;
    });

    document.getElementById("upper_tank").style.height = upperTankHeight.toString() + "%";
    document.getElementById("lower_tank").style.height = lowerTankHeight.toString() + "%";
});