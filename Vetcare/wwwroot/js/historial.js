import { apiRequest, escapeHtml, setStatus } from "./api.js";

const petSelect = document.getElementById("pet");
const searchButton = document.getElementById("search");
const message = document.getElementById("msg");
const list = document.getElementById("list");

function renderHistorial(consultas) {
  if (consultas.length === 0) {
    list.innerHTML = '<div class="empty">Esta mascota no tiene consultas registradas.</div>';
    return;
  }

  list.innerHTML = consultas.map((consulta) => {
    const date = new Date(consulta.fechaConsulta);
    const optionalDetails = [
      consulta.tratamiento
        ? `<div><dt>Tratamiento</dt><dd>${escapeHtml(consulta.tratamiento)}</dd></div>`
        : "",
      consulta.vacunasAplicadas
        ? `<div><dt>Vacunas</dt><dd>${escapeHtml(consulta.vacunasAplicadas)}</dd></div>`
        : "",
    ].join("");

    return `
      <article class="record-card">
        <div class="record-header">
          <div>
            <span class="record-label">${escapeHtml(consulta.veterinarioNombre)}</span>
            <h3>${date.toLocaleDateString("es-PE", { dateStyle: "long" })}</h3>
          </div>
          <time class="record-date">${date.toLocaleTimeString("es-PE", {
            hour: "2-digit",
            minute: "2-digit",
          })}</time>
        </div>
        <dl class="record-details">
          <div><dt>Motivo</dt><dd>${escapeHtml(consulta.motivo)}</dd></div>
          <div><dt>Diagnóstico</dt><dd>${escapeHtml(consulta.diagnostico)}</dd></div>
          ${optionalDetails}
        </dl>
      </article>`;
  }).join("");
}

async function loadPets() {
  try {
    const mascotas = await apiRequest("mascotas");
    petSelect.innerHTML = [
      '<option value="">Selecciona una mascota</option>',
      ...mascotas.map((mascota) =>
        `<option value="${mascota.id}">${escapeHtml(mascota.nombre)} · ${escapeHtml(mascota.especie)}</option>`,
      ),
    ].join("");
  } catch (error) {
    petSelect.innerHTML = '<option value="">No se pudieron cargar</option>';
    setStatus(message, `No se pudieron cargar las mascotas: ${error.message}`, true);
  }
}

async function loadHistorial() {
  const mascotaId = petSelect.value;

  if (!mascotaId) {
    setStatus(message, "Selecciona una mascota para continuar.", true);
    list.replaceChildren();
    return;
  }

  searchButton.disabled = true;
  setStatus(message, "Cargando historial...");

  try {
    const consultas = await apiRequest(`mascotas/${mascotaId}/historial`);
    const count = consultas.length;
    setStatus(
      message,
      `${count} ${count === 1 ? "atención encontrada." : "atenciones encontradas."}`,
    );
    renderHistorial(consultas);
  } catch (error) {
    setStatus(message, `No se pudo consultar el historial: ${error.message}`, true);
  } finally {
    searchButton.disabled = false;
  }
}

searchButton.addEventListener("click", loadHistorial);
loadPets();
