function validateForm() {
    var email = document.getElementById("email").value;
    var password = document.getElementById("password").value;
    var loginButton = document.getElementById("loginButton");

    if (email.trim() !== "" && password.trim() !== "") {
        loginButton.disabled = false;
    } else {
        loginButton.disabled = true;
    }
}
$(document).ready(function () {
    $('.btn-info').click(function () {
        console.log("Botón 'Necesito ayuda' clickeado");
        alert("Se le ha notificado al administrador sobre la necesidad de ayuda con su cuenta. Un administrador se pondrá en contacto con usted.");
    });

});

