(function () {
    'use strict';

    // Variables globales del dashboard
    const Dashboard = {
        animationDuration: 800, // Reducido de 1000ms
        refreshInterval: 300000, // 5 minutos
        charts: {},
        timers: {}
    };

   
    function init() {
        console.log('Inicializando Dashboard...');

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
    }


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
            }
        } catch (error) {
            console.error('Error al registrar visualización:', error);
        }
    }


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
            }
        } catch (error) {
            console.error('Error al mostrar notificación:', error);
        }
    }


    function animateStatCards() {
        const statNumbers = document.querySelectorAll('.stat-number');

        if (statNumbers.length === 0) {
            console.warn('Dashboard: No se encontraron elementos .stat-number');
            return;
        }

        console.log(`Dashboard: Animando ${statNumbers.length} tarjetas de estadísticas`);

        statNumbers.forEach((element, index) => {
            const finalValue = parseInt(element.textContent) || 0;
            element.dataset.originalValue = finalValue;

            // Usar requestAnimationFrame directamente sin setTimeout
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

            // OPTIMIZACIÓN: Usar función de easing suave
            const easeProgress = progress < 0.5
                ? 2 * progress * progress
                : -1 + (4 - 2 * progress) * progress;

            const currentValue = Math.floor(easeProgress * (end - start) + start);

            element.textContent = currentValue;

            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else {
                element.textContent = end;
            }
        };

        window.requestAnimationFrame(step);
    }


    function attachEventListeners() {
        // Usar event delegation
        document.addEventListener('click', function (e) {
            const quickAccessBtn = e.target.closest('.quick-access-btn');
            const statCardLink = e.target.closest('.stat-card a');

            if (quickAccessBtn) {
                handleQuickAccessClick(quickAccessBtn);
            } else if (statCardLink) {
                handleStatCardClick(statCardLink);
            }
        });

        console.log('Dashboard: Event listeners agregados (delegation)');
    }

    /**
     * Maneja click en botones de acceso rápido
     */
    function handleQuickAccessClick(button) {
        const href = button.getAttribute('href');
        console.log(`Dashboard: Navegando a ${href}`);

        if (typeof window.registrarBitacora === 'function') {
            const buttonText = button.querySelector('.fw-bold')?.textContent || 'Desconocido';
            window.registrarBitacora(
                'Acceso Rápido',
                `Usuario accedió a: ${buttonText}`
            );
        }
    }

    /**
     * Maneja click en enlaces de cards de estadísticas
     */
    function handleStatCardClick(link) {
        const href = link.getAttribute('href');
        console.log(`Dashboard: Accediendo a ${href} desde card`);

        if (typeof window.registrarBitacora === 'function') {
            const cardTitle = link.closest('.card').querySelector('h6')?.textContent || 'Desconocido';
            window.registrarBitacora(
                'Navegación',
                `Usuario accedió a ${cardTitle}`
            );
        }
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
            if (typeof window.mostrarCargando === 'function') {
                window.mostrarCargando();
            }

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

        const stats = [
            { key: 'totalUsuarios', index: 0 },
            { key: 'totalInstituciones', index: 1 },
            { key: 'totalCarreras', index: 2 },
            { key: 'totalCursos', index: 3 }
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

            // Liberar el objeto URL
            URL.revokeObjectURL(url);

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

        // Limpiar charts si existen
        for (const key in Dashboard.charts) {
            if (Dashboard.charts[key] && typeof Dashboard.charts[key].destroy === 'function') {
                Dashboard.charts[key].destroy();
            }
        }
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