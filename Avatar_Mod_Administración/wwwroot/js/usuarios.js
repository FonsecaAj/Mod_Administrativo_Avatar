document.addEventListener('DOMContentLoaded', function () {
    inicializarUsuarios();
});

/* Función Principal de Inicialización */
function inicializarUsuarios() {
    // Auto-ocultar alertas
    inicializarAlertas();

    // Validación en tiempo real
    inicializarValidacion();

    // Animaciones de entrada
    inicializarAnimaciones();

    // Tooltips
    inicializarTooltips();

    // Prevenir doble submit
    prevenirDobleSubmit();

    // Contador de caracteres
    agregarContadorCaracteres();
}

/* Auto-ocultar Alertas */
function inicializarAlertas() {
    const alerts = document.querySelectorAll('.alert-dismissible');

    alerts.forEach(function (alert) {
        // Auto-cerrar después de 5 segundos
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 5000);

        // Animación de salida suave
        alert.addEventListener('closed.bs.alert', function () {
            alert.style.transition = 'all 0.3s ease';
            alert.style.opacity = '0';
            alert.style.transform = 'translateX(100%)';
        });
    });
}

/* Validación en Tiempo Real */
function inicializarValidacion() {
    // Validar Email
    const emailInput = document.querySelector('input[name="Input.Email"]');
    if (emailInput) {
        emailInput.addEventListener('input', function () {
            validarEmail(this);
        });
        emailInput.addEventListener('blur', function () {
            validarEmail(this);
        });
    }

    // Validar Nombre
    const nombreInput = document.querySelector('input[name="Input.Nombre"]');
    if (nombreInput) {
        nombreInput.addEventListener('input', function () {
            validarNombre(this);
        });
        nombreInput.addEventListener('blur', function () {
            validarNombre(this);
        });
    }

    // Validar Identificación
    const identificacionInput = document.querySelector('input[name="Input.Identificacion"]');
    if (identificacionInput) {
        identificacionInput.addEventListener('input', function () {
            validarIdentificacion(this);
        });
    }

    // Validar Contraseña
    const contrasennaInput = document.querySelector('input[name="Input.Contrasenna"]');
    if (contrasennaInput) {
        contrasennaInput.addEventListener('input', function () {
            validarContrasenna(this);
        });
    }

    // Feedback visual mejorado para todos los inputs
    const allInputs = document.querySelectorAll('.form-control, .form-select');
    allInputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.parentElement.style.transform = 'scale(1.01)';
            this.parentElement.style.transition = 'transform 0.2s ease';
        });

        input.addEventListener('blur', function () {
            this.parentElement.style.transform = 'scale(1)';
        });
    });
}

/* Validar Email */
function validarEmail(input) {
    const valor = input.value.trim().toLowerCase();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    input.classList.remove('is-invalid', 'is-valid');

    if (valor === '') {
        return true;
    }

    if (!emailRegex.test(valor)) {
        input.classList.add('is-invalid');
        mostrarError(input, 'Formato de email inválido');
        return false;
    }

    if (!valor.endsWith('@cuc.cr') && !valor.endsWith('@cuc.ac.cr')) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El email debe ser @cuc.cr o @cuc.ac.cr');
        return false;
    }

    input.classList.add('is-valid');
    ocultarError(input);
    return true;
}

function validarNombre(input) {
    const valor = input.value.trim();

    input.classList.remove('is-invalid', 'is-valid');

    if (valor === '') {
        return true;
    }

    if (/^\s+$/.test(input.value)) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El nombre no puede contener solo espacios');
        return false;
    }

    if (valor.length < 3) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El nombre debe tener al menos 3 caracteres');
        return false;
    }

    input.classList.add('is-valid');
    ocultarError(input);
    return true;
}


function validarIdentificacion(input) {
    const valor = input.value.trim();

    input.classList.remove('is-invalid', 'is-valid');

    if (valor === '') {
        return true;
    }

    if (valor.length < 3) {
        input.classList.add('is-invalid');
        mostrarError(input, 'La identificación debe tener al menos 3 caracteres');
        return false;
    }

    input.classList.add('is-valid');
    ocultarError(input);
    return true;
}


function validarContrasenna(input) {
    const valor = input.value;

    input.classList.remove('is-invalid', 'is-valid');

    // Si es edición y está vacío, está bien 
    const esEdicion = window.location.pathname.includes('Editar');
    if (esEdicion && valor === '') {
        ocultarError(input);
        return true;
    }

    if (valor === '') {
        return true;
    }

    if (valor.length < 6) {
        input.classList.add('is-invalid');
        mostrarError(input, 'La contraseña debe tener al menos 6 caracteres');
        return false;
    }

    input.classList.add('is-valid');
    ocultarError(input);
    return true;
}


function mostrarError(input, mensaje) {
    const formText = input.parentElement.querySelector('.form-text');
    let errorDiv = input.parentElement.querySelector('.validation-error');

    if (!errorDiv) {
        errorDiv = document.createElement('div');
        errorDiv.className = 'validation-error text-danger mt-2';
        errorDiv.style.fontSize = '0.875rem';
        errorDiv.style.fontWeight = '500';

        if (formText) {
            formText.insertAdjacentElement('afterend', errorDiv);
        } else {
            input.insertAdjacentElement('afterend', errorDiv);
        }
    }

    errorDiv.innerHTML = '<i class="bi bi-exclamation-circle me-1"></i>' + mensaje;
    errorDiv.style.animation = 'shake 0.3s ease';
}

function ocultarError(input) {
    const errorDiv = input.parentElement.querySelector('.validation-error');
    if (errorDiv) {
        errorDiv.remove();
    }
}


