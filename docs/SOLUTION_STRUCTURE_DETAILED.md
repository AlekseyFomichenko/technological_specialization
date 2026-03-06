# Подробная структура решения

**Шаблоны проектов:** Server — Worker Service (.NET), Client — Console Application, Shared — Class Library.

---

## Дерево решения

```
Solution (Homeworks.sln)
│
├── Server/                          [Worker Service]
│   ├── Server.csproj
│   ├── Program.cs
│   ├── Worker.cs                    (BackgroundService, запуск TcpListener, передача в ClientConnectionManager)
│   ├── Dockerfile
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Properties/
│   │   └── launchSettings.json
│   │
│   ├── Options/
│   │   ├── FileStorageOptions.cs   (BasePath, AllowedExtensions, MaxFileSizeMb)
│   │   ├── ConnectionLimitOptions.cs
│   │   └── ServerSessionOptions.cs (InactivityTimeoutMinutes)
│   │
│   ├── Services/
│   │   ├── Abstracts/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IFileStorage.cs
│   │   │   ├── IFileTransferService.cs
│   │   │   ├── IMessageService.cs
│   │   │   ├── IMessageDelivery.cs
│   │   │   ├── IPasswordHasher.cs
│   │   │   ├── ILoginAttemptTracker.cs
│   │   │   ├── IConnectionAcceptPolicy.cs
│   │   │   ├── IClientConnectionManager.cs
│   │   │   ├── IClientSessionFactory.cs
│   │   │   └── (прочие интерфейсы сервисов)
│   │   ├── AuthService.cs
│   │   ├── ClientConnectionManager.cs
│   │   ├── ClientSessionFactory.cs
│   │   ├── MessageService.cs
│   │   ├── FileTransferService.cs
│   │   ├── LocalFileStorage.cs     (реализация IFileStorage, путь из конфигурации BasePath)
│   │   ├── BcryptPasswordHasher.cs
│   │   ├── LoginAttemptTracker.cs  (rate limit по IP/логину, модель угроз)
│   │   ├── ConnectionAcceptPolicy.cs (лимиты соединений по IP и общий, модель угроз)
│   │   ├── ErrorCodes.cs
│   │   ├── StartFileResult.cs, WriteChunkResult.cs, EndFileResult.cs
│   │   ├── LoginResult.cs, RegisterResult.cs, SendMessageResult.cs
│   │   └── ...
│   │
│   ├── Models/
│   │   ├── User.cs                  (entity: Login, PasswordHash, CreatedAt)
│   │   ├── Message.cs               (entity: SenderLogin, ReceiverLogin, Content, CreatedAt, IsDelivered)
│   │   ├── Session.cs               (entity: UserId, Token, ExpiresAt)
│   │   ├── FileMetadata.cs          (entity: Id, SenderLogin, ReceiverLogin, FileName, FilePath, FileSize, CreatedAt)
│   │   └── ClientSessionState.cs    (enum: Disconnected, Connected, AwaitingAuth, Authenticated, ReceivingFile, SendingFile, Blocked, Terminated — TRD)
│   │
│   ├── Data/
│   │   ├── Abstracts/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ISessionRepository.cs
│   │   │   ├── IMessageRepository.cs
│   │   │   └── IFileMetadataRepository.cs
│   │   ├── ChatDbContext.cs         (EF Core, Npgsql)
│   │   ├── UserRepository.cs
│   │   ├── SessionRepository.cs
│   │   ├── MessageRepository.cs
│   │   ├── FileMetadataRepository.cs
│   │   └── Migrations/
│   │       └── (миграции EF Core)
│   │
│   └── Protocol/
│       └── ClientSession.cs         (сессия: состояние, Stream, PacketReader/PacketWriter из Shared, диспетчеризация пакетов по состоянию — TRD)
│
├── Client/                          [Console Application]
│   ├── Client.csproj
│   ├── Program.cs                   (точка входа, LoggerFactory, AppSession, запуск MainMenu → ChatScreen)
│   ├── appsettings.json
│   │
│   ├── Options/
│   │   ├── ClientOptions.cs         (ServerAddress, ServerPort, FileChunkSizeBytes, DownloadsPath)
│   │   └── ClientOptionsLoader.cs   (загрузка из args и конфигурации)
│   │
│   ├── UI/
│   │   ├── MainMenu.cs              (меню регистрации/логина, выбор получателя)
│   │   └── ChatScreen.cs            (ввод сообщений, команды /to, /file, /me, /exit, /help, отображение входящих)
│   │
│   ├── Services/
│   │   ├── AppSession.cs            (подключение TcpClient, создание PacketReader/PacketWriter из Shared, AuthClient, ChatClient, FileClient, ConnectionLoop)
│   │   ├── ConnectionLoop.cs        (чтение пакетов, диспетчеризация по MessageType, PendingResponse, обработка Error/обрыв соединения)
│   │   ├── PendingResponse.cs       (ожидание Ack/Error после отправки запроса)
│   │   ├── SessionContext.cs        (хранение токена и логина после аутентификации)
│   │   ├── AuthClient.cs            (запросы Register/Login, получение токена)
│   │   ├── ChatClient.cs            (отправка текстовых сообщений, приём в реальном времени)
│   │   ├── FileClient.cs            (отправка/приём файлов по протоколу, сохранение в DownloadsPath)
│   │   ├── LoginResult.cs, RegisterResult.cs, SendMessageResult.cs, SendFileResult.cs, PendingResponseResult.cs
│   │   └── ...
│   │
│   └── Protocol/
│       └── ClientProtocolConstants.cs (JsonOptions для сериализации DTO на клиенте)
│
└── Shared/                          [Class Library]
    ├── Shared.csproj
    │
    ├── DTO/
    │   ├── RegisterRequest.cs       (payload 0x01)
    │   ├── LoginRequest.cs          (payload 0x02)
    │   ├── LoginResponse.cs         (ответ с токеном)
    │   ├── TextMessagePayload.cs    (payload 0x03: token, receiver, content)
    │   ├── IncomingTextPayload.cs   (входящее текстовое сообщение: sender, content)
    │   ├── FileStartPayload.cs      (payload 0x04: token, receiverLogin, fileName, fileSize)
    │   ├── FileAvailablePayload.cs  (уведомление получателю: fileId, senderLogin, fileName, fileSize)
    │   ├── ErrorPayload.cs          (payload 0x07: code, message)
    │   └── AckPayload.cs            (payload 0x08). FileChunk (0x05) — бинарные данные без отдельного DTO; FileEnd (0x06) — пустой payload.
    │
    ├── Models/
    │   └── MessageType.cs            (enum 0x01–0x09: Register, Login, TextMessage, FileStart, FileChunk, FileEnd, Error, Ack, FileStartAck)
    │
    └── Protocol/
        ├── PacketFormat.cs           (константы: HeaderSize, MaxPayloadSize 50 МБ, описание формата [1 байт тип, 4 байта длина])
        ├── PacketReader.cs           (чтение пакетов из Stream: тип, длина, payload; лимит длины payload)
        ├── PacketWriter.cs           (запись пакетов в Stream)
        └── ProtocolException.cs     (исключение протокола)
```

