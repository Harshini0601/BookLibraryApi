const apiBase = "/api";

const state = {
	token: localStorage.getItem("token") || null,
	userDisplay: localStorage.getItem("userDisplay") || "",
};

const els = {
	navBooks: document.getElementById("nav-books"),
	navAuth: document.getElementById("nav-auth"),
	viewBooks: document.getElementById("view-books"),
	viewAuth: document.getElementById("view-auth"),
	booksGrid: document.getElementById("books-grid"),
	filterGenre: document.getElementById("filter-genre"),
	filterStatus: document.getElementById("filter-status"),
	btnRefresh: document.getElementById("btn-refresh"),
	createCard: document.getElementById("create-card"),
	formCreate: document.getElementById("form-create"),
	formRegister: document.getElementById("form-register"),
	formLogin: document.getElementById("form-login"),
	btnLogout: document.getElementById("btn-logout"),
	authUser: document.getElementById("auth-user"),
	toast: document.getElementById("toast"),
};

function showToast(message, kind = "") {
	els.toast.textContent = message;
	els.toast.hidden = false;
	els.toast.className = `toast ${kind}`.trim();
	setTimeout(() => (els.toast.hidden = true), 2500);
}

function setActiveNav(target) {
	[els.navBooks, els.navAuth].forEach((b) => b.classList.remove("is-active"));
	target.classList.add("is-active");
}

function goto(view) {
	const isBooks = view === "books";
	els.viewBooks.hidden = !isBooks;
	els.viewAuth.hidden = isBooks;
	setActiveNav(isBooks ? els.navBooks : els.navAuth);
}

function authHeaders() {
	return state.token ? { Authorization: `Bearer ${state.token}` } : {};
}

async function fetchJSON(url, options = {}) {
	const res = await fetch(url, {
		...options,
		headers: {
			"Content-Type": "application/json",
			...authHeaders(),
			...(options.headers || {}),
		},
	});
	if (!res.ok) {
		const text = await res.text();
		throw new Error(text || `HTTP ${res.status}`);
	}
	const ct = res.headers.get("content-type") || "";
	return ct.includes("application/json") ? res.json() : null;
}

function updateAuthUI() {
	const isAuthed = !!state.token;
	els.btnLogout.hidden = !isAuthed;
	els.authUser.textContent = isAuthed ? state.userDisplay : "Guest";
	els.createCard.hidden = !isAuthed;
}

async function loadBooks() {
	const genre = els.filterGenre.value.trim();
	const status = els.filterStatus.value.trim();
	const params = new URLSearchParams();
	if (genre) params.set("genre", genre);
	if (status) params.set("status", status);
	const list = await fetchJSON(`${apiBase}/books?${params.toString()}`);
	renderBooks(list);
}

function renderBooks(list) {
	els.booksGrid.innerHTML = "";
	list.forEach((b) => {
		const div = document.createElement("div");
		div.className = "book";
		const isAvailable = b.status === 0;
		div.innerHTML = `
			<h4>${escapeHtml(b.title)}</h4>
			<div class="muted">by ${escapeHtml(b.author)} · <span class="badge">${escapeHtml(b.genre || "-")}</span></div>
			<div>
				<span class="badge ${isAvailable ? "success" : "warn"}">${isAvailable ? "Available" : "Borrowed"}</span>
			</div>
			<div style="display:flex; gap:8px; margin-top:8px;">
				${state.token ? actionButtons(b) : ""}
			</div>
		`;
		els.booksGrid.appendChild(div);
	});
}

function actionButtons(b) {
	const id = b.id;
	return `
		<button class="btn ${b.status === 0 ? "success" : "danger"}" data-action="${b.status === 0 ? "borrow" : "return"}" data-id="${id}">${b.status === 0 ? "Borrow" : "Return"}</button>
		<button class="btn ghost" data-action="delete" data-id="${id}">Delete</button>
	`;
}

function escapeHtml(s) {
	return String(s).replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
}

// Events
els.navBooks.addEventListener("click", () => goto("books"));
els.navAuth.addEventListener("click", () => goto("auth"));
els.btnRefresh.addEventListener("click", () => loadBooks().catch((e) => showToast(e.message, "warn")));
els.filterGenre.addEventListener("change", () => loadBooks().catch((e) => showToast(e.message, "warn")));
els.filterStatus.addEventListener("change", () => loadBooks().catch((e) => showToast(e.message, "warn")));

els.formCreate.addEventListener("submit", async (e) => {
	e.preventDefault();
	const fd = new FormData(els.formCreate);
	const body = {
		title: fd.get("title"),
		author: fd.get("author"),
		genre: fd.get("genre"),
	};
	try {
		await fetchJSON(`${apiBase}/books`, { method: "POST", body: JSON.stringify(body) });
		els.formCreate.reset();
		showToast("Book created", "success");
		await loadBooks();
	} catch (err) { showToast(err.message, "warn"); }
});

els.booksGrid.addEventListener("click", async (e) => {
	const target = e.target;
	if (!(target instanceof HTMLElement)) return;
	const action = target.getAttribute("data-action");
	const id = target.getAttribute("data-id");
	if (!action || !id) return;
	try {
		if (action === "delete") {
			await fetchJSON(`${apiBase}/books/${id}`, { method: "DELETE" });
			showToast("Deleted", "success");
		} else if (action === "borrow") {
			await fetchJSON(`${apiBase}/books/${id}/borrow`, { method: "POST" });
			showToast("Borrowed", "success");
		} else if (action === "return") {
			await fetchJSON(`${apiBase}/books/${id}/return`, { method: "POST" });
			showToast("Returned", "success");
		}
		await loadBooks();
	} catch (err) { showToast(err.message, "warn"); }
});

els.formRegister.addEventListener("submit", async (e) => {
	e.preventDefault();
	const fd = new FormData(els.formRegister);
	const body = {
		username: fd.get("username"),
		email: fd.get("email"),
		password: fd.get("password"),
	};
	try {
		await fetchJSON(`${apiBase}/auth/register`, { method: "POST", body: JSON.stringify(body) });
		showToast("Registered. You can log in now.", "success");
		els.formRegister.reset();
		goto("auth");
	} catch (err) { showToast(err.message, "warn"); }
});

els.formLogin.addEventListener("submit", async (e) => {
	e.preventDefault();
	const fd = new FormData(els.formLogin);
	const body = {
		username: fd.get("username"),
		password: fd.get("password"),
	};
	try {
		const data = await fetchJSON(`${apiBase}/auth/login`, { method: "POST", body: JSON.stringify(body) });
		state.token = data.token;
		state.userDisplay = String(body.username);
		localStorage.setItem("token", state.token);
		localStorage.setItem("userDisplay", state.userDisplay);
		updateAuthUI();
		goto("books");
		await loadBooks();
		showToast("Logged in", "success");
	} catch (err) { showToast(err.message, "warn"); }
});

els.btnLogout.addEventListener("click", () => {
	state.token = null;
	state.userDisplay = "";
	localStorage.removeItem("token");
	localStorage.removeItem("userDisplay");
	updateAuthUI();
	showToast("Logged out");
});

// Init
updateAuthUI();
goto("books");
loadBooks().catch((e) => showToast(e.message, "warn")); 