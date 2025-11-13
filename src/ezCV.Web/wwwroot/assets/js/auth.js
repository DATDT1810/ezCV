
// Biến toàn cục để lưu email
let currentResetEmail = '';

// Hàm kiểm tra login bằng API backend
async function isUserLoggedIn() {
    try {
        const res = await fetch('/Auth/check-login', { credentials: 'include' });
        if (!res.ok) return false;
        const data = await res.json();
        return data.isLoggedIn === true;
    } catch (err) {
        console.error("Lỗi khi kiểm tra login:", err);
        return false;
    }
}

// Hàm hiển thị lỗi cho field cụ thể
function showFieldError(fieldId, message) {
    const errorElement = document.getElementById(fieldId + '-error');
    const inputElement = document.getElementById(fieldId);

    if (errorElement && inputElement) {
        errorElement.textContent = message;
        errorElement.style.display = 'block';
        inputElement.classList.add('error-field');
    }
}

// Hàm hiển thị lỗi chung
function showGeneralError(elementId, message) {
    const errorElement = document.getElementById(elementId);
    if (errorElement) {
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    }
}

// Hàm reset tất cả lỗi
function resetErrors(formType) {
    // Reset field errors
    const errorElements = document.querySelectorAll('.error-message');
    errorElements.forEach(element => {
        element.style.display = 'none';
        element.textContent = '';
    });

    // Reset general errors
    const generalErrors = ['login-error-message', 'register-error-message'];
    generalErrors.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.style.display = 'none';
            element.textContent = '';
        }
    });

    // Remove error class từ inputs
    const errorInputs = document.querySelectorAll('.error-field');
    errorInputs.forEach(input => {
        input.classList.remove('error-field');
    });
}

// Hàm xử lý lỗi đăng nhập từ server
function handleLoginError(errorMessage) {
    resetErrors('login');

    // Phân tích lỗi và hiển thị ở field phù hợp
    if (errorMessage.includes("Tài khoản không tồn tại") ||
        errorMessage.includes("Email") ||
        errorMessage.toLowerCase().includes("account")) {
        showFieldError('login-email', errorMessage);
    } else if (errorMessage.includes("Mật khẩu") ||
        errorMessage.includes("Password") ||
        errorMessage.includes("Sai mật khẩu")) {
        showFieldError('login-password', errorMessage);
    } else {
        // Hiển thị lỗi chung nếu không xác định được
        showGeneralError('login-error-message', errorMessage);
    }
}

// Hàm xử lý lỗi đăng ký từ server
function handleRegisterError(errorMessage) {
    resetErrors('register');

    if (errorMessage.includes("Email") || errorMessage.includes("tồn tại")) {
        showFieldError('signup-email', errorMessage);
    } else if (errorMessage.includes("Mật khẩu") || errorMessage.includes("password")) {
        showFieldError('signup-password', errorMessage);
    } else if (errorMessage.includes("khớp") || errorMessage.includes("confirm")) {
        showFieldError('signup-confirm-password', errorMessage);
    } else {
        showGeneralError('register-error-message', errorMessage);
    }
}

// Hàm validate email
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Hàm đóng popup thành công
function closeSuccessPopup() {
    if (window.$ && $.magnificPopup) {
        $.magnificPopup.close();
        // Mở popup đăng nhập
        $.magnificPopup.open({
            items: { src: '#test-popup' },
            type: 'inline',
            mainClass: 'mfp-fade'
        });
    }
}

// Hàm hiển thị popup thành công
function showSuccessPopup(message) {
    const successMessage = document.getElementById('success-message');
    if (successMessage) {
        successMessage.textContent = message || 'Mật khẩu đã được đặt lại thành công!';
    }

    if (window.$ && $.magnificPopup) {
        $.magnificPopup.close();
        $.magnificPopup.open({
            items: { src: '#success-popup' },
            type: 'inline',
            mainClass: 'mfp-fade'
        });
    }
}

// Hàm reset errors cho OTP và new password
function resetOtpErrors() {
    const otpMessage = document.getElementById('otp-message');
    if (otpMessage) {
        otpMessage.style.display = 'none';
        otpMessage.textContent = '';
        otpMessage.className = 'col-12 mb-3';
    }
}

