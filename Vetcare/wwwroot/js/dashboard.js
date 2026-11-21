import { apiRequest, escapeHtml, setStatus } from "./api.js";

const $ = (id) => document.getElementById(id);
const state = { propietarios: [], mascotas: [] };

function setNotice(id, message, isError = false) {
  setStatus($(id), message, isError);
}

function resetOwnerForm() {
  $("owner-form").reset();
  $("owner-id").value = "";
  $("owner-title").textContent = "Nuevo propietario";
  $("owner-cancel").classList.add("hidden");
}

function resetPetForm() {
  $("pet-form").reset();
  $("pet-id").value = "";
  $("pet-title").textContent = "Registrar mascota";
  $("pet-cancel").classList.add("hidden");
}

function ownerName(ownerId) {
  const owner = state.propietarios.find((item) => item.id === ownerId);
  return owner
    ? `${owner.nombres} ${owner.apellidos}`
    : "Propietario no disponible";
}

function renderOwners() {
  const owners = state.propietarios;
  $("owner-count").textContent = `${owners.length} ${owners.length === 1 ? "registro" : "registros"}`;
  $("summary-owners").textContent = owners.length;

  if (owners.length === 0) {
    $("owner-list").innerHTML = '<div class="empty">Aún no hay propietarios. Registra el primero aquí.</div>';
    return;
  }

  $("owner-list").innerHTML = owners.map((owner) => `
    <article class="card">
      <div class="card-content">
        <h3>${escapeHtml(owner.nombres)} ${escapeHtml(owner.apellidos)}</h3>
        <div class="contact-lines">
          <a href="tel:${encodeURIComponent(owner.telefono)}">${escapeHtml(owner.telefono)}</a>
          ${owner.correo ? `<a href="mailto:${encodeURIComponent(owner.correo)}">${escapeHtml(owner.correo)}</a>` : ""}
        </div>
        <p class="card-subtext">${escapeHtml(owner.direccion || "Sin dirección registrada")}</p>
      </div>
      <div class="card-actions">
        <button class="small" data-edit-owner="${owner.id}" aria-label="Editar a ${escapeHtml(owner.nombres)}" type="button">Editar</button>
        <button class="small danger" data-delete-owner="${owner.id}" aria-label="Eliminar a ${escapeHtml(owner.nombres)}" type="button">Eliminar</button>
      </div>
    </article>`).join("");
}

function renderPets() {
  const pets = state.mascotas;
  $("pet-count").textContent = `${pets.length} ${pets.length === 1 ? "registro" : "registros"}`;
  $("summary-pets").textContent = pets.length;

  if (pets.length === 0) {
    $("pet-list").innerHTML = '<div class="empty">Aún no hay mascotas. Registra una cuando tengas un propietario.</div>';
    return;
  }

  $("pet-list").innerHTML = pets.map((pet) => `
    <article class="card">
      <div>
        <h3>${escapeHtml(pet.nombre)} <span class="pet-species">· ${escapeHtml(pet.especie)}</span></h3>
        <p>${escapeHtml(pet.raza || "Raza no especificada")}${pet.pesoKg ? ` · ${escapeHtml(pet.pesoKg)} kg` : ""}<br>
          Tutor: ${escapeHtml(ownerName(pet.propietarioId))}${pet.observaciones ? `<br>${escapeHtml(pet.observaciones)}` : ""}</p>
      </div>
      <div class="card-actions">
        <button class="small" data-edit-pet="${pet.id}" type="button">Editar</button>
        <button class="small danger" data-delete-pet="${pet.id}" type="button">Eliminar</button>
      </div>
    </article>`).join("");
}

function fillOwnerOptions(selectedId = "") {
  const options = state.propietarios.map((owner) => `
    <option value="${owner.id}" ${String(owner.id) === String(selectedId) ? "selected" : ""}>
      ${escapeHtml(owner.apellidos)}, ${escapeHtml(owner.nombres)}
    </option>`).join("");

  $("pet-owner").innerHTML = `<option value="">Seleccionar propietario</option>${options}`;
}

async function refreshDashboard() {
  const ownerQuery = new URLSearchParams({ search: $("owner-filter").value.trim() });
  const petQuery = new URLSearchParams({ search: $("pet-filter").value.trim() });

  try {
    [state.propietarios, state.mascotas] = await Promise.all([
      apiRequest(`propietarios?${ownerQuery}`),
      apiRequest(`mascotas?${petQuery}`),
    ]);
    renderOwners();
    renderPets();
    fillOwnerOptions($("pet-owner").value);
  } catch (error) {
    setNotice("owner-notice", error.message, true);
  }
}

function ownerPayload() {
  return {
    nombres: $("owner-first").value.trim(),
    apellidos: $("owner-last").value.trim(),
    telefono: $("owner-phone").value.trim(),
    correo: $("owner-email").value.trim() || null,
    direccion: $("owner-address").value.trim() || null,
  };
}

