// Обёртки над fetch для всех обращений к /api/books
const Api = (() => {
    const BASE = '/api/books';

    async function handle(res) {
        if (res.status === 204) return null;
        const text = await res.text();
        const data = text ? JSON.parse(text) : null;
        if (!res.ok) {
            const message = data?.message || `Ошибка ${res.status}`;
            throw new Error(message);
        }
        return data;
    }

    return {
        // GET /api/books  или  /api/books?search=...
        list: (search = '') => {
            const url = search
                ? `${BASE}?search=${encodeURIComponent(search)}`
                : BASE;
            return fetch(url).then(handle);
        },

        // GET /api/books/{id}
        get: (id) => fetch(`${BASE}/${id}`).then(handle),

        // POST /api/books
        create: (book) => fetch(BASE, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(book)
        }).then(handle),

        // PUT /api/books/{id}
        update: (id, book) => fetch(`${BASE}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(book)
        }).then(handle),

        // DELETE /api/books/{id}
        remove: (id) => fetch(`${BASE}/${id}`, {
            method: 'DELETE'
        }).then(handle)
    };
})();