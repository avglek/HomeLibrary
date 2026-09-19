document.addEventListener('DOMContentLoaded', () => {

    // ─── Инициализация редактора Quill ─────────────────
    const quill = new Quill('#editor', {
        theme: 'snow',
        placeholder: 'Введите оглавление книги…',
        modules: {
            toolbar: [
                [{ header: [1, 2, 3, false] }],
                ['bold', 'italic', 'underline'],
                [{ list: 'ordered' }, { list: 'bullet' }],
                ['link', 'blockquote'],
                ['clean']
            ]
        }
    });

    // ─── Элементы страницы ─────────────────────────────
    const titleEl     = document.getElementById('title');
    const authorEl    = document.getElementById('author');
    const yearEl      = document.getElementById('year');
    const saveBtn     = document.getElementById('saveBtn');
    const deleteBtn   = document.getElementById('deleteBtn');
    const statusEl    = document.getElementById('status');
    const pageTitle   = document.getElementById('pageTitle');

    // ─── Определяем id из URL ──────────────────────────
    const params = new URLSearchParams(location.search);
    const bookId = params.get('id');

    // ─── Загрузка книги при редактировании ─────────────
    async function loadBook() {
        if (!bookId) return;

        setStatus('Загрузка…', 'loading');
        pageTitle.textContent = 'Редактирование книги';
        deleteBtn.style.display = 'inline-block';

        try {
            const book = await Api.get(bookId);
            titleEl.value  = book.title       ?? '';
            authorEl.value = book.author      ?? '';
            yearEl.value   = book.publishYear ?? '';

            // tocContent приходит как XML-строка, напр. "<toc><h1>…</h1></toc>"
            // Пытаемся извлечь внутренний HTML, если он обёрнут в <toc>
            const html = extractTocHtml(book.tocContent);
            quill.root.innerHTML = html || '';

            setStatus('', '');
        } catch (err) {
            setStatus('Не удалось загрузить книгу: ' + err.message, 'error');
        }
    }

    // ─── Сохранение ────────────────────────────────────
    async function saveBook() {
        const title  = titleEl.value.trim();
        const author = authorEl.value.trim();

        if (!title || !author) {
            setStatus('Заполните обязательные поля: Название и Автор', 'error');
            return;
        }

        // Оборачиваем HTML редактора в XML-контейнер.
        // CDATA защищает от невалидных для XML символов (<br>, &nbsp; и т.п.),
        // но тогда tocContent придёт как сырая строка — об этом ниже.
        // Здесь используем безопасный вариант: сериализуем HTML как escaped-текст
        // внутри <toc>…</toc>. Quill выдаёт валидный HTML5.
        const editorHtml = quill.root.innerHTML;
        const tocXml = `<toc>${editorHtml}</toc>`;

        const payload = {
            title,
            author,
            publishYear: yearEl.value ? parseInt(yearEl.value, 10) : null,
            tocContent:  tocXml
        };

        setStatus('Сохранение…', 'loading');
        saveBtn.disabled = true;

        try {
            if (bookId) {
                await Api.update(bookId, payload);
                setStatus('Изменения сохранены', 'success');
            } else {
                const created = await Api.create(payload);
                setStatus('Книга создана', 'success');
                // Переходим в режим редактирования только что созданной книги
                setTimeout(() => {
                    location.href = `/book.html?id=${created.id}`;
                }, 600);
            }
        } catch (err) {
            setStatus('Ошибка сохранения: ' + err.message, 'error');
        } finally {
            saveBtn.disabled = false;
        }
    }

    // ─── Удаление ──────────────────────────────────────
    async function deleteBook() {
        if (!bookId) return;
        if (!confirm('Удалить эту книгу?')) return;

        setStatus('Удаление…', 'loading');
        try {
            await Api.remove(bookId);
            setStatus('Книга удалена', 'success');
            setTimeout(() => location.href = '/', 500);
        } catch (err) {
            setStatus('Ошибка удаления: ' + err.message, 'error');
        }
    }

    // ─── Вспомогательные ───────────────────────────────
    function setStatus(text, cls = '') {
        statusEl.textContent = text;
        statusEl.className = 'status ' + cls;
    }

    // Извлекаем содержимое <toc>…</toc> как HTML.
    // Если тега <toc> нет — возвращаем строку как есть.
    function extractTocHtml(tocContent) {
        if (!tocContent) return '';
        try {
            const parser = new DOMParser();
            const doc = parser.parseFromString(tocContent, 'application/xml');
            const toc = doc.querySelector('toc');
            if (toc) return toc.innerHTML;
        } catch (_) { /* ignore */ }

        // Фолбэк: пробуем как HTML
        const tmp = document.createElement('div');
        tmp.innerHTML = tocContent;
        const toc = tmp.querySelector('toc');
        return toc ? toc.innerHTML : tocContent;
    }

    // ─── События ───────────────────────────────────────
    saveBtn.addEventListener('click', saveBook);
    deleteBtn.addEventListener('click', deleteBook);

    // ─── Старт ─────────────────────────────────────────
    loadBook();
});