function resetNewPasswordErrors() {
    const newPasswordMessage = document.getElementById('new-password-message');
    if (newPasswordMessage) {
        newPasswordMessage.style.display = 'none';
        newPasswordMessage.textContent = '';
        newPasswordMessage.className = 'col-12 mb-3';
    }

    // Reset field errors
    const errorFields = ['new-password-error', 'confirm-new-password-error'];
    errorFields.forEach(fieldId => {
        const element = document.getElementById(fieldId);
        if (element) {
            element.style.display = 'none';
            element.textContent = '';
        }
    });

    // Remove error class từ inputs
    const inputs = ['new-password', 'confirm-new-password'];
    inputs.forEach(inputId => {
        const input = document.getElementById(inputId);
        if (input) {
            input.classList.remove('error-field');
        }
    });
}

// Helper functions cho OTP và new password
function showOtpMessage(message, type) {
    const messageDiv = document.getElementById("otp-message");
    if (messageDiv) {
        messageDiv.textContent = message;
        messageDiv.className = `col-12 mb-3 alert alert-${type}`;
        messageDiv.style.display = 'block';
    }
}

function showNewPasswordMessage(message, type) {
    const messageDiv = document.getElementById("new-password-message");
    if (messageDiv) {
        messageDiv.textContent = message;
        messageDiv.className = `col-12 mb-3 alert alert-${type}`;
        messageDiv.style.display = 'block';
    }
}

// Hàm hiển thị thông báo (cho quên mật khẩu)
function showMessage(messageDiv, message, type) {
    messageDiv.textContent = message;
    messageDiv.className = `col-12 mb-3 alert alert-${type}`;
    messageDiv.style.display = 'block';

    // Tự động ẩn thông báo sau 5 giây
    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 5000);
}

// Helper lấy anti-forgery token
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
}

