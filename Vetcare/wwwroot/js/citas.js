import { apiRequest, escapeHtml, setStatus } from "./api.js";

const form = document.getElementById("appointment-form");
const ownerSelect = document.getElementById("appointment-owner");
const petSelect = document.getElementById("appointment-pet");
const upcomingList = document.getElementById("upcoming-list");
const historyList = document.getElementById("history-list");
const notice = document.getElementById("appointment-notice");
const stateOptions = ["Programada", "Atendida", "No asistió", "Cancelada"];
let owners = [];
let pets = [];
let appointments = [];
let moveTimer;

function renderPetOptions() {
  const ownerId = Number(ownerSelect.value);
  const available = pets.filter((pet) => pet.propietarioId === ownerId);
  petSelect.innerHTML = available.length
    ? ['<option value="">Selecciona una mascota</option>', ...available.map((pet) =>
      `<option value="${pet.id}">${escapeHtml(pet.nombre)} · ${escapeHtml(pet.especie)}</option>`) ].join("")
    : '<option value="">Este dueño aún no tiene mascotas</option>';
}

function cardMarkup(cita, past) {
  const when = new Date(cita.fechaHora);
  const pendingClose = past && cita.estado === "Programada";
  const visibleState = pendingClose ? "Pendiente de cierre" : cita.estado;
  return `<article class="record-card appointment-card ${past ? "appointment-past" : "appointment-upcoming"}">
    <div class="record-header"><div><span class="record-label">${escapeHtml(cita.propietarioNombre)} · ${escapeHtml(cita.mascotaNombre)}</span><h3>${escapeHtml(cita.motivo)}</h3></div>
    <time class="record-date">${when.toLocaleString("es-PE", { dateStyle: "medium", timeStyle: "short" })}</time></div>
    <div class="appointment-meta"><span>Dr. ${escapeHtml(cita.veterinarioNombre)}</span><span>${cita.duracionMinutos} min</span><span class="appointment-status ${pendingClose ? "status-pending" : ""}">${escapeHtml(visibleState)}</span></div>
    <div class="appointment-controls"><label for="appointment-state-${cita.id}">Resultado</label><select id="appointment-state-${cita.id}" data-appointment-state="${cita.id}">${stateOptions.map((state) => `<option value="${escapeHtml(state)}" ${cita.estado === state ? "selected" : ""}>${escapeHtml(state)}</option>`).join("")}</select></div>
  </article>`;
}

function renderAppointments() {
  clearTimeout(moveTimer);
  const now = Date.now();
  const upcoming = appointments.filter((cita) => new Date(cita.fechaHora).getTime() > now && cita.estado === "Programada");
  const history = appointments.filter((cita) => !upcoming.includes(cita)).sort((a, b) => new Date(b.fechaHora) - new Date(a.fechaHora));
  document.getElementById("upcoming-count").textContent = String(upcoming.length);
  upcomingList.innerHTML = upcoming.length
    ? upcoming.map((cita) => cardMarkup(cita, false)).join("")
    : '<div class="empty">No hay próximas citas. Agenda una desde el formulario.</div>';
  historyList.innerHTML = history.length
    ? history.map((cita) => cardMarkup(cita, new Date(cita.fechaHora).getTime() <= now)).join("")
    : '<div class="empty">Todavía no hay citas anteriores.</div>';

  const nextTime = Math.min(...upcoming.map((cita) => new Date(cita.fechaHora).getTime()));
  if (Number.isFinite(nextTime)) moveTimer = setTimeout(renderAppointments, Math.max(1000, nextTime - Date.now() + 1000));
}

async function loadAppointments() {
  appointments = await apiRequest("citas");
  renderAppointments();
}

async function loadDirectory() {
  [owners, pets] = await Promise.all([apiRequest("propietarios"), apiRequest("mascotas")]);
  ownerSelect.innerHTML = ['<option value="">Selecciona un dueño</option>', ...owners.map((owner) =>
    `<option value="${owner.id}">${escapeHtml(owner.nombres)} ${escapeHtml(owner.apellidos)}</option>`) ].join("");
  renderPetOptions();
}

ownerSelect.addEventListener("change", renderPetOptions);
petSelect.addEventListener("change", () => {
  const pet = pets.find((item) => item.id === Number(petSelect.value));
  if (pet) ownerSelect.value = String(pet.propietarioId);
});

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  const submit = form.querySelector("button[type=submit]");
  submit.disabled = true;
  setStatus(notice, "Guardando cita…");
  const payload = {
    propietarioId: Number(ownerSelect.value),
    mascotaId: Number(petSelect.value),
    veterinarioNombre: document.getElementById("appointment-vet").value.trim(),
    fechaHora: document.getElementById("appointment-date").value,
    duracionMinutos: Number(document.getElementById("appointment-duration").value),
    motivo: document.getElementById("appointment-reason").value.trim(),
  };
  try {
    await apiRequest("citas", { method: "POST", body: JSON.stringify(payload) });
    form.reset();
    renderPetOptions();
    await loadAppointments();
    setStatus(notice, "Cita agendada correctamente.");
  } catch (error) { setStatus(notice, error.message, true); }
  finally { submit.disabled = false; }
});

document.addEventListener("change", async (event) => {
  const select = event.target.closest("[data-appointment-state]");
  if (!select) return;
  select.disabled = true;
  try {
    await apiRequest(`citas/${select.dataset.appointmentState}/estado`, {
      method: "PUT", body: JSON.stringify({ estado: select.value }),
    });
    await loadAppointments();
    setStatus(notice, "Resultado de la cita actualizado.");
  } catch (error) { setStatus(notice, error.message, true); await loadAppointments(); }
  finally { select.disabled = false; }
});

document.getElementById("reload-appointments").addEventListener("click", () => loadAppointments().catch((error) => setStatus(notice, error.message, true)));
Promise.all([loadDirectory(), loadAppointments()]).catch((error) => setStatus(notice, error.message, true));
