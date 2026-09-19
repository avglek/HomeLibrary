-- ═══════════════════════════════════════════════════════════════
-- Инициализация БД «Домашняя библиотека»
-- Выполняется автоматически при первом старте контейнера PostgreSQL
-- ═══════════════════════════════════════════════════════════════

-- ───────────────────────────────────────────────────────────────
-- 1. Расширения (на всякий случай; xml встроен в PostgreSQL)
-- ───────────────────────────────────────────────────────────────
-- pgcrypto пригодится, если позже захотите UUID или шифрование
CREATE EXTENSION IF NOT EXISTS pgcrypto;


-- ───────────────────────────────────────────────────────────────
-- 2. Таблица книг
-- ───────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS books (
    id            SERIAL PRIMARY KEY,
    title         VARCHAR(255) NOT NULL,
    author        VARCHAR(255) NOT NULL,
    publish_year  INT          CHECK (publish_year IS NULL OR publish_year BETWEEN 0 AND 2100),
    toc_content   XML          NOT NULL DEFAULT '<toc/>',
    created_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE  books               IS 'Домашняя библиотека: книги';
COMMENT ON COLUMN books.title         IS 'Название книги';
COMMENT ON COLUMN books.author        IS 'Автор книги';
COMMENT ON COLUMN books.publish_year  IS 'Год издания';
COMMENT ON COLUMN books.toc_content   IS 'Оглавление в виде XML-документа';
COMMENT ON COLUMN books.created_at    IS 'Дата добавления записи';
COMMENT ON COLUMN books.updated_at    IS 'Дата последнего изменения';


-- ───────────────────────────────────────────────────────────────
-- 3. Индексы
-- ───────────────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_books_title  ON books (title);
CREATE INDEX IF NOT EXISTS idx_books_author ON books (author);

-- Индекс по году — полезен для сортировки/фильтрации
CREATE INDEX IF NOT EXISTS idx_books_year   ON books (publish_year);

-- GIN-индекс по XML-оглавлению (помогает для contains-запросов)
-- Если возникнут проблемы с производительностью поиска — этот индекс ускорит
CREATE INDEX IF NOT EXISTS idx_books_toc_gin ON books
    USING GIN ( (toc_content::text) gin_trgm_ops );

-- Для gin_trgm_ops нужен pg_trgm
CREATE EXTENSION IF NOT EXISTS pg_trgm;


-- ───────────────────────────────────────────────────────────────
-- 4. Триггер автообновления updated_at
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION trg_books_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS books_set_updated_at ON books;
CREATE TRIGGER books_set_updated_at
    BEFORE UPDATE ON books
    FOR EACH ROW
    EXECUTE FUNCTION trg_books_set_updated_at();


-- ═══════════════════════════════════════════════════════════════
-- 5. Хранимые процедуры
-- ═══════════════════════════════════════════════════════════════

-- ───────────────────────────────────────────────────────────────
-- 5.1 insert_book — создание книги, возвращает новый ID
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION insert_book(
    p_title       VARCHAR,
    p_author      VARCHAR,
    p_year        INT,
    p_toc_content TEXT
) RETURNS INT AS $$
DECLARE
    new_id INT;
    xml_doc XML;
BEGIN
    -- Готовим XML: пустое значение -> <toc/>, иначе парсим
    IF p_toc_content IS NULL OR btrim(p_toc_content) = '' THEN
        xml_doc := xmlparse(document '<toc/>');
    ELSE
        xml_doc := xmlparse(document p_toc_content);
    END IF;

    INSERT INTO books (title, author, publish_year, toc_content)
    VALUES (p_title, p_author, p_year, xml_doc)
    RETURNING id INTO new_id;

    RETURN new_id;
END;
$$ LANGUAGE plpgsql;


-- ───────────────────────────────────────────────────────────────
-- 5.2 update_book — обновление книги по ID
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION update_book(
    p_id          INT,
    p_title       VARCHAR,
    p_author      VARCHAR,
    p_year        INT,
    p_toc_content TEXT
) RETURNS VOID AS $$
DECLARE
    xml_doc XML;
BEGIN
    IF p_toc_content IS NULL OR btrim(p_toc_content) = '' THEN
        xml_doc := xmlparse(document '<toc/>');
    ELSE
        xml_doc := xmlparse(document p_toc_content);
    END IF;

    UPDATE books
    SET title        = p_title,
        author       = p_author,
        publish_year = p_year,
        toc_content  = xml_doc,
        updated_at   = CURRENT_TIMESTAMP
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


-- ───────────────────────────────────────────────────────────────
-- 5.3 delete_book — удаление книги по ID
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION delete_book(p_id INT)
RETURNS VOID AS $$
BEGIN
    DELETE FROM books WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


-- ───────────────────────────────────────────────────────────────
-- 5.4 get_all_books — список всех книг (без оглавления)
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION get_all_books()
RETURNS TABLE(
    id           INT,
    title        VARCHAR,
    author       VARCHAR,
    publish_year INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT b.id, b.title, b.author, b.publish_year
    FROM books b
    ORDER BY b.id DESC;
END;
$$ LANGUAGE plpgsql;


-- ───────────────────────────────────────────────────────────────
-- 5.5 get_book_by_id — одна книга вместе с XML-оглавлением
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION get_book_by_id(p_id INT)
RETURNS TABLE(
    id           INT,
    title        VARCHAR,
    author       VARCHAR,
    publish_year INT,
    toc_content  TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT b.id,
           b.title,
           b.author,
           b.publish_year,
           b.toc_content::text   -- XML -> TEXT для Dapper
    FROM books b
    WHERE b.id = p_id;
END;
$$ LANGUAGE plpgsql;


-- ───────────────────────────────────────────────────────────────
-- 5.6 search_books — поиск по названию, автору и тексту оглавления
-- ───────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION search_books(p_search_term TEXT)
RETURNS TABLE(
    id           INT,
    title        VARCHAR,
    author       VARCHAR,
    publish_year INT
) AS $$
DECLARE
    term TEXT;
BEGIN
    term := btrim(p_search_term);

    IF term = '' THEN
        -- Пустой запрос — возвращаем всё
        RETURN QUERY
        SELECT b.id, b.title, b.author, b.publish_year
        FROM books b
        ORDER BY b.id DESC;
        RETURN;
    END IF;

    RETURN QUERY
    SELECT DISTINCT
           b.id, b.title, b.author, b.publish_year
    FROM books b
    WHERE b.title  ILIKE '%' || term || '%'
       OR b.author ILIKE '%' || term || '%'
       -- Поиск по текстовым узлам XML-оглавления
       OR EXISTS (
           SELECT 1
           FROM unnest(xpath('//text()', b.toc_content)) AS x
           WHERE x::text ILIKE '%' || term || '%'
       )
    ORDER BY b.id DESC;
END;
$$ LANGUAGE plpgsql;


-- ═══════════════════════════════════════════════════════════════
-- 6. Тестовые данные (можно удалить в продакшене)
-- ═══════════════════════════════════════════════════════════════

INSERT INTO books (title, author, publish_year, toc_content) VALUES
(
    'Война и мир',
    'Лев Толстой',
    1869,
    xmlparse(document '<div><h1>Том первый</h1><h2>Часть первая</h2><p>Глава I. Салон Анны Павловны Шерер</p><p>Глава II. Пьер Безухов в салоне</p><h2>Часть вторая</h2><p>Глава I. Смотр под Браунау</p></div>')
),
(
    'Преступление и наказание',
    'Фёдор Достоевский',
    1866,
    xmlparse(document '<div><h1>Часть первая</h1><p>Глава 1. Проба</p><p>Глава 2. Мармеладовы</p><h1>Часть вторая</h1><p>Глава 1. Признание</p></div>')
),
(
    'Мастер и Маргарита',
    'Михаил Булгаков',
    1967,
    xmlparse(document '<div><h1>Часть первая</h1><h2>Глава 1. Никогда не разговаривайте с неизвестными</h2><h2>Глава 2. Понтий Пилат</h2><h1>Часть вторая</h1><h2>Глава 19. Маргарита</h2></div>')
);