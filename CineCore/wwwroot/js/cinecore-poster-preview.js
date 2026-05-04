/* ============================================================
   CineCore — Vista previa del poster en vivo
   ============================================================
   Cuando el usuario tipea o pega una URL en #poster-url-input,
   se actualiza #poster-preview con la imagen.
   Si la URL es inválida o falla la carga, vuelve al placeholder.
   ============================================================ */

(function () {
    'use strict';

    function inicializar() {
        const input = document.getElementById('poster-url-input');
        const preview = document.getElementById('poster-preview');
        const placeholder = document.getElementById('poster-preview-placeholder');

        if (!input || !preview || !placeholder) {
            return;
        }

        function mostrarPlaceholder() {
            preview.style.display = 'none';
            preview.removeAttribute('src');
            placeholder.style.display = '';
        }

        function mostrarImagen(url) {
            preview.style.display = '';
            preview.src = url;
            placeholder.style.display = 'none';
        }

        function actualizar() {
            const url = input.value.trim();

            if (url === '') {
                mostrarPlaceholder();
            } else {
                // Probar cargar la imagen antes de mostrarla.
                const tester = new Image();
                tester.onload = function () { mostrarImagen(url); };
                tester.onerror = function () { mostrarPlaceholder(); };
                tester.src = url;
            }
        }

        // Debounce: esperar 400ms desde el último input antes de actualizar.
        let timeout;
        input.addEventListener('input', function () {
            clearTimeout(timeout);
            timeout = setTimeout(actualizar, 400);
        });

        // También actualizar en blur (cuando el usuario sale del campo).
        input.addEventListener('blur', function () {
            clearTimeout(timeout);
            actualizar();
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', inicializar);
    } else {
        inicializar();
    }
})();