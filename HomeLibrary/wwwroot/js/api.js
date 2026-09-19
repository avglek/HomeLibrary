// Обёртки над $.ajax для всех обращений к /api/books
const Api = (() => {
  const BASE = "/api/books";

  // Универсальный вызов с единой обработкой ответа
  function request(options) {
    return $.ajax(
      $.extend(
        {
          dataType: "json",
          contentType: "application/json; charset=utf-8",
        },
        options,
      ),
    ).then(
      (data) => data,
      (jqXHR) => {
        // Пытаемся вытащить message из тела ответа
        let message = `Ошибка ${jqXHR.status}`;
        if (jqXHR.responseJSON?.message) {
          message = jqXHR.responseJSON.message;
        } else if (jqXHR.responseText) {
          try {
            const parsed = JSON.parse(jqXHR.responseText);
            message = parsed.message || message;
          } catch (_) {
            /* ignore */
          }
        }
        // Пробрасываем дальше как Error, чтобы .catch() работал единообразно
        return $.Deferred().reject(new Error(message)).promise();
      },
    );
  }

  return {
    // GET /api/books?search=...
    list: (search = "") =>
      request({
        url: BASE,
        method: "GET",
        data: search ? { search } : {},
      }),

    // GET /api/books/{id}
    get: (id) =>
      request({
        url: `${BASE}/${id}`,
        method: "GET",
      }),

    // POST /api/books
    create: (book) =>
      request({
        url: BASE,
        method: "POST",
        data: JSON.stringify(book),
      }),

    // PUT /api/books/{id}
    update: (id, book) =>
      request({
        url: `${BASE}/${id}`,
        method: "PUT",
        data: JSON.stringify(book),
      }),

    // DELETE /api/books/{id}
    remove: (id) =>
      request({
        url: `${BASE}/${id}`,
        method: "DELETE",
      }),
  };
})();
