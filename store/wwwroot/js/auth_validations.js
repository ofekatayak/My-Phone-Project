// Sign Up validation
async function validateForm(event) {
    event.preventDefault(); // Prevent default form submission behavior
    var signUpElement = document.querySelector(".signup-form");
    const nameInput = document.querySelector('#name-signup');
    const emailInput = document.querySelector('#email-signup');
    const passwordInput = document.querySelector('#password-signup');
    const nameError = document.getElementById('name-error-signup');
    const emailError = document.getElementById('email-error-signup');
    const passwordError = document.getElementById('password-error-signup');

    // Clear previous error messages
    nameError.textContent = '';
    emailError.textContent = '';
    passwordError.textContent = '';
    let flag = true;
    let emailIsOk = true;

    // Check Name
    if (nameInput.value.trim() === '') {
        nameError.textContent = 'Name is required.';
        flag = false;
    }

    // Check Email
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/; // Corrected email pattern
    if (!emailPattern.test(emailInput.value.trim())) {
        emailError.textContent = 'Invalid email address.';
        flag = false;
    } else {
        // Check if email exists in database
        const emailExists = await checkEmailExists(emailInput.value.trim()); // Await here
        console.log(emailExists);
        if (emailExists) {
            emailError.textContent = 'Email already exists.';
            flag = false;
        }
    }

    // Check Password
    if (passwordInput.value.trim().length < 6) {
        passwordError.textContent = 'Password must be at least 6 characters long.';
        flag = false;
    }

    // If all validations pass, allow form submission
    if (flag) {
        signUpElement.submit();
    }

    // If all validations pass, allow form submission
    console.log("flag: " + flag + ", Vallidatios: " + flag);
    return flag;
}
async function checkEmailExists(email) {
    const response = await fetch(`/Home/CheckEmailExists?email=${email}`);
    const data = await response.json();
    console.log(data); // Log response data for debugging
    return data.exists; // Return true or false directly
}


// Login Verification
async function LoginForm(event) {
    event.preventDefault(); // Prevent default form submission behavior
    var LoginElement = document.querySelector(".login-form");
    const emailInput = document.querySelector('#email-login');
    const passwordInput = document.querySelector('#password-login');
    const emailError = document.getElementById('email-error-login');
    const passwordError = document.getElementById('password-error-login');
    let flag = true;
    let userExists = false;

    // Clear previous error messages
    emailError.textContent = '';
    passwordError.textContent = '';

    // Check inputs
    const emailPattern = /^[a-zA-Z0-9._%+-]+@@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/; // Corrected email pattern
    if (!emailPattern.test(emailInput.value.trim())) {
        emailError.textContent = 'Invalid email address.';
        flag = false;
    }
    // Check if password is empty
    if (!passwordInput.value.trim()) {
        passwordError.textContent = 'Password is required.';
        flag = false;
    }
    if (flag) {
        userExists = await ValidateUser(emailInput.value.trim(), passwordInput.value.trim());
        // If the above line completes without throwing an error, it means the user exists
    }

    // Log userExists after validation
    console.log('userExists:', userExists);

    // Log flag after all validations
    console.log("flag: ", flag);
}