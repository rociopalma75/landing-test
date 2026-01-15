document.addEventListener('DOMContentLoaded', function() {
    const formContacto = document.getElementById('formContacto');

    if (formContacto) {
        formContacto.addEventListener('submit', async function(e) {
            e.preventDefault();

            // Obtener el token de reCAPTCHA
            const recaptchaToken = grecaptcha.getResponse();

            if (!recaptchaToken) {
                alert('Por favor, completa el reCAPTCHA.');
                return;
            }

            // Recopilar datos del formulario
            const nombre = document.getElementById('nombre').value.trim();
            const email = document.getElementById('email').value.trim();
            const empresa = document.getElementById('empresa').value.trim();
            const numero = document.getElementById('numero').value.trim();
            const mensaje = document.getElementById('mensaje').value.trim();

            // Validar campos
            if (!nombre || !email || !empresa || !numero || !mensaje) {
                alert('Por favor, completa todos los campos del formulario.');
                return;
            }

            // Construir objeto de datos
            const formData = {
                Nombre: nombre,
                Email: email,
                Empresa: empresa,
                Telefono: numero,
                Mensaje: mensaje,
                Token: recaptchaToken
            };

            try {
                // Mostrar estado de envío
                const submitButton = formContacto.querySelector('.submit-button');
                const originalText = submitButton.textContent;
                submitButton.textContent = 'Enviando...';
                submitButton.disabled = true;

                // Llamar a la API
                const response = await fetch('http://localhost:7071/api/EnviarMailDesdeLandingPage', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(formData)
                });

                if (response.ok) {
                    const result = await response.json();
                    alert('¡Gracias por tu mensaje! Nos pondremos en contacto pronto.');
                    formContacto.reset();
                    grecaptcha.reset(); // Resetear reCAPTCHA
                } else {
                    alert('Hubo un error al enviar el mensaje. Por favor, intenta nuevamente.');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('Error al enviar el mensaje. Verifica tu conexión e intenta nuevamente.');
            } finally {
                // Restaurar botón
                const submitButton = formContacto.querySelector('.submit-button');
                submitButton.textContent = originalText;
                submitButton.disabled = false;
            }
        });
    }
});
