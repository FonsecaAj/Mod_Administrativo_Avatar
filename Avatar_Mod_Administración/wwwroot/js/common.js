(function () {
    'use strict';

    window.playSuccessSound = function () {
        try {
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            // Sonido agradable de éxito
            oscillator.frequency.value = 800;
            oscillator.type = 'sine';

            // Fade out suave
            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.15);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.15);
        } catch (e) {
            // Navegador no soporta Web Audio API o usuario bloqueó sonido
            console.log('Sonido no disponible');
        }
    };

    /**
     * Reproduce un sonido de error
     */
    window.playErrorSound = function () {
        try {
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            // Sonido grave para error (200 Hz)
            oscillator.frequency.value = 200;
            oscillator.type = 'sawtooth';

            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.2);
        } catch (e) {
            console.log('Sonido no disponible');
        }
    };

    window.playWarningSound = function () {
        try {
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            // Sonido medio para advertencia (500 Hz)
            oscillator.frequency.value = 500;
            oscillator.type = 'square';

            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.15);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.15);
        } catch (e) {
            console.log('Sonido no disponible');
        }
    };

    window.formatearFecha = function (fecha) {
        if (!fecha) return 'N/A';

        const date = typeof fecha === 'string' ? new Date(fecha) : fecha;

        const dia = String(date.getDate()).padStart(2, '0');
        const mes = String(date.getMonth() + 1).padStart(2, '0');
        const anio = date.getFullYear();
        const horas = String(date.getHours()).padStart(2, '0');
        const minutos = String(date.getMinutes()).padStart(2, '0');

        return `${dia}/${mes}/${anio} ${horas}:${minutos}`;
    };

    window.mostrarToast = function (mensaje, tipo = 'info') {

        // Implementacion basica
        const iconos = {
            'success': 'bi-check-circle-fill',
            'error': 'bi-exclamation-triangle-fill',
            'warning': 'bi-exclamation-circle-fill',
            'info': 'bi-info-circle-fill'
        };

        const colores = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        };

        const icono = iconos[tipo] || iconos['info'];
        const color = colores[tipo] || colores['info'];

        // Crear el toast
        const toast = document.createElement('div');
        toast.className = `alert ${color} alert-dismissible fade show position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            <i class="bi ${icono} me-2"></i>
            ${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(toast);

        // Auto-remover después de 5 segundos
        setTimeout(() => {
            toast.remove();
        }, 5000);
    };


    window.confirmarAccion = function (titulo, mensaje, callback) {
        // Esta es una versión básica
        // Si ya tienes modales de confirmación en cada página, usalos
        if (confirm(`${titulo}\n\n${mensaje}`)) {
            callback();
        }
    };

    window.validarEmail = function (email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    };


    window.escaparHTML = function (texto) {
        const div = document.createElement('div');
        div.textContent = texto;
        return div.innerHTML;
    };

    window.inicializarTooltips = function () {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    };


    // Inicializar tooltips cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', inicializarTooltips);
    } else {
        inicializarTooltips();
    }

    console.log('Funciones comunes cargadas');

})();