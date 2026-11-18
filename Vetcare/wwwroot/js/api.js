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
    let message = "No se pudo completar la operación.";

    try {
      const data = await response.json();
      message =
        data.message ||
        Object.values(data.errors || {}).flat().join(" ") ||
        message;
    } catch {
      // Keep the default message when the server does not return JSON.
    }

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