---

## Назначение папок и ключевых файлов

### Server (Worker Service)

| Папка/файл | Назначение |
|------------|------------|
| **Program.cs** | Host.CreateApplicationBuilder, регистрация сервисов в DI, Configure для FileStorageOptions, ConnectionLimitOptions, ServerSessionOptions, миграции при старте, Run. |
| **Worker.cs** | BackgroundService: в ExecuteAsync — запуск TcpListener, цикл AcceptTcpClientAsync, создание ClientSession через IClientSessionFactory и передача в ClientConnectionManager. |
| **Options/** | Классы конфигурации: FileStorage (BasePath, AllowedExtensions, MaxFileSizeMb), Limits (соединения, rate limit), Server (InactivityTimeoutMinutes). |
| **Services/** | Интерфейсы в Services/Abstracts/ (IAuthService, IFileStorage, IFileTransferService, IMessageDelivery, IConnectionAcceptPolicy, ILoginAttemptTracker, IClientSessionFactory и др.). Реализации в Services/: AuthService (регистрация, логин, BCrypt, токены — генерация/проверка внутри сервиса и Session), ClientConnectionManager (приём соединений, ConcurrentDictionary сессий, IConnectionAcceptPolicy), MessageService, FileTransferService (проверка размера/расширения, IFileStorage, IFileMetadataRepository), LocalFileStorage (путь из конфигурации BasePath, безопасное имя GUID + расширение), LoginAttemptTracker, ConnectionAcceptPolicy, BcryptPasswordHasher. Типы результатов и ErrorCodes. |
| **Models/** | Entity-модели для EF Core и enum ClientSessionState (TRD). |
| **Data/** | Интерфейсы репозиториев в Data/Abstracts/. ChatDbContext, реализации репозиториев на EF Core (только параметризованные запросы/LINQ). Migrations. |
| **Protocol/** | ClientSession — состояние сессии, создаёт PacketReader и PacketWriter из Shared.Protocol по переданному Stream, переходы по TRD, вызов AuthService/MessageService/FileTransferService в зависимости от типа пакета и состояния. |

### Client (Console Application)

| Папка/файл | Назначение |
|------------|------------|
| **Program.cs** | Точка входа: LoggerFactory с AddConsole(), создание AppSession(options, loggerFactory), подключение, MainMenu → ChatScreen, обработка OnDisconnected. |
| **Options/** | ClientOptions (адрес/порт сервера, FileChunkSizeBytes, DownloadsPath). ClientOptionsLoader — загрузка из аргументов и appsettings.json. |
| **UI/** | MainMenu — меню регистрации/логина, выбор получателя. ChatScreen — ввод сообщений, команды /to, /file, /me, /exit, /help, вывод входящих. Вызов AuthClient, ChatClient, FileClient через AppSession. |
| **Services/** | AppSession — установка TcpClient, создание PacketReader/PacketWriter (Shared.Protocol) по Stream, PendingResponse, AuthClient, ChatClient, FileClient, ConnectionLoop; StartReadLoop, OnDisconnected. ConnectionLoop — цикл ReadPacketAsync, диспетчеризация по MessageType (TextMessage, FileStart/Chunk/End, Ack, FileStartAck, Error), логирование ошибок. PendingResponse — ожидание Ack/Error после отправки. SessionContext — хранение токена и логина. AuthClient, ChatClient, FileClient — запросы к серверу и приём ответов. Типы результатов (LoginResult, SendFileResult и т.д.). |
| **Protocol/** | ClientProtocolConstants — JsonOptions для сериализации/десериализации DTO. PacketReader/PacketWriter берутся из Shared. |

### Shared (Class Library)

| Папка/файл | Назначение |
|------------|------------|
| **DTO/** | Классы payload для сериализации (JSON для текста, метаданных файла, ошибок, ack). IncomingTextPayload, FileAvailablePayload. FileChunk (0x05) — бинарные данные без отдельного DTO. Общие для Server и Client. |
| **Models/** | MessageType (enum 0x01–0x09, включая FileStartAck). Общие контракты протокола. |
| **Protocol/** | PacketFormat — константы формата пакета (HeaderSize, MaxPayloadSize 50 МБ). PacketReader, PacketWriter — чтение/запись пакетов по Stream. ProtocolException. Используются и Server (ClientSession), и Client (AppSession, ConnectionLoop). |

---

## Зависимости между проектами

- **Server** → Shared: использование DTO, MessageType, PacketReader, PacketWriter, PacketFormat, ProtocolException.
- **Client** → Shared: использование DTO, MessageType, PacketReader, PacketWriter, PacketFormat, ProtocolException, ClientProtocolConstants (локально) для JsonOptions.
- **Server** не ссылается на Client. **Client** не ссылается на Server. PacketReader и PacketWriter реализованы в Shared и используются обоими проектами.

---

## Регистрация в DI (Server)

В **Program.cs** регистрируются:

- AddDbContext&lt;ChatDbContext&gt; (ConnectionString из конфигурации).
- Configure: FileStorageOptions (секция "FileStorage"), ConnectionLimitOptions ("Limits"), ServerSessionOptions ("Server").
- Репозитории: IUserRepository → UserRepository, ISessionRepository → SessionRepository, IMessageRepository → MessageRepository, IFileMetadataRepository → FileMetadataRepository (Scoped).
- IFileStorage → LocalFileStorage, IPasswordHasher → BcryptPasswordHasher (Singleton).
- ILoginAttemptTracker → LoginAttemptTracker, IConnectionAcceptPolicy → ConnectionAcceptPolicy (Singleton).
- IClientSessionFactory → ClientSessionFactory (Scoped).
- ClientConnectionManager (Singleton), IClientConnectionManager и IMessageDelivery — фасады над тем же экземпляром ClientConnectionManager.
- IAuthService → AuthService, IMessageService → MessageService, IFileTransferService → FileTransferService (Scoped).
- AddHostedService&lt;Worker&gt;.

При старте: создаётся scope, выполняется Database.Migrate(), затем host.Run().

---

