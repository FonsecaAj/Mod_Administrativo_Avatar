// dashboard.js - Optimizado para carga rápida
(function () {
    'use strict';

    let cargaInicial = true;

    document.addEventListener('DOMContentLoaded', function () {
        cargarActividades();
    });

    async function cargarActividades() {
        const loadingDiv = document.getElementById('loading-actividades');
        const containerDiv = document.getElementById('actividades-container');
        const emptyDiv = document.getElementById('empty-actividades');
        const errorDiv = document.getElementById('error-actividades');

        // Solo mostrar loading en la primera carga
        if (cargaInicial && loadingDiv) {
            loadingDiv.style.display = 'block';
        }
        if (containerDiv) containerDiv.style.display = 'none';
        if (emptyDiv) emptyDiv.style.display = 'none';
        if (errorDiv) errorDiv.style.display = 'none';

        try {
            // Timeout de 5 segundos para evitar esperas largas
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 5000);

            const response = await fetch('/api/dashboard/actividades', {
                signal: controller.signal,
                headers: { 'Content-Type': 'application/json' }
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const data = await response.json();
            const notificaciones = data.notificaciones || [];
            const bitacoras = data.bitacoras || [];

            // Actualizar contadores
            const countNotif = document.getElementById('count-notif');
            const countBitacora = document.getElementById('count-bitacora');
            if (countNotif) countNotif.textContent = notificaciones.length;
            if (countBitacora) countBitacora.textContent = bitacoras.length;

            if (notificaciones.length === 0 && bitacoras.length === 0) {
                if (loadingDiv) loadingDiv.style.display = 'none';
                if (emptyDiv) emptyDiv.style.display = 'block';
                return;
            }

            // Renderizar usando fragments para mejor performance
            renderizarNotificaciones(notificaciones);
            renderizarBitacoras(bitacoras);

            if (loadingDiv) loadingDiv.style.display = 'none';
            if (containerDiv) containerDiv.style.display = 'block';
            cargaInicial = false;

        } catch (error) {
            console.error('Error al cargar actividades:', error);
            if (loadingDiv) loadingDiv.style.display = 'none';
            if (errorDiv) errorDiv.style.display = 'block';
        }
    }

    function renderizarNotificaciones(notificaciones) {
        const container = document.getElementById('lista-notificaciones');
        if (!container) return;

        if (notificaciones.length === 0) {
            container.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="bi bi-inbox fs-3 d-block mb-2"></i>
                    <p class="mb-0">No hay notificaciones recientes</p>
                </div>
            `;
            return;
        }

        // Usar fragment para mejor performance
        const fragment = document.createDocumentFragment();

        notificaciones.forEach(notif => {
            const fecha = formatearFecha(notif.fecha_Registro || notif.fechaRegistro);
            let destinatario = 'N/A';
            let asunto = 'Sin asunto';

            try {
                const desc = JSON.parse(notif.descripcion);
                destinatario = desc.Destinatario || desc.destinatario || 'N/A';
                asunto = desc.Asunto || desc.asunto || 'Sin asunto';
            } catch { }

            const card = document.createElement('div');
            card.className = 'card mb-2';
            card.innerHTML = `
                <div class="card-body py-2">
                    <div class="d-flex align-items-start">
                        <div class="me-3">
                            <i class="bi bi-envelope fs-4 text-success"></i>
                        </div>
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-start mb-1">
                                <strong class="text-dark">${escapeHtml(asunto)}</strong>
                                <small class="text-muted">${fecha}</small>
                            </div>
                            <p class="mb-1 text-muted small">
                                <i class="bi bi-person me-1"></i>Para: ${escapeHtml(destinatario)}
                            </p>
                            <small class="text-muted">
                                <i class="bi bi-person-circle me-1"></i>Enviado por: ${escapeHtml(notif.usuario)}
                            </small>
                        </div>
                    </div>
                </div>
            `;
            fragment.appendChild(card);
        });

        container.innerHTML = '';
        container.appendChild(fragment);
    }

    function renderizarBitacoras(bitacoras) {
        const tbody = document.getElementById('tabla-bitacoras');
        if (!tbody) return;

        if (bitacoras.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="4" class="text-center py-4 text-muted">
                        <i class="bi bi-inbox fs-3 d-block mb-2"></i>
                        <p class="mb-0">No hay registros de bitácora</p>
                    </td>
                </tr>
            `;
            return;
        }

        // Usar fragment para mejor performance
        const fragment = document.createDocumentFragment();

        bitacoras.forEach(bit => {
            const fecha = formatearFecha(bit.fecha_Registro || bit.fechaRegistro);
            const tipoAccion = bit.tipo_Accion || bit.tipoAccion || 'N/A';
            const iconoAccion = obtenerIconoAccion(tipoAccion);
            const colorAccion = obtenerColorAccion(tipoAccion);

            let descripcionTexto = bit.descripcion || '';
            try {
                const desc = JSON.parse(descripcionTexto);
                if (desc.accion) {
                    descripcionTexto = desc.accion;
                }
            } catch { }

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="text-muted small">${fecha}</td>
                <td>
                    <span class="badge bg-light text-dark">
                        <i class="bi bi-person me-1"></i>${escapeHtml(bit.usuario)}
                    </span>
                </td>
                <td>
                    <span class="badge bg-${colorAccion} bg-opacity-10 text-${colorAccion}">
                        <i class="bi ${iconoAccion} me-1"></i>${escapeHtml(tipoAccion)}
                    </span>
                </td>
                <td class="small">${escapeHtml(truncarTexto(descripcionTexto, 80))}</td>
            `;
            fragment.appendChild(tr);
        });

        tbody.innerHTML = '';
        tbody.appendChild(fragment);
    }

    // Cache de fechas para evitar recalcular
    const fechaCache = new Map();

    function formatearFecha(fecha) {
        const key = fecha.toString();
        if (fechaCache.has(key)) {
            return fechaCache.get(key);
        }

        const ahora = new Date();
        const fechaReg = new Date(fecha);
        const diff = ahora - fechaReg;
        const minutos = Math.floor(diff / 60000);
        const horas = Math.floor(diff / 3600000);
        const dias = Math.floor(diff / 86400000);

        let resultado;
        if (minutos < 1) resultado = 'Hace un momento';
        else if (minutos < 60) resultado = `Hace ${minutos} min`;
        else if (horas < 24) resultado = `Hace ${horas} h`;
        else if (dias < 7) resultado = `Hace ${dias} días`;
        else {
            resultado = fechaReg.toLocaleDateString('es-ES', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }

        fechaCache.set(key, resultado);
        return resultado;
    }

    function obtenerIconoAccion(tipo) {
        const iconos = {
            'INSERT': 'bi-plus-circle',
            'UPDATE': 'bi-pencil-square',
            'DELETE': 'bi-trash',
            'SELECT': 'bi-eye',
            'GENERICA': 'bi-gear'
        };
        return iconos[tipo] || 'bi-question-circle';
    }

    function obtenerColorAccion(tipo) {
        const colores = {
            'INSERT': 'success',
            'UPDATE': 'warning',
            'DELETE': 'danger',
            'SELECT': 'info',
            'GENERICA': 'secondary'
        };
        return colores[tipo] || 'secondary';
    }

    function truncarTexto(texto, maxLength) {
        if (!texto) return '';
        if (texto.length <= maxLength) return texto;
        return texto.substring(0, maxLength) + '...';
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text || '';
        return div.innerHTML;
    }

    // Exponer globalmente para botón reintentar
    window.cargarActividades = cargarActividades;

})();