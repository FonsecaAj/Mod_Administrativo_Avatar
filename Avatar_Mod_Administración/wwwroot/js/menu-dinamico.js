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

    async function cargarModulosPorRol(rolId, token) {
        try {
            const cacheKey = `modulos_rol_${rolId}`;
            const cached = sessionStorage.getItem(cacheKey);

            if (cached) {
                console.log('Módulos cargados desde caché');
                try {
                    return JSON.parse(cached);
                } catch (e) {
                    sessionStorage.removeItem(cacheKey);
                }
            }

            const rolIdNum = parseInt(rolId);
            if (!rolIdNum || rolIdNum === 0) return [];

            const response = await fetch(`/api/rol/${rolIdNum}/modulos`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': token
                },
                credentials: 'include'
            });

            if (!response.ok) {
                if (response.status === 401) window.location.href = '/Login';
                return [];
            }

            const modulos = await response.json();
            sessionStorage.setItem(cacheKey, JSON.stringify(modulos));
            setTimeout(() => sessionStorage.removeItem(cacheKey), 10 * 60 * 1000);

            return modulos;
        } catch (error) {
            console.error('Error al cargar módulos:', error);
            return [];
        }
    }

    async function cargarMenuDinamico() {
        const loader = document.getElementById('menu-loader');
        try {
            const datosUsuario = obtenerDatosUsuario();
            if (!datosUsuario?.token) {
                mostrarMenuPorDefecto();
                return;
            }

            const rolIdNum = parseInt(datosUsuario.rolId);
            if (!rolIdNum || rolIdNum === 0) {
                mostrarMenuPorDefecto();
                return;
            }

            const modulos = await cargarModulosPorRol(datosUsuario.rolId, datosUsuario.token);

            if (!modulos || modulos.length === 0) {
                mostrarMenuPorDefecto();
                return;
            }

            modulosCargados = modulos;
            if (loader) loader.remove();
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

        const fragment = document.createDocumentFragment();
        const grupos = agruparModulos(modulos);

        const dashboardItem = document.createElement('div');
        dashboardItem.className = 'nav-item';
        dashboardItem.innerHTML = '<a href="/" class="nav-link"><i class="bi bi-speedometer2"></i><span>Dashboard</span></a>';
        fragment.appendChild(dashboardItem);

        Object.keys(grupos).sort().forEach(nombreGrupo => {
            const modulosGrupo = grupos[nombreGrupo];

            const tituloSeccion = document.createElement('div');
            tituloSeccion.className = 'nav-section-title';
            tituloSeccion.textContent = nombreGrupo;
            fragment.appendChild(tituloSeccion);

            modulosGrupo.forEach(modulo => {
                const config = MODULOS_CONFIG[modulo.nombreModulo];
                if (config && modulo.moduloActivo) {
                    const navItem = document.createElement('div');
                    navItem.className = 'nav-item';
                    navItem.innerHTML = `<a href="${config.url}" class="nav-link"><i class="bi ${config.icono}"></i><span>${modulo.nombreModulo}</span></a>`;
                    fragment.appendChild(navItem);
                }
            });
        });

        sidebarNav.innerHTML = '';
        sidebarNav.appendChild(fragment);
        marcarRutaActiva();
    }

    function agruparModulos(modulos) {
        return modulos.reduce((grupos, modulo) => {
            const config = MODULOS_CONFIG[modulo.nombreModulo];
            if (config) {
                if (!grupos[config.grupo]) grupos[config.grupo] = [];
                grupos[config.grupo].push(modulo);
            }
            return grupos;
        }, {});
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

        html += `<li class="breadcrumb-item">${moduloMatch.grupo}</li>`;

        if (ruta === moduloMatch.url) {
            html += `<li class="breadcrumb-item active" aria-current="page">${moduloMatch.nombre}</li>`;
        } else {
            html += `<li class="breadcrumb-item"><a href="${moduloMatch.url}">${moduloMatch.nombre}</a></li>`;
            const subpagina = obtenerNombreSubpagina(ruta);
            html += `<li class="breadcrumb-item active" aria-current="page">${subpagina}</li>`;
        }

        breadcrumbOl.innerHTML = html;
    }

    function obtenerNombreSubpagina(ruta) {
        const r = ruta.toLowerCase();

        const mapSubpaginas = {
            'institucioncrear': 'Crear Institución',
            'institucioneditar': 'Editar Institución',
            'usuariocrear': 'Crear Usuario',
            'usuarioeditar': 'Editar Usuario',
            'carreracrear': 'Crear Carrera',
            'carreraeditar': 'Editar Carrera',
            'cursocrear': 'Crear Curso',
            'cursoeditar': 'Editar Curso',
            'rolcrear': 'Crear Rol',
            'roleditar': 'Editar Rol',
            'grupocrear': 'Crear Grupo',
            'grupoeditar': 'Editar Grupo',
            'parametrocrear': 'Crear Parámetro',
            'parametroeditar': 'Editar Parámetro',
            'modulocrear': 'Crear Módulo',
            'moduloeditar': 'Editar Módulo'
        };

        for (const key in mapSubpaginas) {
            if (r.includes(key)) return mapSubpaginas[key];
        }

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

        sidebarNav.innerHTML = '<div class="nav-item"><a href="/" class="nav-link"><i class="bi bi-speedometer2"></i><span>Dashboard</span></a></div>';

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

    let ultimaRuta = window.location.pathname;

    const originalPushState = history.pushState;
    const originalReplaceState = history.replaceState;

    history.pushState = function () {
        originalPushState.apply(this, arguments);
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    };

    history.replaceState = function () {
        originalReplaceState.apply(this, arguments);
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    };

    window.addEventListener('popstate', function () {
        if (window.location.pathname !== ultimaRuta) {
            ultimaRuta = window.location.pathname;
            marcarRutaActiva();
            actualizarBreadcrumbs();
        }
    });

})();