function confirmarEliminacion(email) {
    // Establecer los valores en el modal
    document.getElementById('emailEliminar').textContent = email;
    document.getElementById('emailInput').value = email;

    // Mostrar el modal con animación
    const modalElement = document.getElementById('modalEliminar');
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });

    modal.show();

    // Animación de entrada
    modalElement.addEventListener('shown.bs.modal', function () {
        this.querySelector('.modal-content').style.animation = 'modalSlideIn 0.3s ease';
    });
}

function inicializarAnimaciones() {
    // Animar cards al cargar
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';

        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });

    // Animar filas de tabla al hacer hover
    const tableRows = document.querySelectorAll('.table-usuarios tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.01)';
            this.style.transition = 'all 0.2s ease';
        });

        row.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    });
}

function inicializarTooltips() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl =>
        new bootstrap.Tooltip(tooltipTriggerEl, {
            trigger: 'hover',
            delay: { show: 300, hide: 100 }
        })
    );
}

/* Prevenir Doble Submit */
function prevenirDobleSubmit() {
    const forms = document.querySelectorAll('form');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('button[type="submit"]');

            if (submitBtn && submitBtn.classList.contains('submitting')) {
                e.preventDefault();
                return false;
            }

            if (submitBtn) {
                submitBtn.classList.add('submitting');
                submitBtn.disabled = true;

                // Agregar spinner
                const originalHTML = submitBtn.innerHTML;
                submitBtn.innerHTML = originalHTML + ' <span class="loading-spinner"></span>';

                // Re-habilitar después de 5 segundos
                setTimeout(() => {
                    submitBtn.classList.remove('submitting');
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalHTML;
                }, 5000);
            }
        });
    });
}


function agregarContadorCaracteres() {
    const nombreInput = document.querySelector('input[name="Input.Nombre"]');
    const identificacionInput = document.querySelector('input[name="Input.Identificacion"]');

    const inputs = [nombreInput, identificacionInput].filter(Boolean);

    inputs.forEach(input => {
        const maxLength = input.getAttribute('maxlength') || 100;

        // Crear elemento contador
        const contador = document.createElement('small');
        contador.className = 'form-text text-muted text-end d-block';
        contador.style.marginTop = '0.25rem';

        // Insertar después del form-text si existe, sino después del input
        const formText = input.parentElement.querySelector('.form-text');
        if (formText) {
            formText.insertAdjacentElement('afterend', contador);
        } else {
            input.parentElement.appendChild(contador);
        }

        // Actualizar contador
        const actualizarContador = () => {
            const length = input.value.length;
            const restante = maxLength - length;

            contador.textContent = `${length}/${maxLength} caracteres`;

            if (restante < 10) {
                contador.style.color = 'var(--danger-color)';
            } else if (restante < 20) {
                contador.style.color = 'var(--warning-color)';
            } else {
                contador.style.color = 'var(--text-secondary)';
            }
        };

        input.addEventListener('input', actualizarContador);
        actualizarContador();
    });
}

/* Animación de Paginación */
function animarCambioPagina() {
    const paginationLinks = document.querySelectorAll('.pagination .page-link');

    paginationLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            if (!this.parentElement.classList.contains('disabled') &&
                !this.parentElement.classList.contains('active')) {

                const tbody = document.querySelector('.table-usuarios tbody');
                if (tbody) {
                    tbody.style.opacity = '0';
                    tbody.style.transform = 'translateY(20px)';
                }
            }
        });
    });
}

/* Filtro en Tiempo Real para Búsqueda */
function inicializarBusquedaEnVivo() {
    const filtroInputs = document.querySelectorAll('.filtros-card input, .filtros-card select');

    filtroInputs.forEach(input => {
        input.addEventListener('change', function () {
            // Animación visual al aplicar filtro
            const tableCard = document.querySelector('.usuarios-table-card');
            if (tableCard) {
                tableCard.style.animation = 'fadeIn 0.5s ease';
            }
        });
    });
}

/* Limpiar Formulario */
function limpiarFormulario(formId) {
    const form = document.getElementById(formId);
    if (form) {
        form.reset();

        // Remover clases de validación
        form.querySelectorAll('.is-invalid, .is-valid').forEach(el => {
            el.classList.remove('is-invalid', 'is-valid');
        });

        // Ocultar mensajes de error
        form.querySelectorAll('.validation-error').forEach(el => {
            el.remove();
        });
    }
}

/* Resaltar Fila al Cargar desde Detalle */
function resaltarFilaActual() {
    const urlParams = new URLSearchParams(window.location.search);
    const emailDestacado = urlParams.get('highlight');

    if (emailDestacado) {
        const rows = document.querySelectorAll('.table-usuarios tbody tr');
        rows.forEach(row => {
            const emailCell = row.querySelector('td:first-child');
            if (emailCell && emailCell.textContent.trim() === emailDestacado) {
                row.style.backgroundColor = 'rgba(195, 163, 142, 0.2)';
                row.style.animation = 'pulse 2s ease-in-out';

                // Scroll suave a la fila
                setTimeout(() => {
                    row.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }, 500);

                // Remover resaltado después de 3 segundos
                setTimeout(() => {
                    row.style.backgroundColor = '';
                    row.style.animation = '';
                }, 3000);
            }
        });
    }
}


const pulseStyle = document.createElement('style');
pulseStyle.textContent = `
    @keyframes pulse {
        0%, 100% { transform: scale(1); }
        50% { transform: scale(1.02); }
    }
`;
document.head.appendChild(pulseStyle);


document.addEventListener('DOMContentLoaded', function () {
    animarCambioPagina();
    inicializarBusquedaEnVivo();
    resaltarFilaActual();
});


window.confirmarEliminacion = confirmarEliminacion;
window.limpiarFormulario = limpiarFormulario;