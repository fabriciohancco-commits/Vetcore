import { apiRequest, escapeHtml, setStatus } from "./api.js";

const form = document.getElementById("operation-form");
const list = document.getElementById("operation-list");
const message = document.getElementById("operation-notice");
const petSelect = document.getElementById("operation-pet");
const states = ["Programada", "En curso", "Completada", "Cancelada"];

async function loadPets() {
  const pets = await apiRequest("mascotas");
  petSelect.innerHTML = ['<option value="">Selecciona una mascota</option>', ...pets.map((pet) =>
    `<option value="${pet.id}">${escapeHtml(pet.nombre)} · ${escapeHtml(pet.especie)}</option>`) ].join("");
}

async function loadOperations() {
  list.innerHTML = '<div class="loading-state"><span class="spinner" aria-hidden="true"></span>Cargando agenda…</div>';
  const operations = await apiRequest("operaciones");
  list.innerHTML = operations.length ? operations.map((item) => `
    <article class="record-card operation-card"><div class="record-header"><div><span class="record-label">${escapeHtml(item.mascotaNombre)} · ${escapeHtml(item.veterinarioNombre)}</span><h3>${escapeHtml(item.procedimiento)}</h3></div>
    <time class="record-date">${new Date(item.fechaProgramada).toLocaleString("es-PE", { dateStyle: "medium", timeStyle: "short" })}</time></div>
    ${item.notas ? `<p class="operation-notes">${escapeHtml(item.notas)}</p>` : ""}
    <div class="operation-controls"><label for="operation-state-${item.id}">Estado</label><select id="operation-state-${item.id}" data-operation-state="${item.id}">${states.map((state) => `<option ${item.estado === state ? "selected" : ""}>${state}</option>`).join("")}</select>
    <button class="small danger" type="button" data-delete-operation="${item.id}">Eliminar</button></div></article>`).join("") : '<div class="empty">No hay operaciones programadas.</div>';
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  const submit = form.querySelector("button[type=submit]");
  submit.disabled = true;
  setStatus(message, "Guardando operación…");
  const date = document.getElementById("operation-date").value;
  const payload = {
    mascotaId: Number(petSelect.value),
    procedimiento: document.getElementById("operation-type").value.trim(),
    veterinarioNombre: document.getElementById("operation-vet").value.trim(),
    fechaProgramada: new Date(date).toISOString(),
    estado: "Programada",
    notas: document.getElementById("operation-notes").value.trim() || null,
  };
  try {
    await apiRequest("operaciones", { method: "POST", body: JSON.stringify(payload) });
    form.reset();
    await loadOperations();
    setStatus(message, "Operación agregada a la agenda.");
  } catch (error) { setStatus(message, error.message, true); }
  finally { submit.disabled = false; }
});

list.addEventListener("change", async (event) => {
  const select = event.target.closest("[data-operation-state]");
  if (!select) return;
  select.disabled = true;
  try {
    await apiRequest(`operaciones/${select.dataset.operationState}/estado`, {
      method: "PUT", body: JSON.stringify({ estado: select.value }),
    });
    setStatus(message, "Estado de operación actualizado.");
  } catch (error) { setStatus(message, error.message, true); await loadOperations(); }
  finally { select.disabled = false; }
});

list.addEventListener("click", async (event) => {
  const button = event.target.closest("[data-delete-operation]");
  if (!button || !window.confirm("¿Eliminar esta operación de la agenda?")) return;
  button.disabled = true;
  try {
    await apiRequest(`operaciones/${button.dataset.deleteOperation}`, { method: "DELETE" });
    await loadOperations();
    setStatus(message, "Operación eliminada de la agenda.");
  } catch (error) { setStatus(message, error.message, true); button.disabled = false; }
});

document.getElementById("reload-operations").addEventListener("click", () => loadOperations().catch((error) =>
  setStatus(message, error.message, true)));
Promise.all([loadPets(), loadOperations()]).catch((error) => setStatus(message, error.message, true));
