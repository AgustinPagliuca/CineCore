(function () {
    'use strict';

    function inicializar() {
        const botones = document.querySelectorAll('[data-password-toggle]');

        botones.forEach(function (boton) {
            boton.addEventListener('click', function () {
                const inputId = boton.getAttribute('data-password-toggle');
                const input = document.getElementById(inputId);
                const labelMostrar = boton.querySelector('.toggle-mostrar');
                const labelOcultar = boton.querySelector('.toggle-ocultar');

                if (!input) return;

                const visible = input.type === 'text';

                if (visible) {
                    input.type = 'password';
                    if (labelMostrar) labelMostrar.style.display = '';
                    if (labelOcultar) labelOcultar.style.display = 'none';
                } else {
                    input.type = 'text';
                    if (labelMostrar) labelMostrar.style.display = 'none';
                    if (labelOcultar) labelOcultar.style.display = '';
                }
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', inicializar);
    } else {
        inicializar();
    }
})();