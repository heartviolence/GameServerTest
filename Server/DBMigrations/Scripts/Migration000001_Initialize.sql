IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Accounts] (
    [Id] uniqueidentifier NOT NULL,
    [UID] int NOT NULL,
    [LoginId] nvarchar(450) NOT NULL,
    [LoginPassword] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Characters] (
    [Id] uniqueidentifier NOT NULL,
    [BaseCharacterCode] nvarchar(max) NOT NULL,
    [OwnerId] uniqueidentifier NOT NULL,
    [Level] int NOT NULL,
    [EXP] int NOT NULL,
    CONSTRAINT [PK_Characters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Characters_Accounts_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [GameWorlds] (
    [Id] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_GameWorlds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GameWorlds_Accounts_Id] FOREIGN KEY ([Id]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Items] (
    [Id] uniqueidentifier NOT NULL,
    [BaseItemCode] nvarchar(max) NOT NULL,
    [Count] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [OwnerId] uniqueidentifier NULL,
    [Discriminator] nvarchar(max) NOT NULL,
    [GameAccountId] uniqueidentifier NULL,
    [EXP] int NULL,
    CONSTRAINT [PK_Items] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Items_Accounts_GameAccountId] FOREIGN KEY ([GameAccountId]) REFERENCES [Accounts] ([Id]),
    CONSTRAINT [FK_Items_Characters_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Characters] ([Id])
);
GO

CREATE TABLE [CompletedAchievement] (
    [Id] uniqueidentifier NOT NULL,
    [AchievementCode] nvarchar(max) NOT NULL,
    [GameWorldDataId] uniqueidentifier NULL,
    CONSTRAINT [PK_CompletedAchievement] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompletedAchievement_GameWorlds_GameWorldDataId] FOREIGN KEY ([GameWorldDataId]) REFERENCES [GameWorlds] ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Accounts_LoginId] ON [Accounts] ([LoginId]);
GO

CREATE INDEX [IX_Characters_OwnerId] ON [Characters] ([OwnerId]);
GO

CREATE INDEX [IX_CompletedAchievement_GameWorldDataId] ON [CompletedAchievement] ([GameWorldDataId]);
GO

CREATE INDEX [IX_Items_GameAccountId] ON [Items] ([GameAccountId]);
GO

CREATE INDEX [IX_Items_OwnerId] ON [Items] ([OwnerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230911130610_Initialize', N'8.0.1');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Items] DROP CONSTRAINT [FK_Items_Characters_OwnerId];
GO

DROP INDEX [IX_Items_OwnerId] ON [Items];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'OwnerId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Items] SET [OwnerId] = '00000000-0000-0000-0000-000000000000' WHERE [OwnerId] IS NULL;
ALTER TABLE [Items] ALTER COLUMN [OwnerId] uniqueidentifier NOT NULL;
ALTER TABLE [Items] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [OwnerId];
CREATE INDEX [IX_Items_OwnerId] ON [Items] ([OwnerId]);
GO

ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_Accounts_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230911132954_Initialize2', N'8.0.1');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230911133540_Initialize3', N'8.0.1');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [CompletedAchievement] DROP CONSTRAINT [FK_CompletedAchievement_GameWorlds_GameWorldDataId];
GO

DROP INDEX [IX_CompletedAchievement_GameWorldDataId] ON [CompletedAchievement];
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompletedAchievement]') AND [c].[name] = N'GameWorldDataId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CompletedAchievement] DROP CONSTRAINT [' + @var1 + '];');
UPDATE [CompletedAchievement] SET [GameWorldDataId] = '00000000-0000-0000-0000-000000000000' WHERE [GameWorldDataId] IS NULL;
ALTER TABLE [CompletedAchievement] ALTER COLUMN [GameWorldDataId] uniqueidentifier NOT NULL;
ALTER TABLE [CompletedAchievement] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [GameWorldDataId];
CREATE INDEX [IX_CompletedAchievement_GameWorldDataId] ON [CompletedAchievement] ([GameWorldDataId]);
GO

ALTER TABLE [CompletedAchievement] ADD CONSTRAINT [FK_CompletedAchievement_GameWorlds_GameWorldDataId] FOREIGN KEY ([GameWorldDataId]) REFERENCES [GameWorlds] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230911133855_Initialize4', N'8.0.1');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [CompletedAchievement] DROP CONSTRAINT [FK_CompletedAchievement_GameWorlds_GameWorldDataId];
GO

EXEC sp_rename N'[CompletedAchievement].[GameWorldDataId]', N'GameWorldId', N'COLUMN';
GO

EXEC sp_rename N'[CompletedAchievement].[IX_CompletedAchievement_GameWorldDataId]', N'IX_CompletedAchievement_GameWorldId', N'INDEX';
GO

ALTER TABLE [CompletedAchievement] ADD CONSTRAINT [FK_CompletedAchievement_GameWorlds_GameWorldId] FOREIGN KEY ([GameWorldId]) REFERENCES [GameWorlds] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231015102534_Initialize5', N'8.0.1');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompletedAchievement]') AND [c].[name] = N'AchievementCode');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [CompletedAchievement] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [CompletedAchievement] ALTER COLUMN [AchievementCode] nvarchar(450) NOT NULL;
GO

CREATE UNIQUE INDEX [IX_CompletedAchievement_AchievementCode_GameWorldId] ON [CompletedAchievement] ([AchievementCode], [GameWorldId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231112095509_AcheivementUnique', N'8.0.1');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Accounts] ADD [AccountLevel] int NOT NULL DEFAULT 1;
GO

ALTER TABLE [Accounts] ADD [Icon] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Accounts] ADD [PlayerName] nvarchar(max) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20231214124019_AddNickname', N'8.0.1');
GO

COMMIT;
GO


