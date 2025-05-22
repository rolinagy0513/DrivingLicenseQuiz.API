$(document).ready(function() {
    // Check if user is already logged in
    checkSession();

    // Login form submission
    $('#loginForm').on('submit', function(e) {
        e.preventDefault();
        const email = $('#loginEmail').val();
        const password = $('#loginPassword').val();

        login(email, password);
    });

    // Register form submission
    $('#registerForm').on('submit', function(e) {
        e.preventDefault();
        const name = $('#registerName').val();
        const email = $('#registerEmail').val();
        const password = $('#registerPassword').val();
        const confirmPassword = $('#confirmPassword').val();

        if (password !== confirmPassword) {
            showRegisterError('Passwords do not match');
            return;
        }

        if (password.length < 8) {
            showRegisterError('Password must be at least 8 characters long');
            return;
        }

        register(name, email, password);
    });
});

function checkSession() {
    $.ajax({
        url: '/api/session/validate',
        method: 'GET',
        xhrFields: {
            withCredentials: true
        },
        success: function(response) {
            // User is logged in
            window.location.href = 'index.html';
        },
        error: function() {
            // User is not logged in, stay on the page
        }
    });
}

function login(email, password) {
    $.ajax({
        url: '/api/session/login',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, password }),
        xhrFields: {
            withCredentials: true
        },
        success: function(response) {
            window.location.href = 'index.html';
        },
        error: function(xhr) {
            showLoginError(xhr.responseText || 'Invalid email or password');
        }
    });
}

function register(name, email, password) {
    $.ajax({
        url: '/api/session/register',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ name, email, password }),
        xhrFields: {
            withCredentials: true
        },
        success: function(response) {
            window.location.href = 'index.html';
        },
        error: function(xhr) {
            showRegisterError(xhr.responseText || 'Registration failed');
        }
    });
}

function showLoginError(message) {
    const errorDiv = $('#loginError');
    errorDiv.text(message);
    errorDiv.removeClass('d-none');
}

function showRegisterError(message) {
    const errorDiv = $('#registerError');
    errorDiv.text(message);
    errorDiv.removeClass('d-none');
}

// Clear error messages when switching tabs
$('a[data-bs-toggle="tab"]').on('shown.bs.tab', function() {
    $('#loginError, #registerError').addClass('d-none');
}); 