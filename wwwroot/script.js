let clients = [];
let sortKey = 'fullName';
let sortAsc = true;
let editingClient = null;
let credentials = null;

const API_URL = '/api/clients';

document.addEventListener('DOMContentLoaded', () => {
  const loginForm = document.getElementById('loginForm');
  const clientForm = document.getElementById('clientForm');
  const searchInput = document.getElementById('search');

  if (loginForm) loginForm.addEventListener('submit', handleLogin);
  if (clientForm) clientForm.addEventListener('submit', saveClient);
  if (searchInput) searchInput.addEventListener('input', renderTable);
});

function toggleTheme() {
  document.body.classList.toggle('dark');
}

function handleLogin(e) {
  e.preventDefault();
  const username = document.getElementById('loginUsername').value.trim();
  const password = document.getElementById('loginPassword').value.trim();

  if (!username || !password) return alert("Введите логин и пароль");

  credentials = {
    Authorization: 'Basic ' + btoa(`${username}:${password}`)
  };

  fetchClients()
    .then(() => {
      document.getElementById('loginSection')?.classList.add('hidden');
      document.getElementById('crmSection')?.classList.remove('hidden');
    })
    .catch(() => {
      document.getElementById('loginError')?.classList.remove('hidden');
    });
}

async function fetchClients() {
  const res = await fetch(API_URL, { headers: credentials });
  if (!res.ok) throw new Error('Unauthorized');
  clients = await res.json();
  renderTable();
}

function renderTable() {
  const search = document.getElementById('search')?.value.toLowerCase() || '';
  let filtered = clients.filter(c =>
    c.fullName.toLowerCase().includes(search) ||
    c.email.toLowerCase().includes(search)
  );

  filtered.sort((a, b) => {
    const aVal = a[sortKey]?.toString().toLowerCase() || '';
    const bVal = b[sortKey]?.toString().toLowerCase() || '';
    return sortAsc ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
  });

  const tbody = document.querySelector('#clientsTable tbody');
  tbody.innerHTML = '';
  filtered.forEach(c => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td>${c.fullName}</td>
      <td>${c.email}</td>
      <td>${c.phone}</td>
      <td>${c.company}</td>
      <td class="actions">
        <button onclick='editClient(${c.id})'>✏️</button>
        <button class="delete" onclick='deleteClient(${c.id})'>🗑️</button>
      </td>
    `;
    tbody.appendChild(row);
  });
}

function sortTable(key) {
  sortKey = key;
  sortAsc = !sortAsc;
  renderTable();
}

function openModal(client = null) {
  editingClient = client;
  const form = document.getElementById('clientForm');
  form.reset();

  if (client) {
    form.fullName.value = client.fullName;
    form.email.value = client.email;
    form.phone.value = client.phone;
    form.company.value = client.company;
    form.isActive.checked = client.isActive;
    document.getElementById('modalTitle').innerText = 'Edit Client';
  } else {
    document.getElementById('modalTitle').innerText = 'Add Client';
  }

  document.getElementById('modal').classList.remove('hidden');
}

function editClient(id) {
  const client = clients.find(c => c.id === id);
  if (client) openModal({ ...client });
}

function closeModal() {
  document.getElementById('modal')?.classList.add('hidden');
  editingClient = null;
}

async function saveClient(event) {
  event.preventDefault();
  const form = event.target;

  const client = {
    id: editingClient?.id || 0,
    fullName: form.fullName.value.trim(),
    email: form.email.value.trim(),
    phone: form.phone.value.trim(),
    company: form.company.value.trim(),
    isActive: form.isActive.checked
  };

  try {
    const isEdit = !!editingClient;
    const res = await fetch(
      isEdit ? `${API_URL}/${client.id}` : API_URL,
      {
        method: isEdit ? 'PUT' : 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...credentials
        },
        body: JSON.stringify(client)
      }
    );

    const text = await res.text();
    if (!res.ok) {
      console.error('❌ Ошибка от сервера:', text);
      throw new Error('Failed to save client');
    }

    const data = text ? JSON.parse(text) : client;

    if (isEdit) {
      const idx = clients.findIndex(c => c.id === client.id);
      if (idx !== -1) clients[idx] = data;
    } else {
      clients.push(data);
    }

    closeModal();
    renderTable();
  } catch (err) {
    alert('Ошибка при сохранении клиента. См. консоль.');
    console.error(err);
  }
}

async function deleteClient(id) {
  if (!confirm('Удалить этого клиента?')) return;

  try {
    const res = await fetch(`${API_URL}/${id}`, {
      method: 'DELETE',
      headers: credentials
    });
    if (!res.ok) throw new Error('Failed to delete');
    clients = clients.filter(c => c.id !== id);
    renderTable();
  } catch (err) {
    alert('Ошибка при удалении клиента');
    console.error(err);
  }
}

function exportPdf() {
  fetch('/api/clients/export/pdf', {
    method: 'GET',
    headers: credentials
  })
    .then(async res => {
      if (!res.ok) {
        const errText = await res.text();
        console.error('❌ Ошибка при экспорте:', errText);
        throw new Error('Export failed');
      }

      const blob = await res.blob();
      const contentDisposition = res.headers.get('Content-Disposition');
      const match = contentDisposition?.match(/filename="?(.+)"?/);
      const filename = match ? match[1] : 'clients.pdf';

      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(link.href);
    })
    .catch(err => {
      alert('❌ Не удалось скачать файл.');
      console.error(err);
    });
}