## Qwen Added Memories
- Дизайн-документ головоломок для CodeQuest:

УРОВЕНЬ 0 (Тренировочный, 1 головоломка):
- «Первая команда»: Print("Hello World"); Print("Game Started") Print("Loading..."); Print("Ready"); — ошибка: нет ; в строке 2. Решение: добавить ; в конец строки 2.

УРОВЕНЬ 1 (INT → DOUBLE/FLOAT, 3 головоломки):
- «Целочисленный замок»: int speed = 5.5; → int speed = 5;
- «Калибровка шестерёнок»: int experience = 7.8; → int experience = 7;
- «Сортировка контейнеров»: int total = score + bonus; → float total = score + bonus;

УРОВЕНЬ 2 (CHAR → STRING + IF, 6 головоломок):
- «Символьный ряд»: char symbol = "Б"; → char symbol = 'Б';
- «Текст в рамке (А)»: string title = 'Quest'; → string title = "Quest";
- «Текст в рамке (Б)»: string label = 42; → string label = "42";
- «Термостат-волк»: char limit = '3'; → int limit = 3;
- «Шлюз управления»: if power > 50 → if (power > 50)
- «Команда без кавычек»: string cmd = Move; → string cmd = "Move";

УРОВЕНЬ 3 (BOOL, УСЛОВИЯ/ЦИКЛЫ + СИНТЕЗ, 7 головоломок):
- «Логический переключатель»: bool isReady = 1; → bool isReady = true;
- «Активация терминала»: if (accessCode == X) → if (accessCode == 'X')
- «Ветвление программы»: else (hp <= 50) → else
- «Цикл повторения»: for int i = 0; → for (int i = 0;
- «Пока условие истинно»: while (active = true) → while (active == true)
- «Счётчик активаций»: int energy = 3; (при цикле 5 раз) → int energy = 5;
- «Финальная калибровка»: char maxCharges = '3'; → int maxCharges = 3;

Каждая головоломка содержит: расположение, код с ошибкой, сообщение об ошибке с подсказкой, инфо-окно с правилом, объяснение логики решения, правильный ответ.
