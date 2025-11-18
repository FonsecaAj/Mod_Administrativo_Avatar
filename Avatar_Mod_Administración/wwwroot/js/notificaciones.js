(function () {
    'use strict';

    console.log('Sistema de notificaciones');

    const TIPOS = {
        success: {
            icono: 'bi-check-circle-fill',
            clase: 'bg-success',
            titulo: 'Éxito',
            textColor: 'text-white'
        },
        error: {
            icono: 'bi-x-circle-fill',
            clase: 'bg-danger',
            titulo: 'Error',
            textColor: 'text-white'
        },
        warning: {
            icono: 'bi-exclamation-triangle-fill',
            clase: 'bg-warning',
            titulo: 'Advertencia',
            textColor: 'text-dark'
        },
        info: {
            icono: 'bi-info-circle-fill',
            clase: 'bg-info',
            titulo: 'Información',
            textColor: 'text-white'
        }
    };

    // Configuración por defecto
    const CONFIG_DEFAULT = {
        duracion: 5000,
        autoCerrar: true,
        mostrarTitulo: true
    };

    // Contador para IDs únicos
    let contador = 0;


    /**
     * @param {string} tipo - 'success', 'error', 'warning', 'info'
     * @param {string} mensaje - Mensaje a mostrar
     * @param {object} opciones - Opciones adicionales (opcional)
     * @returns {string} ID del toast creado
     */

    function mostrarNotificacion(tipo, mensaje, opciones = {}) {

        if (!tipo || !TIPOS[tipo]) {
            console.error(`Tipo inválido: "${tipo}". Tipos válidos: success, error, warning, info`);
            return null;
        }

        if (!mensaje || typeof mensaje !== 'string' || mensaje.trim() === '') {
            console.error('El mensaje es requerido y debe ser un string no vacío');
            return null;
        }

        // Verificar que Bootstrap esté disponible
        if (typeof bootstrap === 'undefined' || !bootstrap.Toast) {
            console.error('Bootstrap no está cargado. Asegúrese de incluir Bootstrap JS antes de notificaciones.js');
            return null;
        }

        const config = { ...CONFIG_DEFAULT, ...opciones };
        const tipoConfig = TIPOS[tipo];
        const toastId = `toast-${tipo}-${++contador}-${Date.now()}`;

        console.log(`Mostrando notificación [${tipo}]: ${mensaje}`);


        let container = document.getElementById('toastContainer');

        if (!container) {
            console.warn('Container #toastContainer no encontrado, creando uno nuevo...');
            container = document.createElement('div');
            container.id = 'toastContainer';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.id = toastId;
        toast.className = `toast align-items-center ${tipoConfig.clase} ${tipoConfig.textColor} border-0`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        // Determinar clase del botón de cierre
        const btnCloseClass = tipo === 'warning' ? 'btn-close' : 'btn-close btn-close-white';

        // HTML del toast
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center">
                    <i class="bi ${tipoConfig.icono} fs-4 me-2"></i>
                    <div class="flex-grow-1">
                        ${config.mostrarTitulo ? `<strong class="d-block">${tipoConfig.titulo}</strong>` : ''}
                        <span>${mensaje}</span>
                    </div>
                </div>
                <button type="button" class="${btnCloseClass} me-2 m-auto" 
                        data-bs-dismiss="toast" 
                        aria-label="Cerrar"></button>
            </div>
        `;


        container.appendChild(toast);

        // el toast debe estar en el DOM antes de inicializar
        const bsToast = new bootstrap.Toast(toast, {
            autohide: config.autoCerrar,
            delay: config.duracion
        });


        bsToast.show();

        toast.addEventListener('hidden.bs.toast', function () {
            console.log(`🗑️ Eliminando toast: ${toastId}`);
            this.remove();
        });

        return toastId;
    }

    function notificacionExito(mensaje, opciones) {
        return mostrarNotificacion('success', mensaje, opciones);
    }

    function notificacionError(mensaje, opciones) {
        return mostrarNotificacion('error', mensaje, opciones);
    }

    function notificacionAdvertencia(mensaje, opciones) {
        return mostrarNotificacion('warning', mensaje, opciones);
    }

    function notificacionInfo(mensaje, opciones) {
        return mostrarNotificacion('info', mensaje, opciones);
    }

    function cerrarTodasLasNotificaciones() {
        const toasts = document.querySelectorAll('.toast.show');

        if (toasts.length === 0) {
            console.log('No hay notificaciones para cerrar');
            return;
        }

        console.log(`Cerrando ${toasts.length} notificaciones...`);

        toasts.forEach(toast => {
            const bsToast = bootstrap.Toast.getInstance(toast);
            if (bsToast) {
                bsToast.hide();
            }
        });
    }

    function convertirTempDataAToasts() {
        console.log('Buscando TempData para convertir a toasts...');

        // Mapeo de clases de alerta a tipos de notificación
        const mapeoAlertas = [
            { clase: '.alert-success', tipo: 'success' },
            { clase: '.alert-danger', tipo: 'error' },
            { clase: '.alert-warning', tipo: 'warning' },
            { clase: '.alert-info', tipo: 'info' }
        ];

        let convertidas = 0;

        mapeoAlertas.forEach(({ clase, tipo }) => {
            // Buscar alertas que tengan el botón de cerrar (son de TempData)
            const alertas = document.querySelectorAll(`${clase}.alert-dismissible`);

            alertas.forEach(alerta => {
                // Extraer el texto del mensaje (sin botones ni íconos)
                let mensaje = alerta.textContent || alerta.innerText;

                // Limpiar el mensaje de espacios en blanco excesivos
                mensaje = mensaje.trim().replace(/\s+/g, ' ');

                if (mensaje) {
                    console.log(`Convirtiendo alerta ${tipo}: "${mensaje}"`);
                    mostrarNotificacion(tipo, mensaje);
                    alerta.remove();
                    convertidas++;
                }
            });
        });

        if (convertidas > 0) {
            console.log(`${convertidas} alertas convertidas a toasts`);
        } else {
            console.log('No se encontraron alertas TempData para convertir');
        }
    }

    
    function inicializar() {
        console.log('Inicializando sistema de notificaciones...');

        // Verificar que Bootstrap esté disponible
        if (typeof bootstrap === 'undefined') {
            console.error('Bootstrap no detectado. El sistema de notificaciones no funcionará correctamente.');
            return;
        }

        console.log('Bootstrap detectado correctamente');

        // Convertir TempData automáticamente
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                console.log('DOM cargado, procesando TempData...');
                convertirTempDataAToasts();
            });
        } else {
            console.log('DOM ya estaba cargado, procesando TempData...');
            convertirTempDataAToasts();
        }

        console.log('Sistema de notificaciones inicializado correctamente');
    }

    // funcions globales
    window.mostrarNotificacion = mostrarNotificacion;
    window.notificacionExito = notificacionExito;
    window.notificacionError = notificacionError;
    window.notificacionAdvertencia = notificacionAdvertencia;
    window.notificacionInfo = notificacionInfo;
    window.cerrarTodasLasNotificaciones = cerrarTodasLasNotificaciones;
    window.mostrarNotificacionesDePrueba = mostrarNotificacionesDePrueba;


    inicializar();

})();