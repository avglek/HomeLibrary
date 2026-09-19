$(function () {
  // ─────────────────────────────────────────────────────────
  // 1. Ссылки на элементы
  // ─────────────────────────────────────────────────────────
  const $tbody = $("#booksBody");
  const $statusEl = $("#status");
  const $searchInput = $("#searchInput");
  const $searchBtn = $("#searchBtn");
  const $clearBtn = $("#clearBtn");

  // ─────────────────────────────────────────────────────────
  // 2. Загрузка списка книг
  // ─────────────────────────────────────────────────────────
  function loadBooks(search) {
    search = (search || "").trim();

    setStatus(search ? `Поиск: «${search}»…` : "Загрузка…", "loading");

    Api.list(search)
      .then(function (books) {
        renderBooks(books);

        if (!books || books.length === 0) {
          setStatus("Ничего не найдено", "");
        } else {
          setStatus(`Найдено книг: ${books.length}`, "success");
        }
      })
      .catch(function (err) {
        setStatus("Ошибка загрузки: " + err.message, "error");
        $tbody.empty();
      });
  }

  // ─────────────────────────────────────────────────────────
  // 3. Отрисовка таблицы
  // ─────────────────────────────────────────────────────────
  function renderBooks(books) {
    if (!books || books.length === 0) {
      $tbody.html(
        '<tr class="empty-row">' +
          '<td colspan="5">Книг пока нет. Добавьте первую!</td>' +
          "</tr>",
      );
      return;
    }

    // Собираем HTML одной строкой, чтобы не делать N манипуляций с DOM
    const rows = books
      .map(function (b) {
        const year =
          b.publishYear === null || b.publishYear === undefined
            ? "—"
            : b.publishYear;

        return (
          "" +
          "<tr>" +
          `<td>${b.id}</td>` +
          `<td>${escapeHtml(b.title)}</td>` +
          `<td>${escapeHtml(b.author)}</td>` +
          `<td>${year}</td>` +
          "<td>" +
          `<a class="btn btn-small" href="/book.html?id=${b.id}">Открыть</a> ` +
          `<button class="btn btn-small btn-danger" ` +
          `data-delete="${b.id}" ` +
          `data-title="${escapeHtml(b.title)}">Удалить</button>` +
          "</td>" +
          "</tr>"
        );
      })
      .join("");

    $tbody.html(rows);
  }

  // ─────────────────────────────────────────────────────────
  // 4. Удаление книги (делегирование событий)
  // ─────────────────────────────────────────────────────────
  $tbody.on("click", "[data-delete]", function () {
    const $btn = $(this);
    const id = $btn.data("delete");
    const title = $btn.data("title");

    if (!confirm(`Удалить книгу «${title}»?`)) return;

    $btn.prop("disabled", true);
    setStatus(`Удаление «${title}»…`, "loading");

    Api.remove(id)
      .then(function () {
        setStatus(`Книга «${title}» удалена`, "success");
        loadBooks($searchInput.val());
      })
      .catch(function (err) {
        setStatus("Ошибка удаления: " + err.message, "error");
        $btn.prop("disabled", false);
      });
  });

  // ─────────────────────────────────────────────────────────
  // 5. Вспомогательные
  // ─────────────────────────────────────────────────────────
  function setStatus(text, cls) {
    $statusEl.text(text || "").attr("class", "status " + (cls || ""));
  }

  function escapeHtml(str) {
    return String(str == null ? "" : str)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  // Простейший debounce — чтобы не гонять запрос на каждое нажатие клавиши
  function debounce(fn, delay) {
    let timer = null;
    return function () {
      const args = arguments;
      const ctx = this;
      clearTimeout(timer);
      timer = setTimeout(function () {
        fn.apply(ctx, args);
      }, delay);
    };
  }

  // ─────────────────────────────────────────────────────────
  // 6. Обработчики событий
  // ─────────────────────────────────────────────────────────
  $searchBtn.on("click", function () {
    loadBooks($searchInput.val());
  });

  $clearBtn.on("click", function () {
    $searchInput.val("");
    loadBooks("");
  });

  // Enter в поле поиска
  $searchInput.on("keydown", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      loadBooks($searchInput.val());
    }
  });

  // Живой поиск с debounce 400 мс (опционально)
  $searchInput.on(
    "input",
    debounce(function () {
      loadBooks($searchInput.val());
    }, 400),
  );

  // ─────────────────────────────────────────────────────────
  // 7. Старт
  // ─────────────────────────────────────────────────────────
  loadBooks("");
});
