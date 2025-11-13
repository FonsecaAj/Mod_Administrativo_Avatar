document.addEventListener('DOMContentLoaded', function () {
    inicializarModulos();
});

function inicializarModulos() {
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

    // Inicializar switch de estado
    inicializarSwitchEstado();
}


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


function inicializarValidacion() {
    const nombreInput = document.querySelector('input[name="Input.Nombre"]');

    if (nombreInput) {
        // Validación mientras el usuario escribe
        nombreInput.addEventListener('input', function () {
            validarNombreModulo(this);
        });

        // Validación al perder el foco
        nombreInput.addEventListener('blur', function () {
            validarNombreModulo(this);
        });

        // Feedback visual mejorado
        nombreInput.addEventListener('focus', function () {
            this.parentElement.style.transform = 'scale(1.01)';
            this.parentElement.style.transition = 'transform 0.2s ease';
        });

        nombreInput.addEventListener('blur', function () {
            this.parentElement.style.transform = 'scale(1)';
        });
    }
}

function validarNombreModulo(input) {
    const valor = input.value.trim();
    const regex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]*$/;

    // Remover clases previas
    input.classList.remove('is-invalid', 'is-valid');

    // Validar caracteres permitidos
    if (!regex.test(valor)) {
        input.classList.add('is-invalid');
        mostrarError(input, 'Solo se permiten letras y espacios');
        return false;
    }

    // Validar longitud mínima
    if (valor.length > 0 && valor.length < 3) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El nombre debe tener al menos 3 caracteres');
        return false;
    }

    // Validar longitud máxima
    if (valor.length > 100) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El nombre no puede exceder 100 caracteres');
        return false;
    }

    // Validar espacios consecutivos
    if (/\s{2,}/.test(valor)) {
        input.classList.add('is-invalid');
        mostrarError(input, 'No se permiten espacios consecutivos');
        return false;
    }

    // Validación exitosa
    if (valor.length >= 3) {
        input.classList.add('is-valid');
        ocultarError(input);
        return true;
    }

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


function confirmarEliminacion(id, nombre) {
    // Establecer los valores en el modal
    document.getElementById('nombreEliminar').textContent = nombre;
    document.getElementById('idInput').value = id;

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


function inicializarSwitchEstado() {
    const activoCheckbox = document.querySelector('input[name="Input.Activo"]');
    const estadoTexto = document.getElementById('estadoTexto');

    if (activoCheckbox && estadoTexto) {
        activoCheckbox.addEventListener('change', function () {
            estadoTexto.textContent = this.checked ? 'Activo' : 'Inactivo';

            // Animación de cambio
            estadoTexto.style.animation = 'fadeIn 0.3s ease';

            // Cambiar color del texto
            if (this.checked) {
                estadoTexto.style.color = 'var(--success-color)';
            } else {
                estadoTexto.style.color = 'var(--text-secondary)';
            }
        });

        // Establecer color inicial
        if (activoCheckbox.checked) {
            estadoTexto.style.color = 'var(--success-color)';
        } else {
            estadoTexto.style.color = 'var(--text-secondary)';
        }
    }
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
    const tableRows = document.querySelectorAll('.table-modulos tbody tr');
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

/* Contador de Caracteres */
function agregarContadorCaracteres() {
    const nombreInput = document.querySelector('input[name="Input.Nombre"]');

    if (nombreInput) {
        const maxLength = nombreInput.getAttribute('maxlength') || 100;

        // Crear elemento contador
        const contador = document.createElement('small');
        contador.className = 'form-text text-muted text-end d-block';
        contador.style.marginTop = '0.25rem';

        // Insertar después del input
        nombreInput.parentElement.appendChild(contador);

        // Actualizar contador
        const actualizarContador = () => {
            const length = nombreInput.value.length;
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

        nombreInput.addEventListener('input', actualizarContador);
        actualizarContador();
    }
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

/* Animación Shake para Errores */
const style = document.createElement('style');
style.textContent = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25% { transform: translateX(-5px); }
        75% { transform: translateX(5px); }
    }
    
    @keyframes modalSlideIn {
        from {
            opacity: 0;
            transform: translateY(-50px) scale(0.95);
        }
        to {
            opacity: 1;
            transform: translateY(0) scale(1);
        }
    }
    
    @keyframes fadeIn {
        from {
            opacity: 0;
        }
        to {
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);

/* Exportar Funciones Globales */
window.confirmarEliminacion = confirmarEliminacion;
window.limpiarFormulario = limpiarFormulario;