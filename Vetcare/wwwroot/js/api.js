/** Sends a JSON request to a VetCare API endpoint and returns its response. */
export async function apiRequest(path, options = {}) {
  const token = sessionStorage.getItem("vetcare-access-token");
  if (!token) {
    window.location.replace("/login.html");
    throw new Error("Inicia sesión para continuar.");
  }

  const response = await fetch(`/api/${path}`, {
    ...options,
    headers: {
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      Authorization: `Bearer ${token}`,
      ...(options.headers || {}),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      sessionStorage.removeItem("vetcare-access-token");
      window.location.replace("/login.html");
    }
    const body = await response.text();
    let data = {};
    try { data = body ? JSON.parse(body) : {}; } catch { /* Keep the status-based message. */ }
    const validation = Object.values(data.errors || {}).flat().join(" ");
    const message = data.message || data.detail || validation || (response.status === 404
      ? `VetCare no encontró la ruta /api/${path}. Reinicia la aplicación desde Rider para cargar los módulos nuevos.`
      : `La solicitud falló (HTTP ${response.status}).`);
    throw new Error(message);
  }

  return response.status === 204 ? null : response.json();
}

document.addEventListener("click", (event) => {
  if (!event.target.closest("[data-logout]")) return;
  sessionStorage.removeItem("vetcare-access-token");
  window.location.replace("/login.html");
});

/** Escapes API values before inserting them into an HTML template. */
export function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, (character) => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;",
  })[character]);
}

/** Updates a live status message using the shared success/error colors. */
export function setStatus(element, message, isError = false) {
  element.textContent = message;
  element.classList.toggle("error", isError);
}
