$(function () {
  // ─────────────────────────────────────────────────────────
  // 1. Инициализация редактора Trumbowyg
  // ─────────────────────────────────────────────────────────
  const $editor = $("#editor");

  $editor.trumbowyg({
    lang: "ru",
    semantic: false,
    btns: [
      ["viewHTML"],
      ["undo", "redo"],
      ["formatting"],
      ["strong", "em", "del"],
      ["superscript", "subscript"],
      ["link"],
      ["insertImage"],
      ["justifyLeft", "justifyCenter", "justifyRight", "justifyFull"],
      ["unorderedList", "orderedList"],
      ["horizontalRule"],
      ["removeformat"],
      ["fullscreen"],
    ],
    autogrow: true,
  });

  // ─────────────────────────────────────────────────────────
  // 2. Ссылки на элементы формы (jQuery-объекты)
  // ─────────────────────────────────────────────────────────
  const $titleEl = $("#title");
  const $authorEl = $("#author");
  const $yearEl = $("#year");
  const $saveBtn = $("#saveBtn");
  const $deleteBtn = $("#deleteBtn");
  const $statusEl = $("#status");
  const $pageTitle = $("#pageTitle");

  // ID книги из URL (?id=5)
  const bookId = new URLSearchParams(location.search).get("id");

  // ─────────────────────────────────────────────────────────
  // 3. Загрузка книги при редактировании
  // ─────────────────────────────────────────────────────────
  function loadBook() {
    if (!bookId) return;

    setStatus("Загрузка…", "loading");
    $pageTitle.text("Редактирование книги");
    $deleteBtn.show();

    Api.get(bookId)
      .then(function (book) {
        $titleEl.val(book.title ?? "");
        $authorEl.val(book.author ?? "");
        $yearEl.val(book.publishYear ?? "");

        // Извлекаем HTML из XML-обёртки и ставим в редактор
        const html = extractTocHtml(book.tocContent);
        $editor.trumbowyg("html", html || "");

        setStatus("", "");
      })
      .catch(function (err) {
        setStatus("Не удалось загрузить книгу: " + err.message, "error");
      });
  }

  // ─────────────────────────────────────────────────────────
  // 4. Сохранение (создание или обновление)
  // ─────────────────────────────────────────────────────────
  function saveBook() {
    const title = ($titleEl.val() || "").trim();
    const author = ($authorEl.val() || "").trim();

    if (!title || !author) {
      setStatus("Заполните обязательные поля: Название и Автор", "error");
      return;
    }

    const editorHtml = $editor.trumbowyg("html") || "";

    // Оборачиваем в XML с CDATA. Экранируем закрывающий ]]> на всякий случай.
    const safe = editorHtml.replace(/\]\]>/g, "]]]]><![CDATA[>");
    const tocXml = `<div><![CDATA[${safe}]]></div>`;

    const payload = {
      title: title,
      author: author,
      publishYear: $yearEl.val() ? parseInt($yearEl.val(), 10) : null,
      tocContent: tocXml,
    };

    setStatus("Сохранение…", "loading");
    $saveBtn.prop("disabled", true);

    const promise = bookId ? Api.update(bookId, payload) : Api.create(payload);

    promise
      .then(function (result) {
        if (bookId) {
          setStatus("Изменения сохранены", "success");
        } else {
          setStatus("Книга создана", "success");
          setTimeout(function () {
            location.href = `/book.html?id=${result.id}`;
          }, 600);
        }
      })
      .catch(function (err) {
        setStatus("Ошибка сохранения: " + err.message, "error");
      })
      .always(function () {
        $saveBtn.prop("disabled", false);
      });
  }

  // ─────────────────────────────────────────────────────────
  // 5. Удаление
  // ─────────────────────────────────────────────────────────
  function deleteBook() {
    if (!bookId) return;
    if (!confirm("Удалить эту книгу?")) return;

    setStatus("Удаление…", "loading");
    Api.remove(bookId)
      .then(function () {
        setStatus("Книга удалена", "success");
        setTimeout(function () {
          location.href = "/";
        }, 500);
      })
      .catch(function (err) {
        setStatus("Ошибка удаления: " + err.message, "error");
      });
  }

  // ─────────────────────────────────────────────────────────
  // 6. Вспомогательные
  // ─────────────────────────────────────────────────────────
  function setStatus(text, cls) {
    $statusEl.text(text || "").attr("class", "status " + (cls || ""));
  }

  // Извлекаем содержимое <div>…</div> как HTML
  function extractTocHtml(tocContent) {
    if (!tocContent) return "";
    try {
      const doc = new DOMParser().parseFromString(
        tocContent,
        "application/xml",
      );
      const toc = doc.querySelector("toc");
      if (toc) return toc.textContent || toc.innerHTML || "";
    } catch (_) {
      /* ignore */
    }

    // Фолбэк через jQuery
    const $tmp = $("<div>").html(tocContent);
    const $toc = $tmp.find("toc");
    return $toc.length ? $toc.text() || $toc.html() || "" : tocContent;
  }

  // ─────────────────────────────────────────────────────────
  // 7. Обработчики событий
  // ─────────────────────────────────────────────────────────
  $saveBtn.on("click", saveBook);
  $deleteBtn.on("click", deleteBook);

  // Ctrl/Cmd + S — быстрое сохранение
  $(document).on("keydown", function (e) {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
      e.preventDefault();
      saveBook();
    }
  });

  // ─────────────────────────────────────────────────────────
  // 8. Старт
  // ─────────────────────────────────────────────────────────
  loadBook();
});
