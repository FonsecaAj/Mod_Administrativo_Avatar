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
        'Cursos': { icono: 'bi-book', url: '/Curso/Cursos', grupo: 'Académico', prefijo: '/Curso' },
        'Grupos': { icono: 'bi-diagram-3', url: '/Grupo/Grupos', grupo: 'Académico', prefijo: '/Grupo' },
        'Prematrícula': { icono: 'bi-clipboard-check', url: '/Prematricula/Index', grupo: 'Matrícula', prefijo: '/Prematricula' },
        'Matrícula': { icono: 'bi-journal-check', url: '/Matricula/Index', grupo: 'Matrícula', prefijo: '/Matricula' },
        'Mis Cursos': { icono: 'bi-book-half', url: '/MisCursos/Index', grupo: 'Estudiante', prefijo: '/MisCursos' },
        'Desglose y Notas': { icono: 'bi-clipboard-data', url: '/Notas/Index', grupo: 'Evaluación', prefijo: '/Notas' },
        'Promedios': { icono: 'bi-graph-up', url: '/Promedios/Index', grupo: 'Evaluación', prefijo: '/Promedios' },
        'Reportes': { icono: 'bi-file-earmark-bar-graph', url: '/Reportes/Index', grupo: 'Reportes', prefijo: '/Reportes' }

    };

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

            if (loader) loader.remove();

            //  ADMINISTRADOR
            if (datosUsuario.rol?.toLowerCase() === 'administrador' || rolIdNum === 4) {
                construirMenu(modulos);
                agregarOpcionesAdministrador();
                actualizarBreadcrumbs();
                return;
            }

            //  menú normal
            if (!modulos || modulos.length === 0) { mostrarMenuPorDefecto(); return; }

            modulosCargados = modulos;
            construirMenu(modulos);
            actualizarBreadcrumbs();

        } catch (error) {
            console.error('Error al cargar menú:', error);
            mostrarMenuPorDefecto();
        }
    }

    function construirMenu(modulos) {
        const sidebarNav = document.querySelector('.sidebar-nav');
        if (!sidebarNav) return;
        sidebarNav.innerHTML = '';
        const grupos = agruparModulos(modulos);
        agregarDashboard(sidebarNav);
        Object.keys(grupos).sort().forEach(nombreGrupo => {
            const modulosGrupo = grupos[nombreGrupo];
            const tituloSeccion = document.createElement('div');
            tituloSeccion.className = 'nav-section-title';
            tituloSeccion.textContent = nombreGrupo;
            sidebarNav.appendChild(tituloSeccion);
            modulosGrupo.forEach(modulo => {
                const config = MODULOS_CONFIG[modulo.nombreModulo];
                if (config && modulo.moduloActivo) agregarItemMenu(sidebarNav, modulo.nombreModulo, config);
            });
        });
        marcarRutaActiva();
    }

    function agruparModulos(modulos) {
        const grupos = {};
        modulos.forEach(modulo => {
            const config = MODULOS_CONFIG[modulo.nombreModulo];
            if (config) {
                if (!grupos[config.grupo]) grupos[config.grupo] = [];
                grupos[config.grupo].push(modulo);
            }
        });
        return grupos;
    }

    function agregarDashboard(container) {
        const navItem = document.createElement('div');
        navItem.className = 'nav-item';
        navItem.innerHTML = '<a href="/" class="nav-link"><i class="bi bi-speedometer2"></i><span>Dashboard</span></a>';
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
        let html = '<li class="breadcrumb-item"><a href="/"><i class="bi bi-house-door"></i> Inicio</a></li>';

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
            breadcrumbOl.innerHTML = html;
            return;
        }

        html += '<li class="breadcrumb-item">' + moduloMatch.grupo + '</li>';

        if (ruta === moduloMatch.url) {
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

        if (r.includes('carreracrear')) return 'Crear Carrera';
        if (r.includes('carreraeditar')) return 'Editar Carrera';

        if (r.includes('cursocrear')) return 'Crear Curso';
        if (r.includes('cursoeditar')) return 'Editar Curso';

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
        if (r.includes('detalle')) return 'Detalle';

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

    window.addEventListener('popstate', function () {
        marcarRutaActiva();
        actualizarBreadcrumbs();
    });

    let ultimaRuta = window.location.pathname;
    setInterval(function () {
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    }, 200);



    //CONSTRUIR OPCIONES DEL MENU DE ADMINISTRADOR

    function agregarOpcionesAdministrador() {
        const sidebarNav = document.querySelector('.sidebar-nav');
        if (!sidebarNav) return;

        // Título de la sección
        const tituloSeccion = document.createElement('div');
        tituloSeccion.className = 'nav-section-title';
        tituloSeccion.textContent = 'Administrador';
        sidebarNav.appendChild(tituloSeccion);

        // Opciones específicas del administrador
        const opciones = [
            { texto: 'Home', url: '/Index', icono: 'bi-house' },
            { texto: 'Historial Académico', url: '/Academico/HistorialAcademico', icono: 'bi-journal-text' },
            { texto: 'Listados por periodo', url: '/Academico/ListadoEstudiantes', icono: 'bi-people' },
            { texto: 'Administración de facturas', url: '/Facturacion/Facturas', icono: 'bi-receipt' },
            { texto: 'Consulta de pagos', url: '/Pagos/Pagos', icono: 'bi-cash' },
            { texto: 'Cursos', url: '/ADM10_Cursos', icono: 'bi-book' },
            { texto: 'Profesores', url: '/ADM11_Profesor', icono: 'bi-people' },
            { texto: 'Periodo', url: '/ADM12_Periodo', icono: 'bi-journal-text' },
            { texto: 'Grupo', url: '/ADM13_Grupo', icono: 'bi-journal-text' },
            { texto: 'Prematricula', url: '/ADM14_Prematricula', icono: 'bi-receipt' }

        ];

        opciones.forEach(op => {
            const item = document.createElement('div');
            item.className = 'nav-item';
            item.innerHTML = `
            <a href="${op.url}" class="nav-link">
                <i class="bi ${op.icono}"></i>
                <span>${op.texto}</span>
            </a>`;
            sidebarNav.appendChild(item);
        });
    }

})(); (function () {
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
        'Cursos': { icono: 'bi-book', url: '/Curso/Cursos', grupo: 'Académico', prefijo: '/Curso' },
        'Grupos': { icono: 'bi-diagram-3', url: '/Grupo/Grupos', grupo: 'Académico', prefijo: '/Grupo' },
        'Prematrícula': { icono: 'bi-clipboard-check', url: '/Prematricula/Index', grupo: 'Matrícula', prefijo: '/Prematricula' },
        'Matrícula': { icono: 'bi-journal-check', url: '/Matricula/Index', grupo: 'Matrícula', prefijo: '/Matricula' },
        'Mis Cursos': { icono: 'bi-book-half', url: '/MisCursos/Index', grupo: 'Estudiante', prefijo: '/MisCursos' },
        'Desglose y Notas': { icono: 'bi-clipboard-data', url: '/Notas/Index', grupo: 'Evaluación', prefijo: '/Notas' },
        'Promedios': { icono: 'bi-graph-up', url: '/Promedios/Index', grupo: 'Evaluación', prefijo: '/Promedios' },
        'Reportes': { icono: 'bi-file-earmark-bar-graph', url: '/Reportes/Index', grupo: 'Reportes', prefijo: '/Reportes' }

    };

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

            if (loader) loader.remove();

            //  ADMINISTRADOR
            if (datosUsuario.rol?.toLowerCase() === 'administrador' || rolIdNum === 4) {
                construirMenu(modulos);
                agregarOpcionesAdministrador();
                actualizarBreadcrumbs();
                return;
            }

            //  menú normal
            if (!modulos || modulos.length === 0) { mostrarMenuPorDefecto(); return; }

            modulosCargados = modulos;
            construirMenu(modulos);
            actualizarBreadcrumbs();

        } catch (error) {
            console.error('Error al cargar menú:', error);
            mostrarMenuPorDefecto();
        }
    }

    function construirMenu(modulos) {
        const sidebarNav = document.querySelector('.sidebar-nav');
        if (!sidebarNav) return;
        sidebarNav.innerHTML = '';
        const grupos = agruparModulos(modulos);
        agregarDashboard(sidebarNav);
        Object.keys(grupos).sort().forEach(nombreGrupo => {
            const modulosGrupo = grupos[nombreGrupo];
            const tituloSeccion = document.createElement('div');
            tituloSeccion.className = 'nav-section-title';
            tituloSeccion.textContent = nombreGrupo;
            sidebarNav.appendChild(tituloSeccion);
            modulosGrupo.forEach(modulo => {
                const config = MODULOS_CONFIG[modulo.nombreModulo];
                if (config && modulo.moduloActivo) agregarItemMenu(sidebarNav, modulo.nombreModulo, config);
            });
        });
        marcarRutaActiva();
    }

    function agruparModulos(modulos) {
        const grupos = {};
        modulos.forEach(modulo => {
            const config = MODULOS_CONFIG[modulo.nombreModulo];
            if (config) {
                if (!grupos[config.grupo]) grupos[config.grupo] = [];
                grupos[config.grupo].push(modulo);
            }
        });
        return grupos;
    }

    function agregarDashboard(container) {
        const navItem = document.createElement('div');
        navItem.className = 'nav-item';
        navItem.innerHTML = '<a href="/" class="nav-link"><i class="bi bi-speedometer2"></i><span>Dashboard</span></a>';
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
        let html = '<li class="breadcrumb-item"><a href="/"><i class="bi bi-house-door"></i> Inicio</a></li>';

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
            breadcrumbOl.innerHTML = html;
            return;
        }

        html += '<li class="breadcrumb-item">' + moduloMatch.grupo + '</li>';

        if (ruta === moduloMatch.url) {
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

        if (r.includes('carreracrear')) return 'Crear Carrera';
        if (r.includes('carreraeditar')) return 'Editar Carrera';

        if (r.includes('cursocrear')) return 'Crear Curso';
        if (r.includes('cursoeditar')) return 'Editar Curso';

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
        if (r.includes('detalle')) return 'Detalle';

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

    window.addEventListener('popstate', function () {
        marcarRutaActiva();
        actualizarBreadcrumbs();
    });

    let ultimaRuta = window.location.pathname;
    setInterval(function () {
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    }, 200);



    //CONSTRUIR OPCIONES DEL MENU DE ADMINISTRADOR

    function agregarOpcionesAdministrador() {
        const sidebarNav = document.querySelector('.sidebar-nav');
        if (!sidebarNav) return;

        // Título de la sección
        const tituloSeccion = document.createElement('div');
        tituloSeccion.className = 'nav-section-title';
        tituloSeccion.textContent = 'Administrador';
        sidebarNav.appendChild(tituloSeccion);

        // Opciones específicas del administrador
        const opciones = [
            { texto: 'Home', url: '/Index', icono: 'bi-house' },
            { texto: 'Historial Académico', url: '/Academico/HistorialAcademico', icono: 'bi-journal-text' },
            { texto: 'Listados por periodo', url: '/Academico/ListadoEstudiantes', icono: 'bi-people' },
            { texto: 'Administración de facturas', url: '/Facturacion/Facturas', icono: 'bi-receipt' },
            { texto: 'Consulta de pagos', url: '/Pagos/Pagos', icono: 'bi-cash' },
            { texto: 'Cursos', url: '/ADM10_Cursos', icono: 'bi-book' },
            { texto: 'Profesores', url: '/ADM11_Profesor', icono: 'bi-people' },
            { texto: 'Periodo', url: '/ADM12_Periodo', icono: 'bi-journal-text' },
            { texto: 'Grupo', url: '/ADM13_Grupo', icono: 'bi-journal-text' },
            { texto: 'Prematricula', url: '/ADM14_Prematricula', icono: 'bi-receipt' }

        ];

        opciones.forEach(op => {
            const item = document.createElement('div');
            item.className = 'nav-item';
            item.innerHTML = `
            <a href="${op.url}" class="nav-link">
                <i class="bi ${op.icono}"></i>
                <span>${op.texto}</span>
            </a>`;
            sidebarNav.appendChild(item);
        });
    }

})();