function petPayload() {
  return {
    nombre: $("pet-name").value.trim(),
    especie: $("pet-species").value,
    raza: $("pet-breed").value.trim() || null,
    fechaNacimiento: $("pet-birth").value || null,
    pesoKg: $("pet-weight").value ? Number($("pet-weight").value) : null,
    observaciones: $("pet-notes").value.trim() || null,
    propietarioId: Number($("pet-owner").value),
  };
}

async function saveOwner(event) {
  event.preventDefault();
  const id = $("owner-id").value;

  try {
    await apiRequest(id ? `propietarios/${id}` : "propietarios", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(ownerPayload()),
    });
    resetOwnerForm();
    setNotice("owner-notice", "Propietario guardado correctamente.");
    await refreshDashboard();
  } catch (error) {
    setNotice("owner-notice", error.message, true);
  }
}

async function savePet(event) {
  event.preventDefault();
  const id = $("pet-id").value;

  try {
    await apiRequest(id ? `mascotas/${id}` : "mascotas", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(petPayload()),
    });
    resetPetForm();
    setNotice("pet-notice", "Mascota guardada correctamente.");
    await refreshDashboard();
  } catch (error) {
    setNotice("pet-notice", error.message, true);
  }
}

function editOwner(ownerId) {
  const owner = state.propietarios.find((item) => item.id === ownerId);
  if (!owner) return;

  $("owner-id").value = owner.id;
  $("owner-first").value = owner.nombres;
  $("owner-last").value = owner.apellidos;
  $("owner-phone").value = owner.telefono;
  $("owner-email").value = owner.correo || "";
  $("owner-address").value = owner.direccion || "";
  $("owner-title").textContent = "Editar propietario";
  $("owner-cancel").classList.remove("hidden");
  $("owner-first").focus();
}

function editPet(petId) {
  const pet = state.mascotas.find((item) => item.id === petId);
  if (!pet) return;

  $("pet-id").value = pet.id;
  $("pet-name").value = pet.nombre;
  $("pet-species").value = pet.especie;
  $("pet-breed").value = pet.raza || "";
  $("pet-birth").value = pet.fechaNacimiento || "";
  $("pet-weight").value = pet.pesoKg || "";
  $("pet-notes").value = pet.observaciones || "";
  fillOwnerOptions(pet.propietarioId);
  $("pet-title").textContent = "Editar mascota";
  $("pet-cancel").classList.remove("hidden");
  $("pet-name").focus();
}

async function deleteRecord(path, label, messageId) {
  if (!window.confirm(`¿Eliminar ${label}?`)) return;

  try {
    await apiRequest(path, { method: "DELETE" });
    setNotice(messageId, "Registro eliminado.");
    await refreshDashboard();
  } catch (error) {
    setNotice(messageId, error.message, true);
  }
}

$("owner-form").addEventListener("submit", saveOwner);
$("pet-form").addEventListener("submit", savePet);
$("owner-cancel").addEventListener("click", resetOwnerForm);
$("pet-cancel").addEventListener("click", resetPetForm);
$("owner-search-form").addEventListener("submit", (event) => {
  event.preventDefault();
  refreshDashboard();
});
$("pet-search-form").addEventListener("submit", (event) => {
  event.preventDefault();
  refreshDashboard();
});

$("owner-list").addEventListener("click", (event) => {
  const editButton = event.target.closest("[data-edit-owner]");
  const deleteButton = event.target.closest("[data-delete-owner]");

  if (editButton) editOwner(Number(editButton.dataset.editOwner));
  if (deleteButton) {
    const owner = state.propietarios.find((item) => item.id === Number(deleteButton.dataset.deleteOwner));
    if (owner) {
      deleteRecord(`propietarios/${owner.id}`, `${owner.nombres} ${owner.apellidos}`, "owner-notice");
    }
  }
});

$("pet-list").addEventListener("click", (event) => {
  const editButton = event.target.closest("[data-edit-pet]");
  const deleteButton = event.target.closest("[data-delete-pet]");

  if (editButton) editPet(Number(editButton.dataset.editPet));
  if (deleteButton) {
    const pet = state.mascotas.find((item) => item.id === Number(deleteButton.dataset.deletePet));
    if (pet) deleteRecord(`mascotas/${pet.id}`, pet.nombre, "pet-notice");
  }
});

document.querySelectorAll("button.tab[data-tab]").forEach((button) => {
  button.addEventListener("click", () => {
    document.querySelectorAll("button.tab[data-tab]").forEach((tab) => {
      tab.classList.toggle("active", tab === button);
    });
    document.querySelectorAll(".layout").forEach((section) => {
      section.classList.toggle("hidden", section.id !== button.dataset.tab);
    });
  });
});

refreshDashboard();
