(function () {
    'use strict';

    // Variables globales del dashboard
    const Dashboard = {
        animationDuration: 1000,
        refreshInterval: 300000, // 5 minutos
        charts: {},
        timers: {}
    };

    

    /**
     * Inicialización del dashboard
     */
    function init() {
        console.log('Inicializando Dashboard...');

        // Esperar a que el DOM esté listo
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', onDOMReady);
        } else {
            onDOMReady();
        }
    }

    /**
     * Función ejecutada cuando el DOM está listo
     */
    function onDOMReady() {
        console.log('Dashboard: DOM Ready');

        // Registrar visualización en bitácora
        registrarVisualizacion();

        // Mostrar notificación de bienvenida
        mostrarBienvenida();

        // Animar las tarjetas de estadísticas
        animateStatCards();

        // Agregar listeners a los elementos
        attachEventListeners();

        // Iniciar actualización automática (opcional)
        // startAutoRefresh();
    }

    /**
     * Registra la visualización del dashboard en la bitácora
     */
    function registrarVisualizacion() {
        try {
            if (typeof window.registrarBitacora === 'function' && window.sessionData) {
                const email = window.sessionData.userId || 'Desconocido';
                const nombre = window.sessionData.userName || 'Usuario';

                window.registrarBitacora(
                    'Visualización Dashboard',
                    `Usuario ${nombre} (${email}) visualizó el dashboard`
                );

                console.log('Dashboard: Visualización registrada en bitácora');
            } else {
                console.warn('Dashboard: Función registrarBitacora no disponible');
            }
        } catch (error) {
            console.error('Error al registrar visualización:', error);
        }
    }

    /**
     * Muestra notificación de bienvenida
     */
    function mostrarBienvenida() {
        try {
            if (typeof window.mostrarNotificacion === 'function') {
                setTimeout(() => {
                    window.mostrarNotificacion(
                        'success',
                        '¡Bienvenido al Sistema Avatar!'
                    );
                }, 500);

                console.log('Dashboard: Notificación de bienvenida mostrada');
            } else {
                console.warn('Dashboard: Función mostrarNotificacion no disponible');
            }
        } catch (error) {
            console.error('Error al mostrar notificación:', error);
        }
    }

    /**
     * Anima los números en las tarjetas de estadísticas
     */
    function animateStatCards() {
        const statNumbers = document.querySelectorAll('.stat-number');

        if (statNumbers.length === 0) {
            console.warn('Dashboard: No se encontraron elementos .stat-number');
            return;
        }

        console.log(`Dashboard: Animando ${statNumbers.length} tarjetas de estadísticas`);

        statNumbers.forEach((element, index) => {
            // Obtener el valor final
            const finalValue = parseInt(element.textContent) || 0;

            // Guardar el valor original por si se necesita después
            element.dataset.originalValue = finalValue;

            // Animar con delay escalonado
            setTimeout(() => {
                animateNumber(element, 0, finalValue, Dashboard.animationDuration);
            }, index * 100);
        });
    }

    /**
     * Anima un número desde start hasta end
     * @param {HTMLElement} element - Elemento a animar
     * @param {number} start - Valor inicial
     * @param {number} end - Valor final
     * @param {number} duration - Duración en ms
     */
    function animateNumber(element, start, end, duration) {
        let startTimestamp = null;

        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;

            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            const currentValue = Math.floor(progress * (end - start) + start);

            element.textContent = currentValue;

            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else {
                element.textContent = end; // Asegurar valor final exacto
            }
        };

        window.requestAnimationFrame(step);
    }

    /**
     * Agrega event listeners a los elementos del dashboard
     */
    function attachEventListeners() {
        // Listener para botones de acceso rápido
        const quickAccessButtons = document.querySelectorAll('.quick-access-btn');
        quickAccessButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                console.log(`Dashboard: Navegando a ${href}`);

                // Registrar en bitácora
                if (typeof window.registrarBitacora === 'function') {
                    const buttonText = this.querySelector('.fw-bold')?.textContent || 'Desconocido';
                    window.registrarBitacora(
                        'Acceso Rápido',
                        `Usuario accedió a: ${buttonText}`
                    );
                }
            });
        });

        // Listener para cards de estadísticas con enlaces
        const statCardLinks = document.querySelectorAll('.stat-card a');
        statCardLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                console.log(`Dashboard: Accediendo a ${href} desde card`);

                // Registrar en bitácora
                if (typeof window.registrarBitacora === 'function') {
                    const cardTitle = this.closest('.card').querySelector('h6')?.textContent || 'Desconocido';
                    window.registrarBitacora(
                        'Navegación',
                        `Usuario accedió a ${cardTitle}`
                    );
                }
            });
        });

        console.log('Dashboard: Event listeners agregados');
    }

    /**
     * Inicia la actualización automática de estadísticas
     */
    function startAutoRefresh() {
        console.log(`Dashboard: Iniciando auto-refresh cada ${Dashboard.refreshInterval / 1000} segundos`);

        Dashboard.timers.refresh = setInterval(() => {
            refreshStatistics();
        }, Dashboard.refreshInterval);
    }

    /**
     * Detiene la actualización automática
     */
    function stopAutoRefresh() {
        if (Dashboard.timers.refresh) {
            clearInterval(Dashboard.timers.refresh);
            Dashboard.timers.refresh = null;
            console.log('Dashboard: Auto-refresh detenido');
        }
    }

    /**
     * Refresca las estadísticas del dashboard
     */
    async function refreshStatistics() {
        console.log('Dashboard: Refrescando estadísticas...');

        try {
            // Mostrar indicador de carga
            if (typeof window.mostrarCargando === 'function') {
                window.mostrarCargando();
            }

            // Realizar petición al servidor
            const response = await fetch('/Index?handler=Statistics', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            // Actualizar las estadísticas en la UI
            updateStatisticsUI(data);

            console.log('Dashboard: Estadísticas actualizadas', data);

        } catch (error) {
            console.error('Error al refrescar estadísticas:', error);

            if (typeof window.mostrarNotificacion === 'function') {
                window.mostrarNotificacion(
                    'error',
                    'Error al actualizar estadísticas'
                );
            }
        } finally {
            // Ocultar indicador de carga
            if (typeof window.ocultarCargando === 'function') {
                window.ocultarCargando();
            }
        }
    }

    /**
     * Actualiza la UI con las nuevas estadísticas
     * @param {Object} data - Datos de estadísticas
     */
    function updateStatisticsUI(data) {
        if (!data) return;

        // Actualizar cada estadística con animación
        const stats = [
            { selector: '.stat-number', key: 'totalUsuarios', index: 0 },
            { selector: '.stat-number', key: 'totalInstituciones', index: 1 },
            { selector: '.stat-number', key: 'totalCarreras', index: 2 },
            { selector: '.stat-number', key: 'totalCursos', index: 3 }
        ];

        const statElements = document.querySelectorAll('.stat-number');

        stats.forEach((stat, i) => {
            if (statElements[i] && data[stat.key] !== undefined) {
                const currentValue = parseInt(statElements[i].textContent) || 0;
                const newValue = data[stat.key];

                if (currentValue !== newValue) {
                    animateNumber(statElements[i], currentValue, newValue, 800);
                }
            }
        });
    }

    /**
     * Obtiene el token anti-forgery
     * @returns {string} Token anti-forgery
     */
    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    /**
     * Exporta datos del dashboard a CSV
     */
    function exportToCSV() {
        console.log('Dashboard: Exportando a CSV...');

        try {
            const stats = [];
            const statCards = document.querySelectorAll('.stat-card');

            statCards.forEach(card => {
                const title = card.querySelector('h6')?.textContent.trim() || '';
                const value = card.querySelector('.stat-number')?.textContent.trim() || '0';
                stats.push({ title, value });
            });

            const csvContent = [
                ['Estadística', 'Valor'],
                ...stats.map(s => [s.title, s.value])
            ].map(row => row.join(',')).join('\n');

            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            const url = URL.createObjectURL(blob);

            link.setAttribute('href', url);
            link.setAttribute('download', `dashboard_${new Date().toISOString().split('T')[0]}.csv`);
            link.style.visibility = 'hidden';

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            console.log('Dashboard: Exportación completada');

            if (typeof window.mostrarNotificacion === 'function') {
                window.mostrarNotificacion('success', 'Datos exportados correctamente');
            }

        } catch (error) {
            console.error('Error al exportar:', error);

            if (typeof window.mostrarNotificacion === 'function') {
                window.mostrarNotificacion('error', 'Error al exportar datos');
            }
        }
    }

    /**
     * Imprime el dashboard
     */
    function printDashboard() {
        console.log('Dashboard: Preparando para imprimir...');

        // Registrar en bitácora
        if (typeof window.registrarBitacora === 'function') {
            window.registrarBitacora(
                'Impresión Dashboard',
                'Usuario imprimió el dashboard'
            );
        }

        window.print();
    }

    /**
     * Limpieza al salir de la página
     */
    function cleanup() {
        console.log('Dashboard: Limpiando recursos...');
        stopAutoRefresh();

        // Limpiar cualquier otro recurso
        Object.keys(Dashboard.charts).forEach(key => {
            if (Dashboard.charts[key] && typeof Dashboard.charts[key].destroy === 'function') {
                Dashboard.charts[key].destroy();
            }
        });
    }

    // Event listener para limpieza antes de salir
    window.addEventListener('beforeunload', cleanup);

    // Exponer funciones públicas
    window.Dashboard = {
        init,
        refresh: refreshStatistics,
        export: exportToCSV,
        print: printDashboard,
        startAutoRefresh,
        stopAutoRefresh
    };

    // Auto-inicializar
    init();

})();