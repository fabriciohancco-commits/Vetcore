import { apiRequest, escapeHtml, setStatus } from "./api.js";

const fromInput = document.getElementById("desde");
const toInput = document.getElementById("hasta");
const veterinarianInput = document.getElementById("vet");
const runButton = document.getElementById("run");
const message = document.getElementById("msg");
const summary = document.getElementById("summary");
const list = document.getElementById("list");

function renderReport(rows) {
  const total = rows.reduce((count, row) => count + Number(row.totalAtenciones), 0);
  summary.innerHTML = `
    <div class="report-total">
      <span>Total de atenciones</span>
      <strong>${total}</strong>
    </div>`;

  if (rows.length === 0) {
    list.innerHTML = '<div class="empty">No hay resultados para esos filtros.</div>';
    return;
  }

  const maximum = Math.max(...rows.map((row) => Number(row.totalAtenciones)));

  list.innerHTML = rows.map((row) => {
    const count = Number(row.totalAtenciones);
    const percentage = maximum === 0 ? 0 : Math.round((count / maximum) * 100);
    const label = count === 1 ? "atención" : "atenciones";
    const veterinarian = escapeHtml(row.veterinario);

    return `
      <article class="record-card report-row">
        <div class="report-row-heading">
          <h3>${veterinarian}</h3>
          <span class="record-date">${count} ${label}</span>
        </div>
        <div class="report-bar-track" role="meter" aria-label="Atenciones de ${veterinarian}"
             aria-valuemin="0" aria-valuemax="${maximum}" aria-valuenow="${count}">
          <span class="report-bar-fill" style="width:${percentage}%"></span>
        </div>
      </article>`;
  }).join("");
}

async function loadReport() {
  const from = fromInput.value;
  const to = toInput.value;

  if (from && to && from > to) {
    setStatus(message, "La fecha inicial no puede ser posterior a la fecha final.", true);
    return;
  }

  const query = new URLSearchParams();
  const veterinarian = veterinarianInput.value.trim();
  if (from) query.set("desde", from);
  if (to) query.set("hasta", to);
  if (veterinarian) query.set("veterinario", veterinarian);

  runButton.disabled = true;
  setStatus(message, "Generando reporte...");

  try {
    const rows = await apiRequest(`reportes/atenciones?${query}`);
    renderReport(rows);
    setStatus(message, "Reporte generado correctamente.");
  } catch (error) {
    setStatus(message, `No se pudo generar el reporte: ${error.message}`, true);
  } finally {
    runButton.disabled = false;
  }
}

runButton.addEventListener("click", loadReport);
