(function () {
    'use strict';

    const PANTALLAS_CRITICAS = [
        '/Usuario/Usuarios',
        '/Usuario/UsuarioCrear',
        '/Usuario/UsuarioEditar',
        '/Usuario/UsuarioDetalle',
        '/Rol/Roles',
        '/Rol/RolCrear',
        '/Rol/RolEditar',
        '/Institucion/Instituciones',
        '/Institucion/InstitucionCrear',
        '/Institucion/InstitucionEditar',
        '/Carrera/Carreras',
        '/Curso/Cursos',
        '/Periodo/Periodos',
        '/Parametro/Parametros',
        '/Parametro/ParametroCrear',
        '/Parametro/ParametroEditar',
        '/Permiso/Permisos',
        '/Modulo/Modulos',
        '/Modulo/ModuloCrear',
        '/Modulo/ModuloEditar'
    ];

    function obtenerDatosUsuario() {
        const appData = document.getElementById('app-data');
        if (!appData) {
            console.warn('No se encontró #app-data');
            return null;
        }

        return {
            email: appData.dataset.usuarioId || '',
            nombre: appData.dataset.usuarioNombre || 'Usuario',
            rol: appData.dataset.usuarioRol || 'Sin rol'
        };
    }

    async function registrarBitacora(tipoAccion, descripcionTexto, detalles = {}) {
        try {
            const datosUsuario = obtenerDatosUsuario();
            if (!datosUsuario || !datosUsuario.email) {
                console.warn('No hay datos de usuario para bitácora');
                return;
            }

            const descripcionObj = {
                accion: tipoAccion,
                descripcion: descripcionTexto,
                usuario: datosUsuario.nombre,
                rol: datosUsuario.rol,
                fecha: new Date().toISOString(),
                ...detalles
            };

            const payload = {
                usuario: datosUsuario.email,
                descripcion: JSON.stringify(descripcionObj)
            };

            console.log('Registrando en bitácora:', descripcionObj);

            const response = await fetch('/api/bitacora', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                console.error('Error al registrar bitácora:', response.status);
            } else {
                console.log('Bitácora registrada exitosamente');
            }

        } catch (error) {
            console.error('Excepción al registrar bitácora:', error);
        }
    }

    function esPantallaCritica(ruta) {
        const rutaNormalizada = ruta.split('?')[0].replace(/\/$/, '');

        return PANTALLAS_CRITICAS.some(critica => {
            const criticaNormalizada = critica.toLowerCase();
            const rutaLower = rutaNormalizada.toLowerCase();

            if (rutaLower === criticaNormalizada) {
                return true;
            }

            if (rutaLower.startsWith(criticaNormalizada.toLowerCase())) {
                return true;
            }

            return false;
        });
    }

    function obtenerNombrePantalla(ruta) {
        const rutaLower = ruta.toLowerCase();

        const mapeoNombres = {
            '/usuario/usuarios': 'Listado de Usuarios',
            '/usuario/usuariocrear': 'Crear Usuario',
            '/usuario/usuarioeditar': 'Editar Usuario',
            '/usuario/usuariodetalle': 'Detalle de Usuario',
            '/rol/roles': 'Listado de Roles',
            '/rol/rolcrear': 'Crear Rol',
            '/rol/roleditar': 'Editar Rol',
            '/institucion/instituciones': 'Listado de Instituciones',
            '/institucion/institucioncrear': 'Crear Institución',
            '/institucion/institucioneditar': 'Editar Institución',
            '/carrera/carreras': 'Listado de Carreras',
            '/curso/cursos': 'Listado de Cursos',
            '/periodo/periodos': 'Listado de Períodos',
            '/parametro/parametros': 'Listado de Parámetros',
            '/parametro/parametrocrear': 'Crear Parámetro',
            '/parametro/parametroeditar': 'Editar Parámetro',
            '/permiso/permisos': 'Listado de Permisos',
            '/modulo/modulos': 'Listado de Módulos',
            '/modulo/modulocrear': 'Crear Módulo',
            '/modulo/moduloeditar': 'Editar Módulo'
        };

        for (const [key, value] of Object.entries(mapeoNombres)) {
            if (rutaLower.includes(key)) {
                return value;
            }
        }

        return ruta;
    }

    function registrarCambioRuta() {
        const rutaActual = window.location.pathname;
        const rutaNormalizada = rutaActual.split('?')[0];

        if (!esPantallaCritica(rutaNormalizada)) {
            console.log('Ruta no crítica, no se registra:', rutaActual);
            return;
        }

        const rutaAnterior = sessionStorage.getItem('ruta_anterior') || '/';

        if (rutaAnterior === rutaNormalizada) {
            console.log('Misma ruta, no se registra nuevamente');
            return;
        }

        const nombrePantalla = obtenerNombrePantalla(rutaNormalizada);

        console.log('Cambio de ruta detectado:', rutaAnterior, '→', rutaNormalizada);

        registrarBitacora(
            'NAVEGACION',
            `Acceso a pantalla: ${nombrePantalla}`,
            {
                pantalla_actual: rutaNormalizada,
                pantalla_anterior: rutaAnterior,
                url_completa: window.location.href,
                nombre_pantalla: nombrePantalla
            }
        );

        sessionStorage.setItem('ruta_anterior', rutaNormalizada);
    }

    function registrarError(tipo, mensaje, detalles = {}) {
        console.log('Error de UI capturado:', tipo, mensaje);

        registrarBitacora(
            'ERROR_UI',
            `Error de tipo ${tipo}: ${mensaje}`,
            {
                tipo_error: tipo,
                mensaje_completo: mensaje,
                pantalla: window.location.pathname,
                url: window.location.href,
                ...detalles
            }
        );
    }

    // Eliminar delays innecesarios
    function inicializarObservadorRutas() {
        console.log('Inicializando observador de rutas críticas');
        console.log('Pantallas críticas registradas:', PANTALLAS_CRITICAS.length);

        // Registrar ruta inicial inmediatamente (sin delay de 500ms)
        registrarCambioRuta();

        let ultimaRuta = window.location.pathname;

        window.addEventListener('popstate', () => {
            registrarCambioRuta(); // Inmediato, sin delay de 100ms
        });

        // Eliminar timeout de 100ms
        document.addEventListener('click', (event) => {
            const link = event.target.closest('a[href]');
            if (link && !link.href.includes('javascript:')) {
                const href = link.getAttribute('href');
                if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                    // Usar requestAnimationFrame en lugar de setTimeout
                    requestAnimationFrame(() => {
                        const nuevaRuta = window.location.pathname;
                        if (nuevaRuta !== ultimaRuta) {
                            ultimaRuta = nuevaRuta;
                            registrarCambioRuta();
                        }
                    });
                }
            }
        }, true);
    }

    window.addEventListener('error', (event) => {
        registrarError('JavaScript', event.message, {
            archivo: event.filename,
            linea: event.lineno,
            columna: event.colno,
            stack: event.error?.stack
        });
    });

    window.addEventListener('unhandledrejection', (event) => {
        registrarError('Promise Rechazada', event.reason?.message || String(event.reason), {
            reason: String(event.reason)
        });
    });

    const originalFetch = window.fetch;
    window.fetch = async function (...args) {
        try {
            const response = await originalFetch.apply(this, args);

            if (!response.ok && response.status >= 400) {
                const url = typeof args[0] === 'string' ? args[0] : args[0]?.url;

                if (!url.includes('/api/bitacora')) {
                    registrarError('HTTP', `Error ${response.status} ${response.statusText}`, {
                        url: url,
                        status: response.status,
                        statusText: response.statusText,
                        metodo: args[1]?.method || 'GET'
                    });
                }
            }

            return response;
        } catch (error) {
            const url = typeof args[0] === 'string' ? args[0] : args[0]?.url || 'unknown';

            if (!url.includes('/api/bitacora')) {
                registrarError('Network', error.message, {
                    url: url,
                    mensaje: error.message
                });
            }

            throw error;
        }
    };

    document.addEventListener('invalid', (event) => {
        if (event.target instanceof HTMLInputElement ||
            event.target instanceof HTMLTextAreaElement ||
            event.target instanceof HTMLSelectElement) {

            registrarError('Validacion Formulario', event.target.validationMessage, {
                campo: event.target.name || event.target.id || 'sin_nombre',
                tipo_campo: event.target.type,
                valor: event.target.value,
                pantalla: obtenerNombrePantalla(window.location.pathname)
            });
        }
    }, true);

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', inicializarObservadorRutas);
    } else {
        inicializarObservadorRutas();
    }

    console.log('Bitácora client inicializado correctamente');

})();