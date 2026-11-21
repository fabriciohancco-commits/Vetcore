import { apiRequest, escapeHtml, setStatus } from "./api.js";

const medicineForm = document.getElementById("medicine-form");
const medicineList = document.getElementById("medicine-list");
const medicineSelect = document.getElementById("prescription-medicine");
const petSelect = document.getElementById("prescription-pet");
const prescriptionForm = document.getElementById("prescription-form");
const prescriptionList = document.getElementById("prescription-list");
const rxNumberField = document.getElementById("prescription-number-field");
const rxNumber = document.getElementById("prescription-number");
let medicines = [];

function renderMedicines() {
  document.getElementById("medicine-count").textContent = `${medicines.length} productos`;
  medicineList.innerHTML = medicines.length ? medicines.map((item) => `
    <article class="card medicine-card">
      <div class="card-content"><h3>${escapeHtml(item.nombre)}</h3>
        <p>${escapeHtml(item.principioActivo || "Principio activo no especificado")} · ${escapeHtml(item.presentacion)}</p>
        <div class="medicine-meta"><span class="species-badge ${item.requiereReceta ? "rx-required" : "rx-free"}">${item.requiereReceta ? "Con receta" : "Sin receta"}</span>
        <span class="stock-badge ${item.stockUnidades <= item.stockMinimo ? "stock-low" : ""}">${item.stockUnidades} unidades${item.stockUnidades <= item.stockMinimo ? " · Reponer" : ""}</span></div>
      </div></article>`).join("") : '<div class="empty">Aún no hay medicamentos en el inventario.</div>';
  medicineSelect.innerHTML = ['<option value="">Selecciona del inventario</option>', ...medicines.filter((m) => m.activo).map((m) =>
    `<option value="${m.id}" data-rx="${m.requiereReceta}">${escapeHtml(m.nombre)} · ${m.requiereReceta ? "con receta" : "sin receta"}</option>`) ].join("");
}

function updatePrescriptionRequirement() {
  const selected = medicineSelect.selectedOptions[0];
  const required = selected?.dataset.rx === "true";
  rxNumberField.classList.toggle("hidden", !required);
  rxNumber.required = required;
  if (!required) rxNumber.value = "";
}

async function loadMedicines() {
  medicines = await apiRequest("farmacia/medicamentos");
  renderMedicines();
}

async function loadPets() {
  const pets = await apiRequest("mascotas");
  petSelect.innerHTML = ['<option value="">Selecciona una mascota</option>', ...pets.map((pet) =>
    `<option value="${pet.id}">${escapeHtml(pet.nombre)} · ${escapeHtml(pet.especie)}</option>`) ].join("");
}

async function loadPrescriptions() {
  const items = await apiRequest("farmacia/prescripciones");
  prescriptionList.innerHTML = items.length ? items.map((item) => `
    <article class="record-card"><div class="record-header"><div><span class="record-label">${escapeHtml(item.mascotaNombre)} · ${escapeHtml(item.veterinarioNombre)}</span><h3>${escapeHtml(item.medicamentoNombre)}</h3></div>
    <time class="record-date">${new Date(item.fechaRegistro).toLocaleDateString("es-PE")}</time></div>
    <dl class="record-details"><div><dt>Indicaciones registradas</dt><dd>${escapeHtml(item.indicaciones)}</dd></div>
    ${item.requiereReceta ? `<div><dt>N.º de receta</dt><dd>${escapeHtml(item.numeroReceta)}</dd></div>` : ""}
    <div><dt>Dosis indicada</dt><dd>${escapeHtml(item.dosis)}</dd></div>
    <div><dt>Unidades entregadas</dt><dd>${escapeHtml(item.unidadesEntregadas)}</dd></div>
    <div><dt>Frecuencia</dt><dd>${escapeHtml(item.frecuencia)}</dd></div>
    <div><dt>Duración</dt><dd>${escapeHtml(item.duracion)}</dd></div>
    ${item.notas ? `<div><dt>Seguimiento</dt><dd>${escapeHtml(item.notas)}</dd></div>` : ""}</dl></article>`).join("") : '<div class="empty">Todavía no hay indicaciones registradas.</div>';
}

medicineForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const submit = medicineForm.querySelector("button[type=submit]");
  submit.disabled = true;
  setStatus(document.getElementById("medicine-notice"), "Guardando medicamento…");
  const dateValue = document.getElementById("medicine-expiry").value;
  const payload = {
    nombre: document.getElementById("medicine-name").value.trim(),
    principioActivo: document.getElementById("medicine-active").value.trim() || null,
    presentacion: document.getElementById("medicine-presentation").value.trim(),
    requiereReceta: document.getElementById("medicine-rx").checked,
    stockUnidades: Number(document.getElementById("medicine-stock").value),
    stockMinimo: Number(document.getElementById("medicine-minimum").value),
    fechaVencimiento: dateValue ? new Date(`${dateValue}T12:00:00`).toISOString() : null,
  };
  try {
    await apiRequest("farmacia/medicamentos", { method: "POST", body: JSON.stringify(payload) });
    medicineForm.reset();
    document.getElementById("medicine-stock").value = "0";
    document.getElementById("medicine-minimum").value = "5";
    await loadMedicines();
    setStatus(document.getElementById("medicine-notice"), "Medicamento agregado al inventario.");
  } catch (error) { setStatus(document.getElementById("medicine-notice"), error.message, true); }
  finally { submit.disabled = false; }
});

medicineSelect.addEventListener("change", updatePrescriptionRequirement);
prescriptionForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const submit = prescriptionForm.querySelector("button[type=submit]");
  submit.disabled = true;
  setStatus(document.getElementById("prescription-notice"), "Guardando indicación…");
  const payload = {
    mascotaId: Number(petSelect.value),
    medicamentoId: Number(medicineSelect.value),
    unidadesEntregadas: Number(document.getElementById("prescription-units").value),
    veterinarioNombre: document.getElementById("prescription-vet").value.trim(),
    numeroReceta: rxNumber.value.trim() || null,
    dosis: document.getElementById("prescription-dose").value.trim(),
    frecuencia: document.getElementById("prescription-frequency").value.trim(),
    duracion: document.getElementById("prescription-duration").value.trim(),
    indicaciones: document.getElementById("prescription-directions").value.trim(),
    notas: document.getElementById("prescription-notes").value.trim() || null,
  };
  try {
    await apiRequest("farmacia/prescripciones", { method: "POST", body: JSON.stringify(payload) });
    prescriptionForm.reset();
    updatePrescriptionRequirement();
    await loadPrescriptions();
    setStatus(document.getElementById("prescription-notice"), "Indicaciones veterinarias guardadas.");
  } catch (error) { setStatus(document.getElementById("prescription-notice"), error.message, true); }
  finally { submit.disabled = false; }
});

document.getElementById("reload-prescriptions").addEventListener("click", () => loadPrescriptions().catch((error) =>
  setStatus(document.getElementById("prescription-notice"), error.message, true)));

Promise.all([loadMedicines(), loadPets(), loadPrescriptions()]).catch((error) => {
  setStatus(document.getElementById("medicine-notice"), `No se pudo cargar farmacia: ${error.message}`, true);
});
