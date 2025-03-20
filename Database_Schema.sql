-- Create the "leaderboard" database
CREATE DATABASE leaderboard;

-- Switch to the "leaderboard" database
USE leaderboard;

-- Setup security
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'LeaderboardLogin')
BEGIN
    CREATE LOGIN LeaderboardLogin WITH PASSWORD = 'P@ssw0rd!';
END

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'LeaderboardUser')
BEGIN
    CREATE USER [LeaderboardUser] FOR LOGIN [LeaderboardLogin];
END

EXEC sp_addrolemember 'db_owner', 'LeaderboardUser';

CREATE TABLE Gamer (
    gamer_id      int NOT NULL IDENTITY(1,1),
    gamer_name    varchar NOT NULL,
    password      varchar NOT NULL,
    highest_score int NOT NULL,
    CONSTRAINT gamer_pk PRIMARY KEY (gamer_id),
    CONSTRAINT gamer_nk UNIQUE (gamer_name)
);

CREATE TABLE Score (
    score_id  int NOT NULL IDENTITY(1,1),
    gamer_id  int NOT NULL,
    date      date NOT NULL,
    score     int NOT NULL,
    CONSTRAINT score_pk PRIMARY KEY (score_id),
    CONSTRAINT score_fk FOREIGN KEY (gamer_id)
    REFERENCES Gamer(gamer_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);


-- Here is an error. Views instead of another table?
CREATE TABLE leaderboadrRank (
    rank_id   int NOT NULL IDENTITY(1,1),
    gamer_id  int NOT NULL,
    score_id  int NOT NULL,
    CONSTRAINT rank_pk PRIMARY KEY (rank_id),
    CONSTRAINT rank_fk_user FOREIGN KEY (gamer_id)
    REFERENCES Gamer(gamer_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
    CONSTRAINT rank_fk_score FOREIGN KEY (score_id)
    REFERENCES Score(score_id)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
);