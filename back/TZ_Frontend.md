# ТЗ для Фронтенда — Delivery Aggregator

Бекенд: `http://localhost:5000`
Все защищённые запросы требуют заголовок: `Authorization: Bearer <token>`

---

## Общие вещи (сделать один раз)

### Хранение авторизации
```js
// После логина сохранить:
localStorage.setItem('token', response.token)
localStorage.setItem('role', response.role)        // Customer / Courier / OrganizationOwner / Moderator
localStorage.setItem('userId', response.userId)
localStorage.setItem('displayName', response.displayName)
```

### Axios интерцептор (добавить токен автоматически)
```js
axios.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// При 401 — редирект на логин
axios.interceptors.response.use(null, error => {
  if (error.response?.status === 401) {
    localStorage.clear()
    window.location.href = '/login'
  }
  return Promise.reject(error)
})
```

### Роутинг по роли
После логина смотреть на `role` и редиректить:
- `Customer`          → `/customer`
- `Courier`           → `/courier`
- `OrganizationOwner` → `/restaurant`
- `Moderator`         → `/moderator`

---

## СТРАНИЦА 1 — Логин `/login`

**Что показать:** форма с полями Email + Пароль, кнопка "Войти"

**Запрос:**
```
POST /api/auth/login
{ "email": "...", "password": "..." }
```

**Ответ:**
```json
{ "token": "...", "role": "Customer", "userId": "uuid", "displayName": "Иван" }
```

**После успеха:** сохранить в localStorage, редирект по роли.
**При ошибке:** показать `{ "error": "Неверный email или пароль" }`

---

## СТРАНИЦА 2 — Регистрация покупателя `/register`

> Только покупатели регистрируются сами. Курьеры и владельцы — через форму заявки.

**Что показать:** поля Email, Пароль, Имя, Контакты (опционально)

**Запрос:**
```
POST /api/auth/register
{ "email": "...", "password": "...", "displayName": "Иван Иванов", "contactInfo": "+7..." }
```

**После успеха:** то же что и логин — сохранить токен, редирект на `/customer`

---

## СТРАНИЦА 3 — Форма заявки `/apply`

> Для тех кто хочет стать курьером или зарегистрировать организацию. Без авторизации.

**Что показать:**
- Поле Email
- Поле "Имя / Название организации"
- Выбор роли: `Courier` или `OrganizationOwner`
- Кнопка "Отправить заявку"

**Запрос:**
```
POST /api/applications
{ "email": "...", "displayName": "...", "role": "Courier" }
// role: "Courier" или "OrganizationOwner"
```

**После успеха:** показать сообщение "Заявка принята, с вами свяжется модератор"

---

---

# КАБИНЕТ ПОКУПАТЕЛЯ `/customer`

## Экран 1 — Список ресторанов `/customer/restaurants`

**Запрос (без авторизации):**
```
GET /api/organizations/restaurants
```

**Ответ — массив:**
```json
[{
  "id": "uuid",
  "orgId": "uuid",
  "orgName": "Додо Пицца",
  "name": "Додо на Ленина",
  "address": "ул. Ленина, 1",
  "lat": 55.75, "lng": 37.61,
  "deliveryRadius": 5,
  "isActive": true
}]
```

**Что показать:** карточки ресторанов с названием, адресом. Клик → переход на меню.

---

## Экран 2 — Меню ресторана `/customer/menu/:orgId`

**Запрос (без авторизации):**
```
GET /api/menu/:orgId
```

**Ответ:**
```json
{
  "orgId": "uuid",
  "orgName": "Додо Пицца",
  "categories": [
    {
      "category": "Пицца",
      "items": [
        { "id": "uuid", "name": "Пепперони", "description": "...", "price": 499, "photoUrl": null, "isAvailable": true }
      ]
    }
  ]
}
```

**Что показать:** блюда сгруппированные по категориям, кнопка "Добавить в корзину", счётчик количества.

> Корзину хранить локально (useState/localStorage) — это просто массив `{ menuItemId, quantity }`.

---

## Экран 3 — Оформление заказа `/customer/checkout`

**Что показать:** итог корзины + поле "Адрес доставки"

**Запрос:**
```
POST /api/orders
Authorization: Bearer ...

{
  "restaurantId": "uuid",
  "deliveryAddress": "ул. Пушкина, 5, кв. 12",
  "deliveryLat": null,
  "deliveryLng": null,
  "items": [
    { "menuItemId": "uuid", "quantity": 2 },
    { "menuItemId": "uuid", "quantity": 1 }
  ]
}
```

**Ответ:** объект заказа с `id` и `status: "Pending"`

**После успеха:** редирект на экран отслеживания `/customer/orders/:id`

---

## Экран 4 — Отслеживание заказа `/customer/orders/:id`

**Запрос:**
```
GET /api/orders/:id
Authorization: Bearer ...
```

