// Function to toggle visibility of modal
function toggleModal(modalId) {
    var modal = document.getElementById(modalId);
    modal.style.display = (modal.style.display === "block") ? "none" : "block";
}

// Function to close modal
function closeModal(modalId) {
    var modal = document.getElementById(modalId);
    modal.style.display = "none";
}

// Get the login button, modal, and close button
var loginButton = document.getElementById("login-toggle");
var loginModal = document.getElementById("loginModal");
var signUpLink = document.getElementById("sign-up");
var closeButtonLogin = document.querySelector("#loginModal .close");
var closeButtonSignUp = document.querySelector("#signupModal .close");
var alreadyHaveAccountLink = document.getElementById("already-have-account");

// Event listener for login button click
if (loginButton) {
    loginButton.addEventListener("click", function () {
        toggleModal("loginModal");
    });
}

// Event listener for close button click in login modal
closeButtonLogin.addEventListener("click", function () {
    closeModal("loginModal");
});

// Event listener for outside click in login modal
window.addEventListener("click", function (event) {
    if (event.target === loginModal) {
        closeModal("loginModal");
    }
});

// Event listener for sign up link click
signUpLink.addEventListener("click", function (event) {
    event.preventDefault(); // Prevent default link behavior
    closeModal("loginModal");
    toggleModal("signupModal");
});

// Event listener for close button click in sign-up modal
closeButtonSignUp.addEventListener("click", function () {
    closeModal("signupModal");
});

// Event listener for outside click in sign-up modal
window.addEventListener("click", function (event) {
    if (event.target === signupModal) {
        closeModal("signupModal");
    }
});

// Event listener for already have an account link click
alreadyHaveAccountLink.addEventListener("click", function (event) {
    event.preventDefault(); // Prevent default link behavior
    closeModal("signupModal");
    toggleModal("loginModal");
});