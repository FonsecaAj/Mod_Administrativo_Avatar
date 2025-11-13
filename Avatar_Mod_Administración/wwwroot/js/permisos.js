document.addEventListener('DOMContentLoaded', function () {
    // Inicializar tooltips de Bootstrap si existen
    initializeTooltips();

    // Auto-dismiss de alertas después de 5 segundos
    autoHideAlerts();

    // Animación de entrada suave
    animatePageEntry();
});

/* Inicializa los tooltips de Bootstrap */
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

/* Auto-ocultar alertas después de un tiempo */
function autoHideAlerts() {
    const alerts = document.querySelectorAll('.alert-dismissible');

    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000); // 5 segundos
    });
}

/* Animación de entrada de página */
function animatePageEntry() {
    const container = document.querySelector('.permisos-container');
    if (container) {
        container.style.opacity = '0';
        setTimeout(() => {
            container.style.transition = 'opacity 0.5s ease-out';
            container.style.opacity = '1';
        }, 100);
    }
}

/* Mostrar loading en botón */
function showButtonLoading(button, text = 'Cargando') {
    const originalContent = button.innerHTML;
    button.setAttribute('data-original-content', originalContent);
    button.disabled = true;
    button.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
        ${text}...
    `;
}

/* Restaurar botón después de loading */
function hideButtonLoading(button) {
    const originalContent = button.getAttribute('data-original-content');
    if (originalContent) {
        button.innerHTML = originalContent;
        button.disabled = false;
        button.removeAttribute('data-original-content');
    }
}

/* Animar cambios en checkboxes */
function animateCheckboxChange(checkbox) {
    const row = checkbox.closest('tr');
    if (row) {
        row.style.transition = 'all 0.3s ease';
        row.style.transform = 'scale(1.02)';

        setTimeout(() => {
            row.style.transform = 'scale(1)';
        }, 300);
    }
}

/* Mostrar notificación temporal */
function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-permisos position-fixed top-0 start-50 translate-middle-x mt-3`;
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

/* Agregar animación al contador de selección */
function animateCounter(element) {
    element.style.animation = 'none';
    setTimeout(() => {
        element.style.animation = 'successPulse 0.6s ease';
    }, 10);
}

/* Validar selección de rol */
function validateRolSelection() {
    const rolSelect = document.getElementById('rolSelect');
    if (!rolSelect || !rolSelect.value) {
        showNotification('Por favor seleccione un rol', 'danger');
        rolSelect.focus();
        return false;
    }
    return true;
}

/* Animar la matriz de permisos al mostrarse */
function animateMatrizPermisos() {
    const matriz = document.getElementById('matrizPermisos');
    if (matriz) {
        matriz.style.display = 'block';
        matriz.style.animation = 'expandIn 0.5s ease-out';
    }
}

/* Highlight de filas al hacer hover mejorado */
function enhanceTableInteractivity() {
    const rows = document.querySelectorAll('.table-permisos tbody tr');

    rows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.005)';
            this.style.boxShadow = '0 2px 8px rgba(47, 73, 110, 0.12)';
        });

        row.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
            this.style.boxShadow = 'none';
        });
    });
}

/* Smooth scroll al resumen de selección */
function scrollToResumen() {
    const resumen = document.getElementById('resumenSeleccion');
    if (resumen && resumen.style.display !== 'none') {
        resumen.scrollIntoView({
            behavior: 'smooth',
            block: 'nearest'
        });
    }
}

/* Efecto de confirmación visual al guardar */
function showSaveConfirmation() {
    const form = document.getElementById('formPermisos');
    if (form) {
        form.classList.add('success-animation');
        setTimeout(() => {
            form.classList.remove('success-animation');
        }, 600);
    }
}

// Exportar funciones para uso global
window.PermisosUtils = {
    showButtonLoading,
    hideButtonLoading,
    animateCheckboxChange,
    showNotification,
    animateCounter,
    validateRolSelection,
    animateMatrizPermisos,
    enhanceTableInteractivity,
    scrollToResumen,
    showSaveConfirmation,
    playSuccessSound
};

// Agregar estilos de animación adicionales
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeOut {
        from {
            opacity: 1;
            transform: translateY(0);
        }
        to {
            opacity: 0;
            transform: translateY(-20px);
        }
    }
`;
document.head.appendChild(style);