**Ответ:**
```json
{
  "id": "uuid",
  "status": "Pending",
  "restaurantName": "Додо на Ленина",
  "deliveryAddress": "ул. Пушкина, 5",
  "totalPrice": 747,
  "items": [{ "name": "Пепперони", "quantity": 1, "unitPrice": 499, "subtotal": 499 }]
}
```

**Статусы для отображения:**
| status | Что показать покупателю |
|---|---|
| `Pending` | "Ждём подтверждения ресторана..." |
| `Confirmed` | "Ресторан принял заказ, готовим" |
| `ReadyForPickup` | "Готово! Ищем курьера" |
| `InDelivery` | "Курьер забрал, едет к вам" |
| `Delivered` | "Доставлено! Приятного аппетита" |
| `Cancelled` | "Заказ отменён" |

**SignalR — обновлять статус в реальном времени:**
```js
await connection.invoke('SubscribeToOrder', orderId)
connection.on('OrderStatusChanged', ({ status }) => {
  setStatus(status) // обновить UI без перезагрузки
})
```

---

## Экран 5 — История заказов `/customer/orders`

**Запрос:**
```
GET /api/orders
Authorization: Bearer ...  (роль Customer)
```

**Что показать:** список заказов с датой, суммой, статусом. Клик → экран отслеживания.

---

---

# КАБИНЕТ КУРЬЕРА `/courier`

## Экран 1 — Главная / Статус смены

**Что показать:** кнопка "Начать смену" или "Завершить смену" (зависит от текущего состояния)

**Начать смену:**
```
POST /api/courier/shift/start
Authorization: Bearer ...
```

**Завершить смену:**
```
POST /api/courier/shift/end
Authorization: Bearer ...
```
> Нельзя завершить если есть активный заказ — покажи ошибку из ответа.

---

## Экран 2 — Доступные заказы `/courier/available`

> Показывается только когда курьер на смене.

**Запрос:**
```
GET /api/courier/orders/available
Authorization: Bearer ...
```

**Ответ (ОГРАНИЧЕННЫЕ данные — адрес клиента скрыт!):**
```json
[{
  "id": "uuid",
  "restaurantName": "Додо на Ленина",
  "restaurantAddress": "ул. Ленина, 1",
  "restaurantLat": 55.75,
  "restaurantLng": 37.61,
  "totalPrice": 747
}]
```

**Что показать:** карточки с названием ресторана, откуда забирать, сумма заказа. Кнопка "Принять".

**SignalR — список обновляется автоматически:**
```js
// Новый заказ появился
connection.on('OrderAvailable', (order) => {
  setOrders(prev => [...prev, order])
})
// Заказ взял другой курьер — убрать из списка
connection.on('OrderTaken', ({ orderId }) => {
  setOrders(prev => prev.filter(o => o.id !== orderId))
})
```

---

## Экран 3 — Принять заказ → детали

**Запрос:**
```
POST /api/courier/orders/:id/accept
Authorization: Bearer ...
```

**Ответ (полные данные, включая адрес клиента):**
```json
{
  "id": "uuid",
  "restaurantName": "Додо на Ленина",
  "restaurantAddress": "ул. Ленина, 1",
  "deliveryAddress": "ул. Пушкина, 5, кв. 12",
  "deliveryLat": 55.75,
  "deliveryLng": 37.61,
  "customerContact": "+7 999 777 88 99",
  "totalPrice": 747,
  "status": "InDelivery",
  "items": [...]
}
```

> Если заказ уже взял другой курьер — сервер вернёт `400 "Заказ уже принят другим курьером"`, показать уведомление.

**Что показать:** полный адрес клиента, контакт, список блюд, кнопка "Доставлено".

---

## Экран 4 — Завершить доставку

**Запрос:**
```
POST /api/courier/orders/:id/complete
Authorization: Bearer ...
```

**После успеха:** редирект обратно на экран доступных заказов.

---

## Экран 5 — Текущий заказ (если зашёл заново) `/courier/current`

```
GET /api/courier/orders/current
Authorization: Bearer ...
```

Если нет активного заказа — вернёт `{ "message": "Нет активного заказа" }`, показать экран доступных заказов.

---

## Экран 6 — История доставок `/courier/history`

```
GET /api/courier/orders/history
Authorization: Bearer ...
```

Список выполненных заказов с датой и суммой.

---

---

# КАБИНЕТ РЕСТОРАНА (OrganizationOwner) `/restaurant`

## Экран 1 — Информация об организации

```
GET /api/organizations/my
Authorization: Bearer ...
```

**Ответ:**
```json
{
  "id": "uuid",
  "name": "Додо Пицца",
  "isBlocked": false,
  "restaurants": [
    { "id": "uuid", "name": "Додо на Ленина", "address": "...", "isActive": true }
  ]
}
```

---

## Экран 2 — Управление точками (ресторанами)

