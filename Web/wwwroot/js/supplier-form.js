// Supplier Form Enhancement Script
(function() {
    'use strict';
    
    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        const form = document.getElementById('supplierForm');
        const formFields = form.querySelectorAll('.form-field');
        const progressBar = document.getElementById('formProgress');
        const completedFieldsSpan = document.getElementById('completedFields');
        const totalFieldsSpan = document.getElementById('totalFields');
        const submitBtn = document.getElementById('submitBtn');
        const validationAlert = document.getElementById('validationAlert');
        
        // Set total fields count
        const totalFields = formFields.length;
        totalFieldsSpan.textContent = totalFields;
        
        // Real-time validation and progress tracking
        formFields.forEach(field => {
            field.addEventListener('input', function() {
                validateField(this);
                updateProgress();
            });
            
            field.addEventListener('blur', function() {
                validateField(this);
            });
            
            field.addEventListener('focus', function() {
                this.classList.remove('shake');
            });
        });
        
        // Form submission handling
        form.addEventListener('submit', function(event) {
            event.preventDefault();
            event.stopPropagation();
            
            let isValid = true;
            
            // Validate all fields
            formFields.forEach(field => {
                if (!validateField(field)) {
                    isValid = false;
                    field.classList.add('shake');
                    setTimeout(() => field.classList.remove('shake'), 500);
                }
            });
            
            if (isValid) {
                // Show loading state
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Creating Supplier...';
                
                // Hide validation alert
                validationAlert.style.display = 'none';
                
                // Submit the form
                setTimeout(() => {
                    form.submit();
                }, 1000);
            } else {
                // Show validation alert
                validationAlert.style.display = 'block';
                validationAlert.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            
            form.classList.add('was-validated');
        });
        
        // Field validation function
        function validateField(field) {
            const value = field.value.trim();
            const isRequired = field.hasAttribute('required');
            const fieldType = field.type;
            let isValid = true;
            
            // Check if required field is empty
            if (isRequired && !value) {
                isValid = false;
            }
            
            // Specific validations
            if (value) {
                if (fieldType === 'email') {
                    // Simple email validation
                    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                    isValid = emailPattern.test(value);
                } else if (fieldType === 'tel') {
                    const cleanPhone = value.replace(/[^0-9]/g, '');
                    isValid = cleanPhone.length >= 10;
                } else if (field.id === 'supplierName') {
                    isValid = value.length >= 2;
                }
            }
            
            // Update field appearance
            if (isValid) {
                field.classList.remove('is-invalid');
                field.classList.add('is-valid');
            } else {
                field.classList.remove('is-valid');
                field.classList.add('is-invalid');
            }
            
            return isValid;
        }
        
        // Update progress bar
        function updateProgress() {
            const validFields = form.querySelectorAll('.form-field.is-valid').length;
            const progress = (validFields / totalFields) * 100;
            
            progressBar.style.width = progress + '%';
            completedFieldsSpan.textContent = validFields;
            
            // Change progress bar color based on completion
            progressBar.classList.remove('bg-danger', 'bg-warning', 'bg-success');
            if (progress < 40) {
                progressBar.classList.add('bg-danger');
            } else if (progress < 80) {
                progressBar.classList.add('bg-warning');
            } else {
                progressBar.classList.add('bg-success');
            }
        }
        
        // Auto-format phone number
        document.getElementById('phoneNumber').addEventListener('input', function(e) {
            let value = e.target.value.replace(/[^0-9]/g, '');
            if (value.length >= 10) {
                value = value.replace(/(\d{3})(\d{3})(\d{4})/, '($1) $2-$3');
            }
            e.target.value = value;
        });
        
        // Auto-capitalize supplier name
        document.getElementById('supplierName').addEventListener('input', function(e) {
            const words = e.target.value.split(' ');
            const capitalizedWords = words.map(word => 
                word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
            );
            e.target.value = capitalizedWords.join(' ');
        });
        
        // Clear form function
        window.clearForm = function() {
            if (confirm('Are you sure you want to clear all form data? This action cannot be undone.')) {
                form.reset();
                formFields.forEach(field => {
                    field.classList.remove('is-valid', 'is-invalid');
                });
                form.classList.remove('was-validated');
                updateProgress();
                
                // Reset submit button
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-save me-2"></i>Create Supplier';
                
                // Hide validation alert
                validationAlert.style.display = 'none';
            }
        };
        
        // Initial progress update
        updateProgress();
        
        // Auto-focus first field
        setTimeout(() => {
            document.getElementById('supplierName').focus();
        }, 500);
    });
})();