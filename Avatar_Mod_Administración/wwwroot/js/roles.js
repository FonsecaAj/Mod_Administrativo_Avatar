document.addEventListener('DOMContentLoaded', function () {
    // Inicializar tooltips de Bootstrap
    initializeTooltips();

    // Auto-dismiss de alertas después de 5 segundos
    autoHideAlerts();

    // Animación de entrada suave
    animatePageEntry();

    // Mejorar interactividad de la tabla
    enhanceTableInteractivity();

    // Validación en tiempo real del formulario
    setupFormValidation();
});

/**
 * Inicializa los tooltips de Bootstrap
 */
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="tooltip"]')
    );

    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}


function autoHideAlerts() {
    const alerts = document.querySelectorAll('.alert-dismissible');

    alerts.forEach(alert => {
        // Agregar animación de entrada
        alert.style.animation = 'slideDown 0.4s ease-out';

        setTimeout(() => {
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                alert.style.animation = 'fadeOut 0.4s ease-out';

                setTimeout(() => {
                    bsAlert.close();
                }, 400);
            }
        }, 5000); // 5 segundos
    });
}


function animatePageEntry() {
    const container = document.querySelector('.roles-container, .container');
    if (container) {
        container.style.opacity = '0';
        setTimeout(() => {
            container.style.transition = 'opacity 0.5s ease-out';
            container.style.opacity = '1';
        }, 100);
    }
}


function enhanceTableInteractivity() {
    const rows = document.querySelectorAll('.table-roles tbody tr');

    rows.forEach(row => {
        // Skip empty state rows
        if (row.querySelector('.empty-state')) return;

        row.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.01)';
            this.style.boxShadow = '0 2px 8px rgba(47, 73, 110, 0.12)';
        });

        row.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
            this.style.boxShadow = 'none';
        });
    });
}


function setupFormValidation() {
    const nombreInput = document.querySelector('input[name="Input.Nombre"]');

    if (nombreInput) {
        nombreInput.addEventListener('input', function () {
            // Remover caracteres no permitidos en tiempo real
            this.value = this.value.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '');

            // Prevenir espacios consecutivos
            this.value = this.value.replace(/\s{2,}/g, ' ');

            // Validación visual
            if (this.value.length >= 3) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else if (this.value.length > 0) {
                this.classList.remove('is-valid');
                this.classList.add('is-invalid');
            } else {
                this.classList.remove('is-valid', 'is-invalid');
            }
        });

        // Prevenir el primer espacio
        nombreInput.addEventListener('keydown', function (e) {
            if (e.key === ' ' && this.value.length === 0) {
                e.preventDefault();
            }
        });
    }
}

function confirmarEliminacion(id, nombre) {
    const nombreElement = document.getElementById('nombreEliminar');
    const idInput = document.getElementById('idInput');

    if (nombreElement && idInput) {
        nombreElement.textContent = nombre;
        idInput.value = id;

        // Animar el texto del nombre
        nombreElement.style.animation = 'successPulse 0.6s ease';

        const modalElement = document.getElementById('modalEliminar');
        if (modalElement && typeof bootstrap !== 'undefined') {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();

            // Agregar efecto de entrada al modal
            modalElement.addEventListener('shown.bs.modal', function () {
                const modalDialog = this.querySelector('.modal-dialog');
                if (modalDialog) {
                    modalDialog.style.animation = 'slideUp 0.3s ease-out';
                }
            }, { once: true });
        }
    }
}


function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-rol position-fixed top-0 start-50 translate-middle-x mt-3`;
    notification.style.zIndex = '9999';
    notification.style.minWidth = '300px';
    notification.innerHTML = `
        <i class="bi bi-${type === 'success' ? 'check-circle-fill' : 'exclamation-triangle-fill'}"></i>
        <div>
            <strong>${type === 'success' ? '¡Éxito!' : 'Atención'}</strong>
            <p class="mb-0">${message}</p>
        </div>
    `;

    document.body.appendChild(notification);

    // Animar entrada
    setTimeout(() => {
        notification.style.animation = 'slideDown 0.4s ease-out';
    }, 10);

    // Auto-remover después de 3 segundos
    setTimeout(() => {
        notification.style.animation = 'fadeOut 0.4s ease-out';
        setTimeout(() => {
            notification.remove();
        }, 400);
    }, 3000);
}


function validateForm(form) {
    const nombreInput = form.querySelector('input[name="Input.Nombre"]');

    if (!nombreInput || !nombreInput.value.trim()) {
        showNotification('El nombre del rol es obligatorio', 'danger');
        nombreInput.focus();
        return false;
    }

    if (nombreInput.value.trim().length < 3) {
        showNotification('El nombre debe tener al menos 3 caracteres', 'danger');
        nombreInput.focus();
        return false;
    }

    return true;
}


function showButtonLoading(button, text = 'Procesando') {
    const originalContent = button.innerHTML;
    button.setAttribute('data-original-content', originalContent);
    button.disabled = true;
    button.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
        ${text}...
    `;
}

function hideButtonLoading(button) {
    const originalContent = button.getAttribute('data-original-content');
    if (originalContent) {
        button.innerHTML = originalContent;
        button.disabled = false;
        button.removeAttribute('data-original-content');
    }
}


function setupFormSubmitAnimation() {
    const forms = document.querySelectorAll('form');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!validateForm(this)) {
                e.preventDefault();
                return;
            }

            const submitButton = this.querySelector('button[type="submit"]');
            if (submitButton) {
                showButtonLoading(submitButton, 'Guardando');
            }

            // Animación del formulario
            this.style.animation = 'successPulse 0.6s ease';
        });
    });
}

// Configurar animación de envío de formularios
setupFormSubmitAnimation();

// Exportar funciones para uso global
window.RolesUtils = {
    confirmarEliminacion,
    showNotification,
    showButtonLoading,
    hideButtonLoading,
    validateForm,
    initializeTooltips
};

const style = document.createElement('style');
style.textContent = `
    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateX(-50%) translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateX(-50%) translateY(0);
        }
    }
    
    @keyframes fadeOut {
        from {
            opacity: 1;
            transform: translateX(-50%) translateY(0);
        }
        to {
            opacity: 0;
            transform: translateX(-50%) translateY(-20px);
        }
    }
    
    @keyframes successPulse {
        0%, 100% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.05);
        }
    }
    
    .form-control.is-valid {
        border-color: var(--success-color);
        padding-right: calc(1.5em + 0.75rem);
        background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 8 8'%3e%3cpath fill='%237BA892' d='M2.3 6.73L.6 4.53c-.4-1.04.46-1.4 1.1-.8l1.1 1.4 3.4-3.8c.6-.63 1.6-.27 1.2.7l-4 4.6c-.43.5-.8.4-1.1.1z'/%3e%3c/svg%3e");
        background-repeat: no-repeat;
        background-position: right calc(0.375em + 0.1875rem) center;
        background-size: calc(0.75em + 0.375rem) calc(0.75em + 0.375rem);
    }
    
    .form-control.is-valid:focus {
        border-color: var(--success-color);
        box-shadow: 0 0 0 0.25rem rgba(123, 168, 146, 0.15);
    }
`;
document.head.appendChild(style);