**Создать точку:**
```
POST /api/organizations/restaurants
Authorization: Bearer ...

{ "name": "Додо ТЦ Мега", "address": "...", "lat": null, "lng": null, "deliveryRadius": 5 }
```

**Обновить:**
```
PUT /api/organizations/restaurants/:id
{ "name": "...", "address": "...", "isActive": true/false, ... }
```

**Удалить:**
```
DELETE /api/organizations/restaurants/:id
```

---

## Экран 3 — Управление меню `/restaurant/menu`

**Получить меню:**
```
GET /api/menu/:orgId
```

**Добавить блюдо:**
```
POST /api/menu
Authorization: Bearer ...

{ "name": "Пепперони", "category": "Пицца", "description": "...", "price": 499, "photoUrl": null }
```

**Редактировать:**
```
PUT /api/menu/:id
{ "name": "...", "category": "...", "price": 499, "isAvailable": true/false, ... }
```

**Удалить:**
```
DELETE /api/menu/:id
```

---

## Экран 4 — Входящие заказы `/restaurant/orders`

```
GET /api/orders
Authorization: Bearer ...  (роль OrganizationOwner)
```

**SignalR — новый заказ в реальном времени:**
```js
await connection.invoke('SubscribeToOrg', orgId)
connection.on('NewOrder', ({ orderId }) => {
  // подгрузить детали заказа и добавить в список
})
```

---

## Экран 5 — Управление статусом заказа

Ресторан двигает заказ по цепочке:

```
Pending → Confirmed → ReadyForPickup → Cancelled (в любой момент)
```

**Запрос:**
```
PATCH /api/orders/:id/status
Authorization: Bearer ...

{ "status": "Confirmed" }
```

**Кнопки на карточке заказа:**
- Статус `Pending` → кнопка **"Принять заказ"** (→ `Confirmed`)
- Статус `Confirmed` → кнопка **"Готово к выдаче"** (→ `ReadyForPickup`)
- Любой статус до `InDelivery` → кнопка **"Отменить"** (→ `Cancelled`)

---

---

# КАБИНЕТ МОДЕРАТОРА `/moderator`

## Экран 1 — Заявки на регистрацию

```
GET /api/moderator/applications?pendingOnly=true
Authorization: Bearer ...
```

**Ответ:**
```json
[{
  "id": "uuid",
  "email": "newcourier@test.com",
  "displayName": "Сергей Новиков",
  "role": "Courier",
  "status": "Pending",
  "createdAt": "2026-04-04T10:00:00Z"
}]
```

**Одобрить/отклонить:**
```
PATCH /api/moderator/applications/:id
{ "status": "Approved", "moderatorNote": null }
// или
{ "status": "Rejected", "moderatorNote": "Причина отказа" }
```

---

## Экран 2 — Создание пользователя (после одобрения заявки)

После нажатия "Одобрить" открыть форму для создания аккаунта:

```
POST /api/moderator/users
Authorization: Bearer ...

{
  "applicationId": "uuid",
  "email": "courier@example.com",
  "password": "TempPass123",
  "displayName": "Сергей Новиков",
  "contactInfo": null,
  "role": "Courier",       // "Courier" или "OrganizationOwner"
  "workZone": "Центр"      // только для курьера, иначе null
}
```

После создания — показать сгенерированные логин/пароль, чтобы модератор мог их выслать человеку.

---

## Экран 3 — Курьеры

```
GET /api/moderator/couriers
Authorization: Bearer ...
```

**Заблокировать / разблокировать:**
```
PATCH /api/moderator/couriers/:id/block
{ "isBlocked": true }
```

---

## Экран 4 — Организации

```
GET /api/moderator/organizations
Authorization: Bearer ...
```

**Заблокировать:**
```
PATCH /api/moderator/organizations/:id/block
{ "isBlocked": true }
```

---

## Экран 5 — Все заказы

```
GET /api/moderator/orders
Authorization: Bearer ...
```

---

---

# SignalR — итого для фронта

**Установить пакет:**
```bash
npm install @microsoft/signalr
```

**Подключение:**
```js
import * as signalR from '@microsoft/signalr'

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5000/hubs/orders', {
    accessTokenFactory: () => localStorage.getItem('token')
  })
  .withAutomaticReconnect()
  .build()

await connection.start()
```

**События по роли:**

| Роль | invoke (подписка) | on (получить событие) |
|---|---|---|
| Покупатель | `SubscribeToOrder(orderId)` | `OrderStatusChanged` |
| Курьер | ничего (автоматически) | `OrderAvailable`, `OrderTaken` |
| Ресторан | `SubscribeToOrg(orgId)` | `NewOrder` |

---

# Тестовые аккаунты (пока нет БД)

| Email | Пароль | Роль |
|---|---|---|
| moderator@test.com | Admin123 | Модератор |
| owner@dodopizza.ru | Owner123 | Владелец орги |
| courier@test.com | Courier123 | Курьер |
| customer@test.com | Customer123 | Покупатель |
