document.addEventListener('DOMContentLoaded', () => {

    const tbody       = document.getElementById('booksBody');
    const statusEl    = document.getElementById('status');
    const searchInput = document.getElementById('searchInput');
    const searchBtn   = document.getElementById('searchBtn');
    const clearBtn    = document.getElementById('clearBtn');

    // ─── Загрузка списка ───────────────────────────────
    async function loadBooks(search = '') {
        setStatus(search ? `Поиск: «${search}»…` : 'Загрузка…', 'loading');
        try {
            const books = await Api.list(search);
            renderBooks(books);
            setStatus(
                books.length
                    ? `Найдено книг: ${books.length}`
                    : 'Ничего не найдено',
                books.length ? 'success' : ''
            );
        } catch (err) {
            setStatus('Ошибка загрузки: ' + err.message, 'error');
            tbody.innerHTML = '';
        }
    }

    // ─── Отрисовка таблицы ─────────────────────────────
    function renderBooks(books) {
        if (!books || books.length === 0) {
            tbody.innerHTML = `
                <tr class="empty-row">
                    <td colspan="5">Книг пока нет. Добавьте первую!</td>
                </tr>`;
            return;
        }

        tbody.innerHTML = books.map(b => `
            <tr>
                <td>${b.id}</td>
                <td>${escapeHtml(b.title)}</td>
                <td>${escapeHtml(b.author)}</td>
                <td>${b.publishYear ?? '—'}</td>
                <td>
                    <a class="btn btn-small" href="/book.html?id=${b.id}">Открыть</a>
                    <button class="btn btn-small btn-danger"
                            data-delete="${b.id}"
                            data-title="${escapeHtml(b.title)}">Удалить</button>
                </td>
            </tr>
        `).join('');

        // Обработчики удаления
        tbody.querySelectorAll('[data-delete]').forEach(btn => {
            btn.addEventListener('click', onDelete);
        });
    }

    // ─── Удаление книги ────────────────────────────────
    async function onDelete(e) {
        const id    = e.target.dataset.delete;
        const title = e.target.dataset.title;

        if (!confirm(`Удалить книгу «${title}»?`)) return;

        try {
            await Api.remove(id);
            setStatus(`Книга «${title}» удалена`, 'success');
            loadBooks(searchInput.value.trim());
        } catch (err) {
            setStatus('Ошибка удаления: ' + err.message, 'error');
        }
    }

    // ─── Вспомогательные ───────────────────────────────
    function setStatus(text, cls = '') {
        statusEl.textContent = text;
        statusEl.className = 'status ' + cls;
    }

    function escapeHtml(str) {
        return String(str ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // ─── События ───────────────────────────────────────
    searchBtn.addEventListener('click', () => {
        loadBooks(searchInput.value.trim());
    });

    searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            loadBooks(searchInput.value.trim());
        }
    });

    clearBtn.addEventListener('click', () => {
        searchInput.value = '';
        loadBooks();
    });

    // ─── Старт ─────────────────────────────────────────
    loadBooks();
});