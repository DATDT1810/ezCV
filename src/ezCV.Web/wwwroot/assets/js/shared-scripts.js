// === GLOBAL STATE ===
let sharedContactsState = [];
let sharedSocialLinkIndex = 0;

// === UTILITY FUNCTIONS ===
function sharedGetContactIconClass(type) {
    const icons = {
        'email': 'fas fa-envelope',
        'phone': 'fas fa-phone',
        'address': 'fas fa-map-marker-alt',
        'birthday': 'fas fa-birthday-cake',
        'gender': 'fas fa-venus-mars',
        'facebook': 'fab fa-facebook-f',
        'linkedin': 'fab fa-linkedin-in',
        'github': 'fab fa-github',
        'website': 'fas fa-globe'
    };
    return icons[type] || 'fas fa-circle-info';
}

function sharedGetContactTypeName(type) {
    const names = {
        'email': 'email',
        'phone': 'số điện thoại',
        'address': 'địa chỉ',
        'birthday': 'ngày sinh',
        'gender': 'giới tính',
        'facebook': 'Facebook',
        'linkedin': 'LinkedIn',
        'github': 'GitHub',
        'website': 'Website'
    };
    return names[type] || 'thông tin';
}

function sharedGetContactDefaultValue(type) {
    const defaults = {
        'email': 'Nhập email',
        'phone': 'Nhập số điện thoại',
        'address': 'Nhập địa chỉ',
        'birthday': 'dd/MM/yyyy',
        'gender': 'Nhập giới tính',
        'website': 'https://example.com',
        'facebook': 'https://facebook.com/username',
        'linkedin': 'https://linkedin.com/in/username',
        'github': 'https://github.com/username'
    };
    return defaults[type] || 'Nhập thông tin';
}

// === DATE PARSING ===
function sharedParseDateOnly(dateString) {
    if (!dateString) return null;

    if (dateString.includes('/') && dateString.split('/').length === 3) {
        const [day, month, year] = dateString.split('/');
        if (day && month && year) {
            return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
        }
    }

    if (dateString.includes('/') && dateString.split('/').length === 2) {
        const [month, year] = dateString.split('/');
        if (month && year) {
            return `${year}-${month.padStart(2, '0')}-01`;
        }
    }

    if (/^\d{4}$/.test(dateString)) {
        return `${dateString}-01-01`;
    }

    if (/^\d{4}-\d{2}-\d{2}$/.test(dateString)) {
        return dateString;
    }

    return null;
}

function sharedParseDateRange(dateText) {
    if (!dateText) return { startDate: null, endDate: null };

    const parts = dateText.split(' - ');
    if (parts.length !== 2) return { startDate: null, endDate: null };

    let startDate = sharedParseDateOnly(parts[0].trim());
    let endDate = parts[1].trim().toLowerCase() === 'present' ? 
        null : sharedParseDateOnly(parts[1].trim());

    return { startDate, endDate };
}

// === FORM MANAGEMENT ===
function sharedCreateHiddenInput(form, name, value) {
    if (!value) return;
    
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = name;
    input.value = value;
    input.className = 'dynamic-section-input';
    form.appendChild(input);
}

function sharedAddToSocialLinks(type, value) {
    const form = document.getElementById('cvForm');
    sharedCreateHiddenInput(form, `Profile.SocialLinks[${sharedSocialLinkIndex}].PlatformName`, type);
    sharedCreateHiddenInput(form, `Profile.SocialLinks[${sharedSocialLinkIndex}].Url`, value);
    sharedSocialLinkIndex++;
}

// === MODAL FUNCTIONS ===
function sharedOpenContactModal() {
    document.getElementById('sharedContactModal').style.display = 'flex';
}

function sharedCloseContactModal() {
    document.getElementById('sharedContactModal').style.display = 'none';
}

function sharedConfirmAddContact() {
    const type = document.getElementById('sharedContactTypeSelect').value;
    // This will be implemented in each template
    console.log('Add contact type:', type);
    sharedCloseContactModal();
}

// === LOADING FUNCTIONS ===
function sharedShowLoading() {
    document.getElementById('sharedLoadingOverlay').style.display = 'flex';
}

function sharedHideLoading() {
    document.getElementById('sharedLoadingOverlay').style.display = 'none';
}

// === VALIDATION ===
function sharedValidateForm(fullNameSelector, emailSelector) {
    const fullName = document.querySelector(fullNameSelector)?.innerText.trim();
    const email = document.querySelector(emailSelector)?.innerText.trim();

    if (!fullName || fullName === 'Họ và tên') {
        alert('Vui lòng nhập họ và tên');
        return false;
    }

    if (!email || email === 'Nhập email') {
        alert('Vui lòng nhập email liên hệ');
        return false;
    }

    const emailRegex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
    if (!emailRegex.test(email)) {
        alert('Email không đúng định dạng.');
        return false;
    }

    return true;
}

// === PLACEHOLDER MANAGEMENT ===
function sharedSetupPlaceholders() {
    const editableFields = document.querySelectorAll('.shared-editable[data-placeholder]');
    editableFields.forEach(field => {
        if (!field.innerText.trim()) {
            field.classList.add('empty');
        }

        field.addEventListener('focus', function() {
            this.classList.remove('empty');
            if (this.innerText === this.dataset.placeholder) {
                this.innerText = '';
            }
        });

        field.addEventListener('blur', function() {
            if (this.innerText.trim() === '') {
                this.innerText = this.dataset.placeholder;
                this.classList.add('empty');
            }
        });
    });
}