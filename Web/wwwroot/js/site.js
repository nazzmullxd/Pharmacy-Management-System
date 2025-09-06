// Modern Pharmacy Management System JavaScript

$(document).ready(function () {
    // Sidebar Toggle Functionality
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Initialize tooltips and popovers
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Dashboard functionality
    if (window.location.pathname === '/' || window.location.pathname === '/Index') {
        initializeDashboard();
    }

    // Initialize search and forms
    initializeSearch();
    initializeFormValidation();
});

// Dashboard initialization
function initializeDashboard() {
    animateKPICards();
    initializeCharts();
    setInterval(refreshDashboardData, 30000);
}

// Animate KPI Cards
function animateKPICards() {
    $('.kpi-card').each(function(index) {
        $(this).css('animation-delay', (index * 0.1) + 's');
    });
}

// Initialize Charts
function initializeCharts() {
    // Sales Chart
    const salesCtx = document.getElementById('salesChart');
    if (salesCtx) {
        new Chart(salesCtx, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                datasets: [{
                    label: 'Sales',
                    data: [12000, 19000, 15000, 25000, 22000, 30000],
                    borderColor: '#3498db',
                    backgroundColor: 'rgba(52, 152, 219, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    // Stock Chart
    const stockCtx = document.getElementById('stockChart');
    if (stockCtx) {
        new Chart(stockCtx, {
            type: 'doughnut',
            data: {
                labels: ['In Stock', 'Low Stock', 'Out of Stock'],
                datasets: [{
                    data: [65, 25, 10],
                    backgroundColor: ['#27ae60', '#f39c12', '#e74c3c']
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' }
                }
            }
        });
    }
}

// Refresh Dashboard Data
function refreshDashboardData() {
    $('.kpi-card .kpi-value').each(function() {
        const $value = $(this);
        const currentValue = parseInt($value.text().replace(/[^\d]/g, ''));
        const newValue = currentValue + Math.floor(Math.random() * 100) - 50;
        $value.text(newValue.toLocaleString());
    });
}

// Initialize Search
function initializeSearch() {
    $('.search-input').on('input', function() {
        const searchTerm = $(this).val().toLowerCase();
        const $table = $(this).closest('.table-container').find('table tbody tr');
        
        $table.each(function() {
            const $row = $(this);
            const text = $row.text().toLowerCase();
            $row.toggle(text.includes(searchTerm));
        });
    });
}

// Initialize Form Validation
function initializeFormValidation() {
    $('form').on('submit', function(e) {
        const $form = $(this);
        let isValid = true;
        
        $form.find('.is-invalid').removeClass('is-invalid');
        $form.find('.invalid-feedback').remove();
        
        $form.find('[required]').each(function() {
            const $field = $(this);
            if (!$field.val().trim()) {
                $field.addClass('is-invalid');
                $field.after('<div class="invalid-feedback">This field is required.</div>');
                isValid = false;
            }
        });
        
        if (!isValid) {
            e.preventDefault();
        }
    });
}

// Utility Functions
function showAlert(message, type) {
    const alertId = 'alert-' + Date.now();
    const $alert = $(`
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('.main-content').prepend($alert);
    setTimeout(() => $alert.alert('close'), 5000);
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function formatDate(date) {
    return new Intl.DateTimeFormat('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    }).format(new Date(date));
}