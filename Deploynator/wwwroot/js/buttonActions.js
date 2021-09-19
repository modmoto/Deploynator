window.attachHandlers = () => {
    function onMouseDown(event) {
        var targetElement = event.target || event.srcElement;
        console.log(targetElement);
        targetElement.classList.remove("mouseup");
        targetElement.classList.add("mousedown");
    }

    function onMouseUp(event) {
        var targetElement = event.target || event.srcElement;
        console.log(targetElement);
        targetElement.classList.remove("mousedown");
        targetElement.classList.add("mouseup");
    }

    var deployButton = document.getElementById("deploy-button");
    deployButton.addEventListener("mousedown", onMouseDown);
    deployButton.addEventListener("mouseup", onMouseUp);

    var controlButtons = document.querySelectorAll(".control-button.active");
    for (var i = 0; i < controlButtons.length; i++) {
        controlButtons[i].addEventListener('mousedown', onMouseDown);
        controlButtons[i].addEventListener('mouseup', onMouseUp);
    }
};