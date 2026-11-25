/** Sends a JSON request to a VetCare API endpoint and returns its response. */
export async function apiRequest(path, options = {}) {
  const response = await fetch(`/api/${path}`, {
    ...options,
    headers: {
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...(options.headers || {}),
    },
  });

  if (!response.ok) {
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
