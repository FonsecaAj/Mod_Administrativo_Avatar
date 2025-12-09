(function () {
    'use strict';

    console.log('menu-dinamico.js CARGADO');

    const MODULOS_CONFIG = {
        'Usuarios': { icono: 'bi-people', url: '/Usuario/Usuarios', grupo: 'Administración', prefijo: '/Usuario' },
        'Roles': { icono: 'bi-shield-check', url: '/Rol/Roles', grupo: 'Administración', prefijo: '/Rol' },
        'Parámetros': { icono: 'bi-gear', url: '/Parametro/Parametros', grupo: 'Administración', prefijo: '/Parametro' },
        'Permisos': { icono: 'bi-journal-text', url: '/Permiso/Permisos', grupo: 'Administración', prefijo: '/Permiso' },
        'Módulos': { icono: 'bi-puzzle', url: '/Modulo/Modulos', grupo: 'Administración', prefijo: '/Modulo' },
        'Bitácoras': { icono: 'bi-journal-text', url: '/api/Bitacora', grupo: 'Administración', prefijo: '/api/Bitacora' },
        'Instituciones': { icono: 'bi-building', url: '/Institucion/Instituciones', grupo: 'Académico', prefijo: '/Institucion' },
        'Carreras': { icono: 'bi-mortarboard', url: '/Carrera/Carreras', grupo: 'Académico', prefijo: '/Carrera' },
        'Cursos': { icono: 'bi-book', url: '/ADM10_Cursos', grupo: 'Académico', prefijo: '/Curso' },
        'Grupos': { icono: 'bi-diagram-3', url: '/ADM13_Grupo', grupo: 'Académico', prefijo: '/Grupo' },
        'Prematrícula': { icono: 'bi-clipboard-check', url: '/ADM14_Prematricula/Index', grupo: 'Matrícula', prefijo: '/Prematricula' },
        'Matrícula': { icono: 'bi-journal-check', url: '/Matricula/Index', grupo: 'Matrícula', prefijo: '/Matricula' },
        'Mis Cursos': { icono: 'bi-book-half', url: '/MisCursos/Index', grupo: 'Estudiante', prefijo: '/MisCursos' },
        'Desglose y Notas': { icono: 'bi-clipboard-data', url: '/Notas/Index', grupo: 'Evaluación', prefijo: '/Notas' },
        'Promedios': { icono: 'bi-graph-up', url: '/Promedios/Index', grupo: 'Evaluación', prefijo: '/Promedios' },
        'Reportes': { icono: 'bi-file-earmark-bar-graph', url: '/Reportes/Index', grupo: 'Reportes', prefijo: '/Reportes' },
        'Profesores': { icono: 'bi-file-earmark-bar-graph', url: '/ADM11_Profesor', grupo: 'Profesores', prefijo: '/Profesores' },
        'Notificaciones': { icono: 'bi-envelope', url: '/Notificaciones_Correo/Index', grupo: 'Comunicación', prefijo: '/Notificaciones_Correo' }
    };

    const ADMIN_MENU_EXTENSIONS = [
        { texto: 'Home', url: '/Index', icono: 'bi-house', grupo: 'Principal', esDashboard: true, prefijo: '/Index' },
        { texto: 'Historial Académico', url: '/Academico/HistorialAcademico', icono: 'bi-journal-text', grupo: 'Académico', prefijo: '/Academico/HistorialAcademico' },
        { texto: 'Listados por periodo', url: '/Academico/ListadoEstudiantes', icono: 'bi-people', grupo: 'Académico', prefijo: '/Academico/ListadoEstudiantes' },
        { texto: 'Administración de facturas', url: '/Facturacion/Facturas', icono: 'bi-receipt', grupo: 'Facturación', prefijo: '/Facturacion' },
        { texto: 'Consulta de pagos', url: '/Pagos/Pagos', icono: 'bi-cash', grupo: 'Facturación', prefijo: '/Pagos' },
        { texto: 'Prueba Notificaciones', url: '/Notificaciones_Correo/Index', icono: 'bi-cash', grupo: 'Facturación', prefijo: '/Notificaciones_Correo' },
        { texto: 'Cursos', url: '/ADM10_Cursos', icono: 'bi-book', grupo: 'Mantenimiento Adm', prefijo: '/ADM10_Cursos' },
        { texto: 'Profesores', url: '/ADM11_Profesor', icono: 'bi-people', grupo: 'Mantenimiento Adm', prefijo: '/ADM11_Profesor' },
        { texto: 'Periodo', url: '/ADM12_Periodo', icono: 'bi-journal-text', grupo: 'Mantenimiento Adm', prefijo: '/ADM12_Periodo' },
        { texto: 'Grupo', url: '/ADM13_Grupo', icono: 'bi-journal-text', grupo: 'Mantenimiento Adm', prefijo: '/ADM13_Grupo' },
        { texto: 'Prematrícula', url: '/ADM14_Prematricula', icono: 'bi-receipt', grupo: 'Mantenimiento Adm', prefijo: '/ADM14_Prematricula' }
    ];

    let modulosCargados = [];
    let datosUsuarioCache = null;

    function obtenerDatosUsuario() {
        if (datosUsuarioCache) return datosUsuarioCache;
        const appData = document.getElementById('app-data');
        if (!appData) return null;
        datosUsuarioCache = {
            email: appData.dataset.usuarioId || '',
            nombre: appData.dataset.usuarioNombre || 'Usuario',
            rol: appData.dataset.usuarioRol || 'Sin rol',
            rolId: appData.dataset.usuarioRolId || '0',
            token: appData.dataset.accessToken || ''
        };
        return datosUsuarioCache;
    }

    async function verificarSesion() {
        try {
            const response = await fetch('/api/sesion/verificar', { method: 'GET', credentials: 'include' });
            if (!response.ok) { window.location.href = '/Login'; return false; }
            return true;
        } catch (error) { return false; }
    }

    async function cargarModulosPorRol(rolId, token) {
        try {
            const rolIdNum = parseInt(rolId);
            if (!rolIdNum || rolIdNum === 0) return [];
            const response = await fetch(`/api/rol/${rolIdNum}/modulos`, {
                method: 'GET', headers: { 'Content-Type': 'application/json', 'Authorization': token }, credentials: 'include'
            });
            if (!response.ok) { if (response.status === 401) window.location.href = '/Login'; return []; }
            return await response.json();
        } catch (error) { return []; }
    }

    async function cargarMenuDinamico() {
        const loader = document.getElementById('menu-loader');

        try {
            const sesionValida = await verificarSesion();
            if (!sesionValida) return;
            const datosUsuario = obtenerDatosUsuario();
            if (!datosUsuario?.token) { mostrarMenuPorDefecto(); return; }
            const rolIdNum = parseInt(datosUsuario.rolId);
            if (!rolIdNum || rolIdNum === 0) { mostrarMenuPorDefecto(); return; }
            const modulos = await cargarModulosPorRol(datosUsuario.rolId, datosUsuario.token);
            if (!modulos || modulos.length === 0) { mostrarMenuPorDefecto(); return; }
            modulosCargados = modulos;
            if (loader) loader.remove();
            construirMenu(modulos, datosUsuario.rol);
            actualizarBreadcrumbs();
        } catch (error) { mostrarMenuPorDefecto(); }
    }

    function construirMenu(modulos, rolUsuario) {
        const sidebarNav = document.querySelector('.sidebar-nav');
        if (!sidebarNav) return;
        sidebarNav.innerHTML = '';

        let grupos = agruparModulos(modulos);

        agregarDashboard(sidebarNav);

        if (rolUsuario && rolUsuario.toLowerCase() === 'administrador') {
            ADMIN_MENU_EXTENSIONS.forEach(item => {
                if (!item.esDashboard) {
                    const extensionModule = {
                        nombreModulo: item.texto,
                        moduloActivo: true,
                        config: {
                            icono: item.icono,
                            url: item.url,
                            grupo: item.grupo,
                            prefijo: item.prefijo || item.url
                        }
                    };

                    const existingGroup = grupos[item.grupo];

                    if (!existingGroup) {
                        grupos[item.grupo] = [extensionModule];
                    } else {
                        const index = existingGroup.findIndex(m => m.nombreModulo === item.texto);
                        if (index !== -1) {
                            existingGroup[index] = extensionModule;
                        } else {
                            existingGroup.push(extensionModule);
                        }
                    }
                }
            });
        }

        Object.keys(grupos).sort().forEach(nombreGrupo => {
            const modulosGrupo = grupos[nombreGrupo];
            const tituloSeccion = document.createElement('div');
            tituloSeccion.className = 'nav-section-title';
            tituloSeccion.textContent = nombreGrupo;
            sidebarNav.appendChild(tituloSeccion);

            modulosGrupo.forEach(modulo => {
                const config = modulo.config || MODULOS_CONFIG[modulo.nombreModulo];
                if (config && modulo.moduloActivo) {
                    agregarItemMenu(sidebarNav, modulo.nombreModulo, config);
                }
            });
        });
        marcarRutaActiva();
    }

    function agruparModulos(modulos) {
        const grupos = {};
        modulos.forEach(modulo => {
            const config = MODULOS_CONFIG[modulo.nombreModulo];
            if (config && modulo.moduloActivo) {
                if (!grupos[config.grupo]) grupos[config.grupo] = [];
                grupos[config.grupo].push(modulo);
            }
        });
        return grupos;
    }

    function agregarDashboard(container) {
        const navItem = document.createElement('div');
        navItem.className = 'nav-item';
        navItem.innerHTML = '<a href="/Index" class="nav-link"><i class="bi bi-house"></i><span>Home</span></a>';
        container.appendChild(navItem);
    }

    function agregarItemMenu(container, nombre, config) {
        const navItem = document.createElement('div');
        navItem.className = 'nav-item';
        navItem.innerHTML = '<a href="' + config.url + '" class="nav-link"><i class="bi ' + config.icono + '"></i><span>' + nombre + '</span></a>';
        container.appendChild(navItem);
    }

    function marcarRutaActiva() {
        const rutaActual = window.location.pathname;
        const links = document.querySelectorAll('.sidebar-nav .nav-link');
        let mejorCoincidencia = null;
        let longitudCoincidencia = 0;
        links.forEach(function (link) {
            link.classList.remove('active');
            const href = link.getAttribute('href');
            if (rutaActual === href) {
                mejorCoincidencia = link;
                longitudCoincidencia = href.length;
            } else if (href !== '/' && rutaActual.startsWith(href) && href.length > longitudCoincidencia) {
                mejorCoincidencia = link;
                longitudCoincidencia = href.length;
            }
        });
        if (mejorCoincidencia) mejorCoincidencia.classList.add('active');
    }

    function actualizarBreadcrumbs() {
        const breadcrumbOl = document.querySelector('.breadcrumb-container ol');
        if (!breadcrumbOl) return;

        const ruta = window.location.pathname;
        let html = '<li class="breadcrumb-item"><a href="/Index"><i class="bi bi-house-door"></i> Inicio</a></li>';

        if (ruta === '/' || ruta === '/Index') {
            breadcrumbOl.innerHTML = html;
            return;
        }

        let moduloMatch = null;

        for (const nombre in MODULOS_CONFIG) {
            const config = MODULOS_CONFIG[nombre];
            if (ruta.startsWith(config.prefijo + '/')) {
                moduloMatch = { nombre: nombre, url: config.url, grupo: config.grupo };
                break;
            }
        }

        if (!moduloMatch) {
            const extensionMatch = ADMIN_MENU_EXTENSIONS.find(item => {
                const prefix = item.prefijo || item.url;
                return ruta.startsWith(prefix + '/') || ruta === prefix;
            });

            if (extensionMatch) {
                moduloMatch = { nombre: extensionMatch.texto, url: extensionMatch.url, grupo: extensionMatch.grupo, prefijo: extensionMatch.prefijo };
            }
        }

        if (moduloMatch && obtenerDatosUsuario()?.rol.toLowerCase() === 'administrador') {
            const adminOverride = ADMIN_MENU_EXTENSIONS.find(item => item.texto === moduloMatch.nombre);
            if (adminOverride) {
                moduloMatch = {
                    nombre: adminOverride.texto,
                    url: adminOverride.url,
                    grupo: adminOverride.grupo,
                    prefijo: adminOverride.prefijo
                };
            }
        }

        if (!moduloMatch) {
            breadcrumbOl.innerHTML = html;
            return;
        }

        html += '<li class="breadcrumb-item">' + moduloMatch.grupo + '</li>';

        if (ruta === moduloMatch.url || ruta === moduloMatch.url + '/') {
            html += '<li class="breadcrumb-item active" aria-current="page">' + moduloMatch.nombre + '</li>';
        } else {
            html += '<li class="breadcrumb-item"><a href="' + moduloMatch.url + '">' + moduloMatch.nombre + '</a></li>';
            const subpagina = obtenerNombreSubpagina(ruta);
            html += '<li class="breadcrumb-item active" aria-current="page">' + subpagina + '</li>';
        }

        breadcrumbOl.innerHTML = html;
    }

    function obtenerNombreSubpagina(ruta) {
        const r = ruta.toLowerCase();

        if (r.includes('institucioncrear')) return 'Crear Institución';
        if (r.includes('institucioneditar')) return 'Editar Institución';
        if (r.includes('usuariocrear')) return 'Crear Usuario';
        if (r.includes('usuarioeditar')) return 'Editar Usuario';
        if (r.includes('/carrera/carreracrear')) return 'Crear Carrera';
        if (r.includes('/carrera/carreraeditar')) return 'Editar Carrera';
        if (r.includes('/adm10_cursos/crear')) return 'Crear Curso';
        if (r.includes('/adm10_cursos/editar')) return 'Editar Curso';
        if (r.includes('/adm10_cursos/eliminar')) return 'Eliminar Curso';
        if (r.includes('rolcrear')) return 'Crear Rol';
        if (r.includes('roleditar')) return 'Editar Rol';
        if (r.includes('grupocrear')) return 'Crear Grupo';
        if (r.includes('grupoeditar')) return 'Editar Grupo';
        if (r.includes('parametrocrear')) return 'Crear Parámetro';
        if (r.includes('parametroeditar')) return 'Editar Parámetro';
        if (r.includes('modulocrear')) return 'Crear Módulo';
        if (r.includes('moduloeditar')) return 'Editar Módulo';
        if (r.includes('crear')) return 'Crear';
        if (r.includes('editar')) return 'Editar';
        if (r.includes('eliminar')) return 'Eliminar';
        if (r.includes('detalle')) return 'Detalle';
        if (r.includes('historialacademico')) return 'Historial Académico';
        if (r.includes('listadoestudiantes')) return 'Listados por Periodo';
        if (r.includes('/facturacion/facturas')) return 'Administración de Facturas';
        if (r.includes('/pagos/pagos')) return 'Consulta de Pagos';
        if (r.includes('adm10_cursos')) return 'Mantenimiento Cursos';
        if (r.includes('adm11_profesor')) return 'Mantenimiento Profesores';
        if (r.includes('adm12_periodo')) return 'Mantenimiento Periodos';
        if (r.includes('adm13_grupo')) return 'Mantenimiento Grupos';
        if (r.includes('adm14_prematricula')) return 'Mantenimiento Prematrícula';

        return 'Página';
    }

    function mostrarMenuPorDefecto() {
        const sidebarNav = document.querySelector('.sidebar-nav');
        if (!sidebarNav) return;
        const loader = document.getElementById('menu-loader');
        if (loader) loader.remove();
        sidebarNav.innerHTML = '';
        agregarDashboard(sidebarNav);
        marcarRutaActiva();
        actualizarBreadcrumbs();
    }

    function inicializarSidebar() {
        const btnCollapse = document.querySelector('.btn-collapse');
        const btnMenuToggle = document.querySelector('.btn-menu-toggle');
        const sidebar = document.querySelector('.sidebar');
        if (btnCollapse) {
            btnCollapse.addEventListener('click', function () {
                if (sidebar) {
                    sidebar.classList.toggle('collapsed');
                    localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
                }
            });
        }
        if (btnMenuToggle) {
            btnMenuToggle.addEventListener('click', function () {
                if (sidebar) sidebar.classList.toggle('mobile-open');
            });
        }
        const estaColapsado = localStorage.getItem('sidebarCollapsed') === 'true';
        if (estaColapsado && sidebar) sidebar.classList.add('collapsed');
    }

    function init() {
        console.log('INICIANDO SISTEMA');
        cargarMenuDinamico();
        inicializarSidebar();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // En su lugar, usar MutationObserver para detectar cambios de ruta
    let ultimaRuta = window.location.pathname;

    // Detectar cambios de URL con popstate (botones navegador)
    window.addEventListener('popstate', function () {
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    });

    // Detectar cambios de URL usando MutationObserver en lugar de polling
    const observer = new MutationObserver(function () {
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    });

    observer.observe(document.querySelector('title'), {
        childList: true,
        characterData: true,
        subtree: true
    });

})();