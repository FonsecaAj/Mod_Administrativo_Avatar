// Inicialización cuando el DOM está listo
document.addEventListener('DOMContentLoaded', function () {
    inicializarParametros();
});

/* Función Principal de Inicialización */
function inicializarParametros() {
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

    // Formato automático para ID
    inicializarFormatoId();

    // Contador de caracteres para valor
    inicializarContadorValor();
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
    const idInput = document.querySelector('input[name="Input.IdParametro"]');
    const valorInput = document.querySelector('textarea[name="Input.Valor"]');

    if (idInput && !idInput.readOnly) {
        // Validación del ID
        idInput.addEventListener('input', function () {
            validarIdParametro(this);
        });

        idInput.addEventListener('blur', function () {
            validarIdParametro(this);
        });
    }

    if (valorInput) {
        // Validación del valor
        valorInput.addEventListener('input', function () {
            validarValor(this);
        });

        valorInput.addEventListener('blur', function () {
            validarValor(this);
        });
    }

    // Feedback visual mejorado
    [idInput, valorInput].forEach(input => {
        if (input && !input.readOnly) {
            input.addEventListener('focus', function () {
                this.parentElement.style.transform = 'scale(1.01)';
                this.parentElement.style.transition = 'transform 0.2s ease';
            });

            input.addEventListener('blur', function () {
                this.parentElement.style.transform = 'scale(1)';
            });
        }
    });
}

/* Validar ID de Parámetro */
function validarIdParametro(input) {
    const valor = input.value.trim();
    const regex = /^[A-Z_]*$/;

    // Remover clases previas
    input.classList.remove('is-invalid', 'is-valid');

    // Validar que no esté vacío
    if (valor.length === 0) {
        input.classList.remove('is-valid');
        ocultarError(input);
        return false;
    }

    // Validar formato (solo mayúsculas y guiones bajos)
    if (!regex.test(valor)) {
        input.classList.add('is-invalid');
        mostrarError(input, 'Solo se permiten letras mayúsculas y guiones bajos');
        return false;
    }

    // Validar longitud mínima
    if (valor.length < 2) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El ID debe tener al menos 2 caracteres');
        return false;
    }

    // Validar longitud máxima
    if (valor.length > 10) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El ID no puede exceder 10 caracteres');
        return false;
    }

    // Validar que no comience o termine con guion bajo
    if (valor.startsWith('_') || valor.endsWith('_')) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El ID no puede comenzar ni terminar con guion bajo');
        return false;
    }

    // Validar guiones bajos consecutivos
    if (/__/.test(valor)) {
        input.classList.add('is-invalid');
        mostrarError(input, 'No se permiten guiones bajos consecutivos');
        return false;
    }

    // Validación exitosa
    input.classList.add('is-valid');
    ocultarError(input);
    return true;
}

/* Validar Valor */
function validarValor(input) {
    const valor = input.value.trim();

    // Remover clases previas
    input.classList.remove('is-invalid', 'is-valid');

    // Validar que no esté vacío
    if (valor.length === 0) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El valor es obligatorio');
        return false;
    }

    // Validar longitud máxima
    if (valor.length > 500) {
        input.classList.add('is-invalid');
        mostrarError(input, 'El valor no puede exceder 500 caracteres');
        return false;
    }

    // Validación exitosa
    if (valor.length > 0) {
        input.classList.add('is-valid');
        ocultarError(input);
        return true;
    }

    return true;
}

/* Mostrar/Ocultar Errores de Validación */
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

/* Formato Automático para ID */
function inicializarFormatoId() {
    const idInput = document.querySelector('input[name="Input.IdParametro"]');

    if (idInput && !idInput.readOnly) {
        idInput.addEventListener('input', function (e) {
            // Convertir a mayúsculas automáticamente
            let valor = this.value.toUpperCase();

            // Remover caracteres no permitidos
            valor = valor.replace(/[^A-Z_]/g, '');

            // Actualizar el valor
            this.value = valor;
        });

        // Prevenir pegar texto no válido
        idInput.addEventListener('paste', function (e) {
            e.preventDefault();
            const texto = (e.clipboardData || window.clipboardData).getData('text');
            const textoLimpio = texto.toUpperCase().replace(/[^A-Z_]/g, '');
            document.execCommand('insertText', false, textoLimpio);
        });
    }
}

