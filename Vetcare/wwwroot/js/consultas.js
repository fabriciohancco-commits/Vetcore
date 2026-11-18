import { apiRequest, escapeHtml, setStatus } from "./api.js";

const message = document.getElementById("msg");
const list = document.getElementById("list");
const reloadButton = document.getElementById("reload");

function renderConsultas(consultas) {
  if (consultas.length === 0) {
    list.innerHTML = '<div class="empty">Todavía no hay consultas registradas.</div>';
    return;
  }

  list.innerHTML = consultas.map((consulta) => {
    const petName = consulta.mascota?.nombre || `Mascota #${consulta.mascotaId}`;
    const date = new Date(consulta.fechaConsulta).toLocaleString("es-PE");
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
            <h3>${escapeHtml(petName)}</h3>
          </div>
          <time class="record-date">${date}</time>
        </div>
        <dl class="record-details">
          <div><dt>Motivo</dt><dd>${escapeHtml(consulta.motivo)}</dd></div>
          <div><dt>Diagnóstico</dt><dd>${escapeHtml(consulta.diagnostico)}</dd></div>
          ${optionalDetails}
        </dl>
      </article>`;
  }).join("");
}

async function loadConsultas() {
  reloadButton.disabled = true;
  setStatus(message, "Cargando atenciones...");

  try {
    const consultas = await apiRequest("consultas");
    const count = consultas.length;
    setStatus(
      message,
      `${count} ${count === 1 ? "atención encontrada." : "atenciones encontradas."}`,
    );
    renderConsultas(consultas);
  } catch (error) {
    setStatus(message, `No se pudieron cargar las atenciones: ${error.message}`, true);
  } finally {
    reloadButton.disabled = false;
  }
}

reloadButton.addEventListener("click", loadConsultas);
loadConsultas();
