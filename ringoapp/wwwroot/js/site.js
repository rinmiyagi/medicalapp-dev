// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.showConfirmModal = function (options) {
    document.getElementById("globalConfirmModalLabel").innerText = options.title || "Confirm Action";
    document.getElementById("globalConfirmModalMessage").innerText = options.message || "Are you sure?";
    
    var form = document.getElementById("globalConfirmModalForm");
    form.action = options.action || "";
    
    // Clear previous inputs
    var inputsContainer = document.getElementById("globalConfirmModalInputs");
    inputsContainer.innerHTML = "";
    
    // Add custom inputs
    if (options.inputs) {
        for (var key in options.inputs) {
            if (options.inputs.hasOwnProperty(key)) {
                var input = document.createElement("input");
                input.type = "hidden";
                input.name = key;
                input.value = options.inputs[key];
                inputsContainer.appendChild(input);
            }
        }
    }
    
    var submitBtn = document.getElementById("globalConfirmModalSubmitBtn");
    submitBtn.className = "btn px-4 fw-bold " + (options.buttonClass || "btn-primary");
    submitBtn.innerText = options.buttonText || "Confirm";
    
    var myModal = new bootstrap.Modal(document.getElementById('globalConfirmModal'));
    myModal.show();
};
