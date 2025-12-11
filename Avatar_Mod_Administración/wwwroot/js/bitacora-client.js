(function () {
    'use strict';

    // Pantallas críticas que se deben registrar
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

    // Obtener datos del usuario desde el DOM
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

    // Registrar en bitácora
    async function registrarBitacora(tipoAccion, descripcionTexto, detalles = {}) {
        try {
            const datosUsuario = obtenerDatosUsuario();
            if (!datosUsuario || !datosUsuario.email) {
                console.warn('No hay datos de usuario para bitácora');
                return;
            }

            // Construir objeto de descripción
            const descripcionObj = {
                accion: tipoAccion,
                descripcion: descripcionTexto,
                usuario: datosUsuario.nombre,
                rol: datosUsuario.rol,
                fecha: new Date().toISOString(),
                ...detalles
            };

            // Payload para el backend
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

    // Verificar si la ruta actual es crítica (mejorado)
    function esPantallaCritica(ruta) {
        // Normalizar la ruta (eliminar query strings y trailing slashes)
        const rutaNormalizada = ruta.split('?')[0].replace(/\/$/, '');

        // Buscar coincidencia exacta o por prefijo
        return PANTALLAS_CRITICAS.some(critica => {
            const criticaNormalizada = critica.toLowerCase();
            const rutaLower = rutaNormalizada.toLowerCase();

            // Coincidencia exacta
            if (rutaLower === criticaNormalizada) {
                return true;
            }

            // Coincidencia por prefijo (para rutas con parámetros)
            // Ejemplo: /Usuario/UsuarioEditar?email=test@cuc.cr
            if (rutaLower.startsWith(criticaNormalizada.toLowerCase())) {
                return true;
            }

            return false;
        });
    }

    // Obtener nombre descriptivo de la pantalla
    function obtenerNombrePantalla(ruta) {
        const rutaLower = ruta.toLowerCase();

        // Mapeo de rutas a nombres descriptivos
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

        // Buscar coincidencia
        for (const [key, value] of Object.entries(mapeoNombres)) {
            if (rutaLower.includes(key)) {
                return value;
            }
        }

        return ruta;
    }

    // Registrar cambio de ruta en pantallas críticas
    function registrarCambioRuta() {
        const rutaActual = window.location.pathname;
        const rutaNormalizada = rutaActual.split('?')[0];

        if (!esPantallaCritica(rutaNormalizada)) {
            console.log('Ruta no crítica, no se registra:', rutaActual);
            return;
        }

        const rutaAnterior = sessionStorage.getItem('ruta_anterior') || '/';

        // No registrar si es la misma ruta (evitar duplicados)
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

    // Registrar error de UI
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

    // Inicializar observador de cambios de ruta
    function inicializarObservadorRutas() {
        console.log('Inicializando observador de rutas críticas');
        console.log('Pantallas críticas registradas:', PANTALLAS_CRITICAS.length);

        // Registrar ruta inicial si es crítica
        setTimeout(() => {
            registrarCambioRuta();
        }, 500); // Pequeño delay para asegurar que el DOM está listo





        let ultimaRuta = window.location.pathname;

        // Detectar con eventos nativos
        window.addEventListener('popstate', () => {
            setTimeout(() => registrarCambioRuta(), 100);
        });

        // Interceptar clicks en enlaces
        document.addEventListener('click', (event) => {
            const link = event.target.closest('a[href]');
            if (link && !link.href.includes('javascript:')) {
                const href = link.getAttribute('href');
                if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                    // Esperar a que la navegación ocurra
                    setTimeout(() => {
                        const nuevaRuta = window.location.pathname;
                        if (nuevaRuta !== ultimaRuta) {
                            ultimaRuta = nuevaRuta;
                            registrarCambioRuta();
                        }
                    }, 100);
                }
            }
        }, true);





        // También detectar con popstate (navegación con botones del navegador)
        window.addEventListener('popstate', () => {
            console.log('↩️ Navegación con botones del navegador');
            setTimeout(() => {
                registrarCambioRuta();
            }, 100);
        });

        // Detectar clicks en enlaces para registro inmediato
        document.addEventListener('click', (event) => {
            const link = event.target.closest('a[href]');
            if (link && !link.href.includes('javascript:')) {
                const href = link.getAttribute('href');
                if (href && !href.startsWith('#') && !href.startsWith('javascript:')) {
                    console.log('Click en enlace:', href);
                    // El registro se hará cuando cambie la URL
                }
            }
        });
    }

    // Capturar errores JavaScript globales
    window.addEventListener('error', (event) => {
        registrarError('JavaScript', event.message, {
            archivo: event.filename,
            linea: event.lineno,
            columna: event.colno,
            stack: event.error?.stack
        });
    });

    // Capturar promesas rechazadas
    window.addEventListener('unhandledrejection', (event) => {
        registrarError('Promise Rechazada', event.reason?.message || String(event.reason), {
            reason: String(event.reason)
        });
    });

    // Interceptar fetch para capturar errores HTTP
    const originalFetch = window.fetch;
    window.fetch = async function (...args) {
        try {
            const response = await originalFetch.apply(this, args);

            // Registrar errores HTTP
            if (!response.ok && response.status >= 400) {
                const url = typeof args[0] === 'string' ? args[0] : args[0]?.url;

                // Evitar loop infinito
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

    // Capturar errores de validación de formularios
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

    // Inicializar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', inicializarObservadorRutas);
    } else {
        inicializarObservadorRutas();
    }

    console.log('Bitácora client inicializado correctamente');

})();