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
        'Cursos': { icono: 'bi-book', url: '/ADM10_Cursos', grupo: 'Académico', prefijo: '/ADM10_Cursos' },
        'Grupos': { icono: 'bi-diagram-3', url: '/ADM13_Grupo', grupo: 'Académico', prefijo: '/ADM13_Grupo' },
        'Prematrícula': { icono: 'bi-clipboard-check', url: '/ADM14_Prematricula/Index', grupo: 'Matrícula', prefijo: '/ADM14_Prematricula' },
        'Matrícula': { icono: 'bi-journal-check', url: '/Matricula/Index', grupo: 'Matrícula', prefijo: '/Matricula' },
        'Mis Cursos': { icono: 'bi-book-half', url: '/MisCursos/Index', grupo: 'Estudiante', prefijo: '/MisCursos' },
        'Desglose y Notas': { icono: 'bi-clipboard-data', url: '/Notas/Index', grupo: 'Evaluación', prefijo: '/Notas' },
        'Promedios': { icono: 'bi-graph-up', url: '/Promedios/Index', grupo: 'Evaluación', prefijo: '/Promedios' },
        'Reportes': { icono: 'bi-file-earmark-bar-graph', url: '/Reportes/Index', grupo: 'Reportes', prefijo: '/Reportes' },
        'Profesores': { icono: 'bi-file-earmark-bar-graph', url: '/ADM11_Profesor', grupo: 'Profesores', prefijo: '/ADM11_Profesor' },
        'Notificaciones': { icono: 'bi-envelope', url: '/Notificaciones_Correo/Index', grupo: 'Comunicación', prefijo: '/Notificaciones_Correo' }
    };

    const ADMIN_MENU_EXTENSIONS = [
        { texto: 'Home', url: '/Index', icono: 'bi-house', grupo: 'Principal', esDashboard: true, prefijo: '/Index' },
        { texto: 'Historial Académico', url: '/Academico/HistorialAcademico', icono: 'bi-journal-text', grupo: 'Académico', prefijo: '/Academico/HistorialAcademico' },
        { texto: 'Listados por periodo', url: '/Academico/ListadoEstudiantes', icono: 'bi-people', grupo: 'Académico', prefijo: '/Academico/ListadoEstudiantes' },
        { texto: 'Administración de facturas', url: '/Facturacion/Facturas', icono: 'bi-receipt', grupo: 'Facturación', prefijo: '/Facturacion' },
        { texto: 'Consulta de pagos', url: '/Pagos/Pagos', icono: 'bi-cash', grupo: 'Facturación', prefijo: '/Pagos' },
        { texto: 'Prueba Notificaciones', url: '/Notificaciones_Correo/Index', icono: 'bi-envelope', grupo: 'Comunicación', prefijo: '/Notificaciones_Correo' },
        { texto: 'Cursos', url: '/ADM10_Cursos', icono: 'bi-book', grupo: 'Mantenimiento Adm', prefijo: '/ADM10_Cursos' },
        { texto: 'Profesores', url: '/ADM11_Profesor', icono: 'bi-people', grupo: 'Mantenimiento Adm', prefijo: '/ADM11_Profesor' },
        { texto: 'Periodo', url: '/ADM12_Periodo', icono: 'bi-journal-text', grupo: 'Mantenimiento Adm', prefijo: '/ADM12_Periodo' },
        { texto: 'Grupo', url: '/ADM13_Grupo', icono: 'bi-diagram-3', grupo: 'Mantenimiento Adm', prefijo: '/ADM13_Grupo' },
        { texto: 'Prematrícula', url: '/ADM14_Prematricula', icono: 'bi-receipt', grupo: 'Mantenimiento Adm', prefijo: '/ADM14_Prematricula' },
        { texto: 'Reporte Bitacoras', url: '/Mod_General/Index', icono: 'bi-cash', grupo: 'Mantenimiento Adm', prefijo: '/Mod_General/Index' },
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
                    const li = document.createElement('li');
                    li.className = 'nav-item';
                    const a = document.createElement('a');
                    a.className = 'nav-link';
                    a.href = config.url;
                    a.dataset.moduloPrefijo = config.prefijo;
                    a.innerHTML = `<i class="bi ${config.icono}"></i><span class="nav-text">${modulo.nombreModulo}</span>`;
                    li.appendChild(a);
                    sidebarNav.appendChild(li);
                }
            });
        });

        marcarRutaActiva();
    }

    function agruparModulos(modulos) {
        const grupos = {};
        modulos.forEach(modulo => {
            const config = MODULOS_CONFIG[modulo.nombreModulo];
            if (!config) return;
            const nombreGrupo = config.grupo;
            if (!grupos[nombreGrupo]) grupos[nombreGrupo] = [];
            grupos[nombreGrupo].push(modulo);
        });
        return grupos;
    }

    function agregarDashboard(sidebarNav) {
        const tituloSeccion = document.createElement('div');
        tituloSeccion.className = 'nav-section-title';
        tituloSeccion.textContent = 'Principal';
        sidebarNav.appendChild(tituloSeccion);
        const li = document.createElement('li');
        li.className = 'nav-item';
        const a = document.createElement('a');
        a.className = 'nav-link';
        a.href = '/Index';
        a.dataset.moduloPrefijo = '/Index';
        a.innerHTML = '<i class="bi bi-house"></i><span class="nav-text">Home</span>';
        li.appendChild(a);
        sidebarNav.appendChild(li);
    }

    function marcarRutaActiva() {
        const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');
        const rutaActual = window.location.pathname.toLowerCase();

        navLinks.forEach(link => {
            link.classList.remove('active');
            const prefijo = link.dataset.moduloPrefijo?.toLowerCase();
            if (prefijo) {
                // Normalizar rutas para comparación
                const rutaNormalizada = rutaActual.endsWith('/') ? rutaActual.slice(0, -1) : rutaActual;
                const prefijoNormalizado = prefijo.endsWith('/') ? prefijo.slice(0, -1) : prefijo;

                if (rutaNormalizada === prefijoNormalizado || rutaNormalizada.startsWith(prefijoNormalizado + '/')) {
                    link.classList.add('active');
                }
            }
        });
    }

    function actualizarBreadcrumbs() {
        const breadcrumbOl = document.querySelector('.breadcrumb');
        if (!breadcrumbOl) return;

        const ruta = window.location.pathname.toLowerCase();
        let html = '<li class="breadcrumb-item"><a href="/Index"><i class="bi bi-house"></i> Home</a></li>';

        // Casos especiales primero
        if (ruta === '/' || ruta === '/index' || ruta === '/index/') {
            breadcrumbOl.innerHTML = '<li class="breadcrumb-item active" aria-current="page"><i class="bi bi-house"></i> Home</li>';
            return;
        }

        // Buscar coincidencia en todas las configuraciones
        let moduloMatch = null;

        // 1. Primero buscar en ADMIN_MENU_EXTENSIONS (para administradores)
        const datosUsuario = obtenerDatosUsuario();
        if (datosUsuario?.rol.toLowerCase() === 'administrador') {
            for (const item of ADMIN_MENU_EXTENSIONS) {
                if (item.esDashboard) continue;
                const prefijoItem = item.prefijo || item.url;
                const rutaNormalizada = ruta.endsWith('/') ? ruta.slice(0, -1) : ruta;
                const prefijoNormalizado = prefijoItem.toLowerCase().endsWith('/')
                    ? prefijoItem.toLowerCase().slice(0, -1)
                    : prefijoItem.toLowerCase();

                if (rutaNormalizada === prefijoNormalizado || rutaNormalizada.startsWith(prefijoNormalizado + '/')) {
                    moduloMatch = {
                        nombre: item.texto,
                        url: item.url,
                        grupo: item.grupo,
                        prefijo: prefijoItem
                    };
                    break;
                }
            }
        }

        // 2. Si no se encontró, buscar en MODULOS_CONFIG
        if (!moduloMatch) {
            for (const nombre in MODULOS_CONFIG) {
                const config = MODULOS_CONFIG[nombre];
                const rutaNormalizada = ruta.endsWith('/') ? ruta.slice(0, -1) : ruta;
                const prefijoNormalizado = config.prefijo.toLowerCase().endsWith('/')
                    ? config.prefijo.toLowerCase().slice(0, -1)
                    : config.prefijo.toLowerCase();

                if (rutaNormalizada === prefijoNormalizado || rutaNormalizada.startsWith(prefijoNormalizado + '/')) {
                    moduloMatch = {
                        nombre: nombre,
                        url: config.url,
                        grupo: config.grupo,
                        prefijo: config.prefijo
                    };
                    break;
                }
            }
        }

        if (!moduloMatch) {
            breadcrumbOl.innerHTML = html;
            return;
        }

        html += '<li class="breadcrumb-item">' + moduloMatch.grupo + '</li>';

        // Normalizar URLs para comparación
        const rutaNormalizada = ruta.endsWith('/') ? ruta.slice(0, -1) : ruta;
        const urlNormalizada = moduloMatch.url.toLowerCase().endsWith('/')
            ? moduloMatch.url.toLowerCase().slice(0, -1)
            : moduloMatch.url.toLowerCase();

        // Comprobar si la ruta actual es la ruta base del módulo
        if (rutaNormalizada === urlNormalizada) {
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

        // Instituciones
        if (r.includes('institucion/institucioncrear')) return 'Crear Institución';
        if (r.includes('institucion/institucioneditar')) return 'Editar Institución';

        // Usuarios
        if (r.includes('usuario/usuariocrear')) return 'Crear Usuario';
        if (r.includes('usuario/usuarioeditar')) return 'Editar Usuario';

        // Carreras
        if (r.includes('carrera/carreracrear')) return 'Crear Carrera';
        if (r.includes('carrera/carreraeditar')) return 'Editar Carrera';

        // Cursos
        if (r.includes('adm10_cursos/crear')) return 'Crear Curso';
        if (r.includes('adm10_cursos/editar')) return 'Editar Curso';
        if (r.includes('adm10_cursos/eliminar')) return 'Eliminar Curso';
        if (r.includes('adm10_cursos')) return 'Cursos';

        // Profesores
        if (r.includes('adm11_profesor/crear')) return 'Crear Profesor';
        if (r.includes('adm11_profesor/editar')) return 'Editar Profesor';
        if (r.includes('adm11_profesor')) return 'Profesores';

        // Periodos
        if (r.includes('adm12_periodo/crear')) return 'Crear Periodo';
        if (r.includes('adm12_periodo/editar')) return 'Editar Periodo';
        if (r.includes('adm12_periodo')) return 'Periodos';

        // Grupos
        if (r.includes('adm13_grupo/crear')) return 'Crear Grupo';
        if (r.includes('adm13_grupo/editar')) return 'Editar Grupo';
        if (r.includes('adm13_grupo')) return 'Grupos';

        // Prematrícula
        if (r.includes('adm14_prematricula/crear')) return 'Crear Prematrícula';
        if (r.includes('adm14_prematricula/editar')) return 'Editar Prematrícula';
        if (r.includes('adm14_prematricula')) return 'Prematrícula';

        // Roles
        if (r.includes('rol/rolcrear')) return 'Crear Rol';
        if (r.includes('rol/roleditar')) return 'Editar Rol';

        // Parámetros
        if (r.includes('parametro/parametrocrear')) return 'Crear Parámetro';
        if (r.includes('parametro/parametroeditar')) return 'Editar Parámetro';

        // Módulos
        if (r.includes('modulo/modulocrear')) return 'Crear Módulo';
        if (r.includes('modulo/moduloeditar')) return 'Editar Módulo';

        // Académico
        if (r.includes('academico/historialacademico')) return 'Historial Académico';
        if (r.includes('academico/listadoestudiantes')) return 'Listados por Periodo';

        // Facturación
        if (r.includes('facturacion/facturas')) return 'Administración de Facturas';
        if (r.includes('pagos/pagos')) return 'Consulta de Pagos';

        // Genéricos
        if (r.includes('/crear')) return 'Crear';
        if (r.includes('/editar')) return 'Editar';
        if (r.includes('/eliminar')) return 'Eliminar';
        if (r.includes('/detalle')) return 'Detalle';

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

})();