const createForm = document.querySelector("#create-form");
const titleInput = document.querySelector("#new-title");
const filterSelect = document.querySelector("#filter");
const taskList = document.querySelector("#task-list");
const emptyState = document.querySelector("#empty-state");
const message = document.querySelector("#message");

let currentFilter = "All";

async function request(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: options.body ? { "Content-Type": "application/json" } : options.headers
  });
  if (response.ok) return response.status === 204 ? null : response.json();

  let detail = "Something went wrong. Please try again.";
  try {
    const problem = await response.json();
    detail = problem.detail || problem.title || detail;
  } catch { }
  throw new Error(detail);
}

function showError(error) { message.textContent = error.message; }

function makeButton(label, action) {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = label;
  button.addEventListener("click", action);
  return button;
}

function renderTask(task) {
  const item = document.createElement("li");
  item.className = `task-row${task.status === "Completed" ? " completed" : ""}`;

  const content = document.createElement("div");
  content.className = "task-content";
  const checkbox = document.createElement("input");
  checkbox.type = "checkbox";
  checkbox.checked = task.status === "Completed";
  checkbox.setAttribute("aria-label", `Mark ${task.title} ${checkbox.checked ? "pending" : "completed"}`);
  checkbox.addEventListener("change", async () => {
    try {
      await request(`/api/tasks/${task.id}/status`, { method: "PUT", body: JSON.stringify({ status: checkbox.checked ? "Completed" : "Pending" }) });
      await loadTasks();
    } catch (error) { showError(error); }
  });
  const title = document.createElement("span");
  title.className = "task-title";
  title.textContent = task.title;
  content.append(checkbox, title);

  const actions = document.createElement("div");
  actions.className = "task-actions";
  actions.append(
    makeButton("Edit", () => editTask(item, task)),
    makeButton("Delete", async () => {
      if (!window.confirm(`Delete “${task.title}”?`)) return;
      try {
        await request(`/api/tasks/${task.id}`, { method: "DELETE" });
        await loadTasks();
      } catch (error) { showError(error); }
    })
  );
  item.append(content, actions);
  return item;
}

function editTask(item, task) {
  const input = document.createElement("input");
  input.className = "edit-input";
  input.type = "text";
  input.maxLength = 255;
  input.value = task.title;
  input.setAttribute("aria-label", "Task title");
  const save = makeButton("Save", async () => {
    try {
      await request(`/api/tasks/${task.id}`, { method: "PATCH", body: JSON.stringify({ title: input.value }) });
      await loadTasks();
    } catch (error) { showError(error); }
  });
  const cancel = makeButton("Cancel", loadTasks);
  const editor = document.createElement("div");
  editor.className = "task-row";
  editor.append(input, save, cancel);
  item.replaceWith(editor);
  input.focus();
}

async function loadTasks() {
  message.textContent = "";
  try {
    const tasks = await request(`/api/tasks?status=${currentFilter}`);
    taskList.replaceChildren(...tasks.map(renderTask));
    emptyState.hidden = tasks.length !== 0;
  } catch (error) { showError(error); }
}

createForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const title = titleInput.value.trim();
  if (!title || title.length > 255) {
    message.textContent = "Title must contain 1 to 255 characters.";
    return;
  }
  try {
    await request("/api/tasks", { method: "POST", body: JSON.stringify({ title }) });
    titleInput.value = "";
    await loadTasks();
  } catch (error) { showError(error); }
});

filterSelect.addEventListener("change", () => {
  currentFilter = filterSelect.value;
  loadTasks();
});

loadTasks();
