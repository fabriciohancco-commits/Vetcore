const $ = (id) => document.getElementById(id);

if (sessionStorage.getItem("vetcare-access-token")) window.location.replace("/index.html");

function status(element, message, isError = false) {
  element.textContent = message;
  element.classList.toggle("error", isError);
}

async function submitAuth(form, endpoint, noticeId, busyLabel) {
  const button = form.querySelector('[type="submit"]');
  button.disabled = true;
  const oldLabel = button.textContent;
  button.textContent = busyLabel;
  try {
    const isSetup = endpoint.endsWith("configuracion-inicial");
    const payload = isSetup
      ? { correo: $("setup-email").value.trim(), contrasena: $("setup-password").value }
      : { correo: $("login-email").value.trim(), contrasena: $("login-password").value };
    const response = await fetch(`/api/auth/${endpoint}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.message || data.detail || "No se pudo iniciar sesión.");
    sessionStorage.setItem("vetcare-access-token", data.accessToken);
    window.location.replace("/index.html");
  } catch (error) {
    status($(noticeId), error.message, true);
  } finally {
    button.disabled = false;
    button.textContent = oldLabel;
  }
}

$("login-form").addEventListener("submit", (event) => {
  event.preventDefault();
  submitAuth(event.currentTarget, "login", "login-notice", "Validando...");
});

$("setup-form").addEventListener("submit", (event) => {
  event.preventDefault();
  submitAuth(event.currentTarget, "configuracion-inicial", "setup-notice", "Creando cuenta...");
});

fetch("/api/auth/estado")
  .then((response) => response.json())
  .then((state) => {
    if (state.requiereConfiguracionInicial) {
      $("login-view").classList.add("hidden");
      $("setup-view").classList.remove("hidden");
      $("setup-email").focus();
    }
  })
  .catch(() => status($("login-notice"), "No se pudo conectar con la API. Comprueba que VetCare esté en ejecución.", true));
