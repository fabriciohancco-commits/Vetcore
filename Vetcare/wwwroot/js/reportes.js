import { apiRequest, escapeHtml, setStatus } from "./api.js";

const fromInput = document.getElementById("desde");
const toInput = document.getElementById("hasta");
const veterinarianInput = document.getElementById("vet");
const filterForm = document.getElementById("report-filter-form");
const runButton = document.getElementById("run");
const pdfButton = document.getElementById("download-pdf");
const message = document.getElementById("msg");
const summary = document.getElementById("summary");
const list = document.getElementById("list");

function reportQuery() {
  const query = new URLSearchParams();
  if (fromInput.value) query.set("desde", fromInput.value);
  if (toInput.value) query.set("hasta", toInput.value);
  const veterinarian = veterinarianInput.value.trim();
  if (veterinarian) query.set("veterinario", veterinarian);
  return query;
}

function hasValidDateRange() {
  if (fromInput.value && toInput.value && fromInput.value > toInput.value) {
    setStatus(message, "La fecha inicial no puede ser posterior a la fecha final.", true);
    return false;
  }
  return true;
}

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
  if (!hasValidDateRange()) return;
  const query = reportQuery();

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

filterForm.addEventListener("submit", (event) => {
  event.preventDefault();
  loadReport();
});

pdfButton.addEventListener("click", async () => {
  if (!hasValidDateRange()) return;
  pdfButton.disabled = true;
  setStatus(message, "Preparando el PDF...");
  try {
    const response = await fetch(`/api/reportes/atenciones/pdf?${reportQuery()}`);
    if (!response.ok) {
      throw new Error(`La descarga falló (HTTP ${response.status}).`);
    }
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `vetcare-atenciones-${new Date().toISOString().slice(0, 10)}.pdf`;
    link.click();
    URL.revokeObjectURL(url);
    setStatus(message, "PDF descargado correctamente.");
  } catch (error) {
    setStatus(message, `No se pudo descargar el PDF: ${error.message}`, true);
  } finally {
    pdfButton.disabled = false;
  }
});

filterForm.addEventListener("reset", () => {
  window.requestAnimationFrame(() => {
    setStatus(message, "");
    summary.replaceChildren();
    list.replaceChildren();
  });
});