// Khi DOM đã load xong
document.addEventListener("DOMContentLoaded", async () => {
    const loggedIn = await isUserLoggedIn();
    console.log('DOM loaded - Kiểm tra login status:', loggedIn);

    // Gán sự kiện cho các nút "Sử dụng mẫu"
    document.querySelectorAll('a.preview-demo.v2').forEach(link => {
        link.addEventListener('click', async (e) => {
            e.preventDefault();
            e.stopPropagation();

            const loggedIn = await isUserLoggedIn();
            if (loggedIn) {
                console.log('User đã đăng nhập → cho phép dùng mẫu');
                window.location.href = link.href;
                return;
            }

            console.log('User chưa đăng nhập → mở popup');
            if (window.$ && $.magnificPopup) {
                $.magnificPopup.open({
                    items: { src: '#test-popup' },
                    type: 'inline',
                    mainClass: 'mfp-fade'
                });
            }
        });
    });

    // Xử lý chuyển đổi giữa tất cả popup
    if (window.$) {
        $('a.open-popup-link').magnificPopup({
            type: 'inline',
            midClick: true,
            mainClass: 'mfp-fade',
            removalDelay: 300
        });
    }

    // Xử lý đăng nhập
    const loginForm = document.getElementById("main_login_form");
    if (loginForm) {
        loginForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            resetErrors('login');

            const email = document.getElementById("login-email").value.trim();
            const password = document.getElementById("login-password").value.trim();

            // Validate client-side
            let hasError = false;

            if (!email) {
                showFieldError('login-email', 'Vui lòng nhập email');
                hasError = true;
            } else if (!isValidEmail(email)) {
                showFieldError('login-email', 'Email không hợp lệ');
                hasError = true;
            }

            if (!password) {
                showFieldError('login-password', 'Vui lòng nhập mật khẩu');
                hasError = true;
            }

            if (hasError) return;

            try {
                console.log("Sending login request...");

                const response = await fetch("/Auth/ajax-login", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": getAntiForgeryToken(),
                        "Accept": "application/json"
                    },
                    body: JSON.stringify({
                        email: email,
                        password: password
                    })
                });

                console.log("Response status:", response.status);

                if (!response.ok) {
                    const errorText = await response.text();
                    console.error("Error response:", errorText);

                    // Parse lỗi từ response
                    let errorMessage = "Đăng nhập thất bại";
                    try {
                        const errorResult = JSON.parse(errorText);
                        errorMessage = errorResult.message || errorMessage;
                    } catch (e) {
                        errorMessage = errorText;
                    }

                    handleLoginError(errorMessage);
                    return;
                }

                const result = await response.json();
                console.log("Success result:", result);

                if (result.success) {
                    // Đóng popup
                    if (window.$ && $.magnificPopup) {
                        $.magnificPopup.close();
                    }

                    // Chuyển hướng
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else {
                        window.location.reload();
                    }
                } else {
                    handleLoginError(result.message || "Đăng nhập thất bại");
                }
            } catch (err) {
                console.error("Login error:", err);
                handleLoginError("Lỗi kết nối đến server: " + err.message);
            }
        });
    }

    // Xử lý đăng ký
    const registerForm = document.getElementById("main_Signup_form");
    if (registerForm) {
        registerForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            resetErrors('register');

            const email = document.getElementById("signup-email").value.trim();
            const password = document.getElementById("signup-password").value.trim();
            const confirmPassword = document.getElementById("signup-confirm-password").value.trim();

            // Validate client-side
            let hasError = false;

            if (!email) {
                showFieldError('signup-email', 'Vui lòng nhập email');
                hasError = true;
            } else if (!isValidEmail(email)) {
                showFieldError('signup-email', 'Email không hợp lệ');
                hasError = true;
            }

            if (!password) {
                showFieldError('signup-password', 'Vui lòng nhập mật khẩu');
                hasError = true;
            } else if (password.length < 6) {
                showFieldError('signup-password', 'Mật khẩu phải có ít nhất 6 ký tự');
                hasError = true;
            }

            if (!confirmPassword) {
                showFieldError('signup-confirm-password', 'Vui lòng nhập lại mật khẩu');
                hasError = true;
            } else if (password !== confirmPassword) {
                showFieldError('signup-confirm-password', 'Mật khẩu nhập lại không khớp');
                hasError = true;
            }

            if (hasError) return;

            try {
                const response = await fetch("/Auth/ajax-register", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": getAntiForgeryToken()
                    },
                    body: JSON.stringify({ email, password, confirmPassword })
                });

                const result = await response.json();

                if (response.ok && result.success) {
                    // Đăng ký thành công
                    if (window.$ && $.magnificPopup) {
                        $.magnificPopup.close();
                    }

                    // Hiển thị thông báo thành công
                    showGeneralError('register-error-message', '🎉 Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.');

                    // Tự động chuyển sang popup login sau 2 giây
                    setTimeout(() => {
                        if (window.$ && $.magnificPopup) {
                            $.magnificPopup.close();
                            $.magnificPopup.open({
                                items: { src: '#test-popup' },
                                type: 'inline',
                                mainClass: 'mfp-fade'
                            });
                        }
                    }, 2000);
                } else {
                    handleRegisterError(result.message || "Đăng ký thất bại");
                }
            } catch (err) {
                console.error("Fetch error:", err);
                handleRegisterError("Lỗi kết nối đến server");
            }
        });
    }

    // Xử lý quên mật khẩu - CẬP NHẬT để mở popup OTP
    const forgotPasswordForm = document.getElementById("forgot-password-form");
    if (forgotPasswordForm) {
        forgotPasswordForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            const email = document.getElementById("forgot-password-email").value.trim();
            const messageDiv = document.getElementById("forgot-password-message");

            if (!email) {
                showMessage(messageDiv, "Vui lòng nhập email đăng ký", "danger");
                return;
            }

            if (!isValidEmail(email)) {
                showMessage(messageDiv, "Email không hợp lệ", "danger");
                return;
            }

            try {
                console.log("Sending OTP request for email:", email);

                const requestBody = {
                    email: email
                };

                console.log("Request body:", requestBody);

                const response = await fetch("/Auth/send-otp", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": getAntiForgeryToken()
                    },
                    body: JSON.stringify(requestBody)
                });

                console.log("OTP Response status:", response.status);

                if (response.ok) {
                    const result = await response.json();
                    console.log("OTP Response result:", result);

                    if (result.success) {
                        // Lưu email để sử dụng ở các bước sau
                        currentResetEmail = email;

                        // Đóng popup hiện tại và mở popup OTP
                        if (window.$ && $.magnificPopup) {
                            $.magnificPopup.close();
                            $.magnificPopup.open({
                                items: { src: '#otp-popup' },
                                type: 'inline',
                                mainClass: 'mfp-fade'
                            });
                        }
                    } else {
                        showMessage(messageDiv, result.message || "Có lỗi xảy ra. Vui lòng thử lại.", "danger");
                    }
                } else {
                    const errorText = await response.text();
                    console.error("OTP Error response:", errorText);

                    let errorMessage = "Có lỗi xảy ra. Vui lòng thử lại.";
                    try {
                        const errorResult = JSON.parse(errorText);
                        errorMessage = errorResult.message || errorMessage;

                        // Hiển thị lỗi validation chi tiết
                        if (errorResult.errors) {
                            const errorDetails = Object.values(errorResult.errors).flat().join(', ');
                            errorMessage += ` Chi tiết: ${errorDetails}`;
                        }
                    } catch (e) {
                        errorMessage = errorText;
                    }

                    showMessage(messageDiv, errorMessage, "danger");
                }
            } catch (err) {
                console.error("Send OTP error:", err);
                showMessage(messageDiv, "Lỗi kết nối đến server", "danger");
            }
        });
    }

    // Xử lý gửi lại OTP
    const resendOtpLink = document.getElementById("resend-otp");
    if (resendOtpLink) {
        resendOtpLink.addEventListener("click", async (e) => {
            e.preventDefault();

            if (!currentResetEmail) {
                showOtpMessage("Không tìm thấy email. Vui lòng thử lại.", "danger");
                return;
            }

            try {
                console.log("Resending OTP for email:", currentResetEmail);

                const requestBody = {
                    email: currentResetEmail
                };

                const response = await fetch("/Auth/send-otp", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": getAntiForgeryToken()
                    },
                    body: JSON.stringify(requestBody)
                });

                if (response.ok) {
                    const result = await response.json();

                    if (result.success) {
                        showOtpMessage("✅ Mã OTP mới đã được gửi đến email của bạn.", "success");
                    } else {
                        showOtpMessage(result.message || "Có lỗi xảy ra. Vui lòng thử lại.", "danger");
                    }
                } else {
                    const errorText = await response.text();
                    let errorMessage = "Có lỗi xảy ra. Vui lòng thử lại.";
                    try {
                        const errorResult = JSON.parse(errorText);
                        errorMessage = errorResult.message || errorMessage;
                    } catch (e) {
                        errorMessage = errorText;
                    }
                    showOtpMessage(errorMessage, "danger");
                }
            } catch (err) {
                console.error("Resend OTP error:", err);
                showOtpMessage("Lỗi kết nối đến server", "danger");
            }
        });
    }

    // Xử lý xác thực OTP
    const otpForm = document.getElementById("otp-form");
    if (otpForm) {
        otpForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            resetOtpErrors();

            const otp = document.getElementById("otp-code").value.trim();

            if (!otp) {
                showOtpMessage("Vui lòng nhập mã OTP", "danger");
                return;
            }

            if (otp.length !== 6) {
                showOtpMessage("Mã OTP phải có 6 ký tự", "danger");
                return;
            }

            try {
                console.log("Verifying OTP for email:", currentResetEmail);

                const requestBody = {
                    email: currentResetEmail,
                    otp: otp
                };

                console.log("Verify OTP request body:", requestBody);

                const response = await fetch("/Auth/verify-otp", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": getAntiForgeryToken()
                    },
                    body: JSON.stringify(requestBody)
                });

                console.log("Verify OTP Response status:", response.status);

                if (response.ok) {
                    const result = await response.json();

                    if (result.success) {
                        // Đóng popup hiện tại và mở popup nhập mật khẩu mới
                        if (window.$ && $.magnificPopup) {
                            $.magnificPopup.close();
                            $.magnificPopup.open({
                                items: { src: '#new-password-popup' },
                                type: 'inline',
                                mainClass: 'mfp-fade'
                            });
                        }
                    } else {
                        showOtpMessage(result.message || "Mã OTP không hợp lệ", "danger");
                    }
                } else {
                    const errorText = await response.text();
                    console.error("Verify OTP Error response:", errorText);

                    let errorMessage = "Có lỗi xảy ra. Vui lòng thử lại.";
                    try {
                        const errorResult = JSON.parse(errorText);
                        errorMessage = errorResult.message || errorMessage;
                    } catch (e) {
                        errorMessage = errorText;
                    }

                    showOtpMessage(errorMessage, "danger");
                }
            } catch (err) {
                console.error("Verify OTP error:", err);
                showOtpMessage("Lỗi kết nối đến server", "danger");
            }
        });
    }

    // Xử lý đặt lại mật khẩu mới
    const newPasswordForm = document.getElementById("new-password-form");
    if (newPasswordForm) {
        newPasswordForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            resetNewPasswordErrors();

            const newPassword = document.getElementById("new-password").value.trim();
            const confirmNewPassword = document.getElementById("confirm-new-password").value.trim();

            // Validate
            let hasError = false;

            if (!newPassword) {
                showFieldError('new-password', 'Vui lòng nhập mật khẩu mới');
                hasError = true;
            } else if (newPassword.length < 6) {
                showFieldError('new-password', 'Mật khẩu phải có ít nhất 6 ký tự');
                hasError = true;
            }

            if (!confirmNewPassword) {
                showFieldError('confirm-new-password', 'Vui lòng nhập lại mật khẩu mới');
                hasError = true;
            } else if (newPassword !== confirmNewPassword) {
                showFieldError('confirm-new-password', 'Mật khẩu nhập lại không khớp');
                hasError = true;
            }

            if (hasError) return;

            try {
                console.log("Resetting password for email:", currentResetEmail);

                const requestBody = {
                    identifier: currentResetEmail,
                    newPassword: newPassword,
                    otp: document.getElementById("otp-code").value.trim()
                };

                console.log("Reset password request body:", requestBody);

                const response = await fetch("/Auth/reset-password-with-otp", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": getAntiForgeryToken()
                    },
                    body: JSON.stringify(requestBody)
                });

                console.log("Reset password Response status:", response.status);

                if (response.ok) {
                    const result = await response.json();

                    if (result.success) {
                        // Reset form
                        document.getElementById("new-password-form").reset();
                        currentResetEmail = '';

                        // Hiển thị popup thành công
                        showSuccessPopup(result.message || "Mật khẩu đã được đặt lại thành công!");
                    } else {
                        showNewPasswordMessage(result.message || "Đặt lại mật khẩu thất bại", "danger");
                    }
                } else {
                    const errorText = await response.text();
                    console.error("Reset password Error response:", errorText);

                    let errorMessage = "Có lỗi xảy ra. Vui lòng thử lại.";
                    try {
                        const errorResult = JSON.parse(errorText);
                        errorMessage = errorResult.message || errorMessage;
                    } catch (e) {
                        errorMessage = errorText;
                    }

                    showNewPasswordMessage(errorMessage, "danger");
                }
            } catch (err) {
                console.error("Reset password error:", err);
                showNewPasswordMessage("Lỗi kết nối đến server", "danger");
            }
        });
    }

    // Xử lý xem mẫu (preview hình ảnh)
    const previewLinks = document.querySelectorAll(".demo-item .preview-btn-wrapper a:not(.v2)");
    previewLinks.forEach(link => {
        link.addEventListener("click", (e) => {
            e.preventDefault();
            const demoItem = link.closest(".demo-item");
            if (!demoItem) return;

            const imgElement = demoItem.querySelector("img");
            const imageSrc = imgElement ? imgElement.src : null;

            if (imageSrc && window.$ && $.magnificPopup) {
                $.magnificPopup.open({
                    items: { src: imageSrc },
                    type: 'image',
                    mainClass: 'mfp-with-zoom mfp-img-mobile mfp-cv-preview',
                    closeOnContentClick: true,
                    titleSrc: function (item) {
                        return imgElement.alt || 'Mẫu CV';
                    }
                }, 0);
            }
        });
    });
});