/* Contador de Caracteres para Valor */
function inicializarContadorValor() {
    const valorInput = document.querySelector('textarea[name="Input.Valor"]');

    if (valorInput) {
        const maxLength = 500;

        // Crear elemento contador
        const contador = document.createElement('small');
        contador.className = 'form-text text-muted text-end d-block';
        contador.style.marginTop = '0.25rem';

        // Insertar después del form-text existente
        const formText = valorInput.parentElement.querySelector('.form-text');
        if (formText) {
            formText.insertAdjacentElement('afterend', contador);
        } else {
            valorInput.insertAdjacentElement('afterend', contador);
        }

        // Actualizar contador
        const actualizarContador = () => {
            const length = valorInput.value.length;
            const restante = maxLength - length;

            contador.innerHTML = `<i class="bi bi-text-paragraph me-1"></i>${length}/${maxLength} caracteres`;

            if (restante < 50) {
                contador.style.color = 'var(--danger-color)';
                contador.style.fontWeight = '600';
            } else if (restante < 100) {
                contador.style.color = 'var(--warning-color)';
                contador.style.fontWeight = '500';
            } else {
                contador.style.color = 'var(--text-secondary)';
                contador.style.fontWeight = '400';
            }
        };

        valorInput.addEventListener('input', actualizarContador);
        actualizarContador();
    }
}

/* Confirmación de Eliminación */
function confirmarEliminacion(id) {
    // Establecer los valores en el modal
    document.getElementById('idEliminar').textContent = id;
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

/* Animaciones de Entrada */
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
    const tableRows = document.querySelectorAll('.table-parametros tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.01)';
            this.style.transition = 'all 0.2s ease';
        });

        row.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    });

    // Animar botones de paginación
    const paginationLinks = document.querySelectorAll('.pagination .page-link');
    paginationLinks.forEach(link => {
        link.addEventListener('mouseenter', function () {
            if (!this.parentElement.classList.contains('disabled')) {
                this.style.transform = 'translateY(-2px)';
            }
        });

        link.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
}

/* Tooltips de Bootstrap */
function inicializarTooltips() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl =>
        new bootstrap.Tooltip(tooltipTriggerEl, {
            trigger: 'hover',
            delay: { show: 300, hide: 100 }
        })
    );

    // Tooltips para valores truncados
    const valoresTruncados = document.querySelectorAll('.valor-truncado');
    valoresTruncados.forEach(elemento => {
        new bootstrap.Tooltip(elemento, {
            trigger: 'hover',
            html: true
        });
    });
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

                // Re-habilitar después de 5 segundos (por si hay error)
                setTimeout(() => {
                    submitBtn.classList.remove('submitting');
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalHTML;
                }, 5000);
            }
        });
    });
}

/* Sugerencias de IDs de Parámetros */
function mostrarSugerencias() {
    const sugerencias = [
        { id: 'JWTEXPMIN', descripcion: 'Tiempo de expiración del JWT en minutos' },
        { id: 'REFEXPMIN', descripcion: 'Tiempo de expiración del refresh token' },
        { id: 'MAX_LOGIN', descripcion: 'Número máximo de intentos de login' },
        { id: 'SMTP_HOST', descripcion: 'Host del servidor SMTP' },
        { id: 'SMTP_PORT', descripcion: 'Puerto del servidor SMTP' },
        { id: 'APP_NAME', descripcion: 'Nombre de la aplicación' }
    ];

    const idInput = document.querySelector('input[name="Input.IdParametro"]');
    if (!idInput || idInput.readOnly) return;

    // Crear lista de sugerencias
    const listaSugerencias = document.createElement('div');
    listaSugerencias.className = 'alert alert-info alert-parametro mt-3';
    listaSugerencias.innerHTML = `
        <i class="bi bi-lightbulb-fill"></i>
        <div>
            <strong>Sugerencias de IDs comunes:</strong>
            <ul class="mb-0 mt-2">
                ${sugerencias.map(s => `
                    <li>
                        <code style="cursor: pointer;" onclick="usarSugerencia('${s.id}')">${s.id}</code>
                        - ${s.descripcion}
                    </li>
                `).join('')}
            </ul>
        </div>
    `;

    // Insertar después del campo ID si no existe
    if (!document.querySelector('.sugerencias-parametros')) {
        listaSugerencias.classList.add('sugerencias-parametros');
        idInput.parentElement.insertAdjacentElement('afterend', listaSugerencias);
    }
}

/* Usar Sugerencia */
function usarSugerencia(id) {
    const idInput = document.querySelector('input[name="Input.IdParametro"]');
    if (idInput && !idInput.readOnly) {
        idInput.value = id;
        idInput.focus();
        validarIdParametro(idInput);

        // Scroll suave al input
        idInput.scrollIntoView({ behavior: 'smooth', block: 'center' });

        // Highlight temporal
        idInput.style.transition = 'background 0.3s ease';
        idInput.style.background = 'rgba(195, 163, 142, 0.2)';
        setTimeout(() => {
            idInput.style.background = '';
        }, 1000);
    }
}

/* Animación Shake y otras animaciones */
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
`;
document.head.appendChild(style);

/* Exportar Funciones Globales */
window.confirmarEliminacion = confirmarEliminacion;
window.usarSugerencia = usarSugerencia;
window.mostrarSugerencias = mostrarSugerencias;